using DirectProblem.Core.Base;
using DirectProblem.Core.Global;
using InverseProblem.Assembling;

namespace InverseProblem.SLAE;

public class Regularizer
{
    private readonly GaussElimination _gaussElimination;
    public Matrix BufferMatrix { get; set; }
    public Vector BufferVector { get; set; }
    public Vector PreviousSolution { get; set; }


    public Regularizer(GaussElimination gaussElimination)
    {
        _gaussElimination = gaussElimination;

    }
    private Vector CalculateAlpha(Matrix matrix)
    {
        var n = matrix.CountRows;
        var alphas = new Vector(matrix.CountRows);

        for (var i = 0; i < n; i++)
        {
            //alphas[i] += matrix[i, i];
            //alphas[i] /= 10e8; //уточнить
            alphas[i] = 0d;
        }

        return alphas;
    }
    public Vector Regularize(Equation<Matrix> equation, double[] sigmas, double[] previousSigmas, Parameter[] parameters)
    {
        var alphas = CalculateAlpha(equation.Matrix);
        alphas = FindPossibleAlpha(equation, alphas);

        alphas = FindLocalConstraint(sigmas, previousSigmas, alphas);

        alphas = FindGlobalConstraint(equation, alphas, sigmas, parameters);

        return alphas;
    }


    private void AssembleSLAE(Equation<Matrix> equation, Vector alphas)
    {
        Matrix.CreateIdentityMatrix(BufferMatrix);

        Matrix.Sum(equation.Matrix, Matrix.Multiply(alphas, BufferMatrix, BufferMatrix), BufferMatrix);

        BufferVector = equation.RightPart; // нет разности в правой части, потому что равенство
    }

    private double CalculateResidual(Equation<Matrix> equation, double alpha)
    {
        Matrix.CreateIdentityMatrix(BufferMatrix);

        Matrix.Sum(equation.Matrix, Matrix.Multiply(alpha, BufferMatrix, BufferMatrix), BufferMatrix);

        Matrix.Multiply(BufferMatrix, BufferVector, ResidualBufferVector);

        BufferVector = equation.RightPart; // нет разности в правой части, потому что равенство

        return Vector.Subtract(
            BufferVector,
            ResidualBufferVector, BufferVector)
            .Norm;
    }

    private Vector FindLocalConstraint(double[] sigmas, double[] previousSigmas, Vector alphas)
    {
        var ratio = 0d;
        var alphaNumber = 0;

        for (int i = 0; i < sigmas.Length; i++)
        {
            ratio = sigmas[i] / previousSigmas[i];
            ratio = Math.Max(ratio, 1d / ratio);

            if (ratio >= 2)
            {
                alphas[alphaNumber] *= 1.5;
                alphaNumber++;

            }
            if (alphaNumber > alphas.Count) break;

        }

        return alphas;
    }
    private Vector FindGlobalConstraint(Equation<Matrix> equation, Vector alphas, double[] sigmas, Parameter[] parameters)
    {
        var sum = 0d;
        for (int i = 0; i < parameters.Length; i++)
        {
            sum = sigmas[parameters[i].Index] + BufferVector[i];

            if (sum is >= 5 or <= 1e-3)
            {
                alphas[i] *= 1.5;
                break;
            }
        }
        return alphas;
    }
    private Vector FindPossibleAlpha(Equation<Matrix> equation, Vector alphas)
    {
        for (; ; )
        {
            try
            {
                AssembleSLAE(equation, alphas);

                BufferVector = _gaussElimination.Solve(BufferMatrix, BufferVector);

                break;
            }
            catch
            {
                for (var i = 0; i < alphas.Count; i++)
                {
                    alphas[i] *= 1.5;
                }
            }
        }

        return alphas;
    }

    private Vector FindBestAlpha(Equation<Matrix> equation, Vector alphas)
    {
        equation.Solution.Copy(PreviousSolution);                     

        do
        {
            AssembleSLAE(equation, alphas);

            BufferVector = _gaussElimination.Solve(BufferMatrix, BufferVector);

            
            ratio = Math.Max(ratio, 1d / ratio);

            if (ratio >= 2)
            {
                alphas[alphaNumber] *= 1.5;
                alphaNumber++;

            }

        } while (ratio is < 1.999d or > 3d);

        return alphas;
    }
}