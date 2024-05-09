using DirectProblem.Core.Base;
using DirectProblem.Core.Global;
using InverseProblem.Assembling;

namespace InverseProblem.SLAE;

public class Regularizer
{
    private readonly GaussElimination _gaussElimination;
    public Matrix BufferMatrix { get; set; }
    public Vector BufferVector { get; set; }
    private readonly Vector _previousSolution;


    public Regularizer(GaussElimination gaussElimination, Parameter[] parameters)
    {
        _gaussElimination = gaussElimination;
        _previousSolution = new Vector(parameters.Length);

    }
    private Vector CalculateAlpha(Matrix matrix)
    {
        var n = matrix.CountRows;
        var alphas = new Vector(matrix.CountRows);

        for (var i = 0; i < n; i++)
        {
            alphas[i] = matrix[i, i] * 10e-8;
            //alphas[i] = 0d;
        }

        return alphas;
    }
    public Vector Regularize(Equation<Matrix> equation)
    {
        var alphas = CalculateAlpha(equation.Matrix);
        alphas = FindPossibleAlphas(equation, alphas);
        alphas = FindBestAlphas(equation, alphas);

        return alphas;
    }


    private void AssembleSLAE(Equation<Matrix> equation, Vector alphas)
    {
        Matrix.CreateIdentityMatrix(BufferMatrix);

        Matrix.Sum(equation.Matrix, Matrix.Multiply(alphas, BufferMatrix, BufferMatrix), BufferMatrix);

        equation.RightPart.Copy(BufferVector); // нет разности в правой части, потому что равенство
    }

    //private double CalculateResidual(Equation<Matrix> equation, double alpha)
    //{
    //    Matrix.CreateIdentityMatrix(BufferMatrix);

    //    Matrix.Sum(equation.Matrix, Matrix.Multiply(alpha, BufferMatrix, BufferMatrix), BufferMatrix);

    //    Matrix.Multiply(BufferMatrix, BufferVector, ResidualBufferVector);

    //    BufferVector = equation.RightPart; // нет разности в правой части, потому что равенство

    //    return Vector.Subtract(
    //        BufferVector,
    //        ResidualBufferVector, BufferVector)
    //        .Norm;
    //}

    private bool CheckLocalConstraints(double changeRatio)
    {
        return !(Math.Max(1d / changeRatio, changeRatio) > 2d);

    }
    private bool CheckGlobalConstraints(double parameterValue)
    {
        return parameterValue is >= 1e-3 and <= 5d;
    }
    private Vector FindPossibleAlphas(Equation<Matrix> equation, Vector alphas)
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

    private Vector FindBestAlphas(Equation<Matrix> equation, Vector alphas)
    {
        bool stop;

        equation.Solution.Copy(_previousSolution);

        do
        {
            AssembleSLAE(equation, alphas);

            BufferVector = _gaussElimination.Solve(BufferMatrix, BufferVector);

            alphas = ChangeAlphas(equation, alphas, out stop);

        } while (!stop);

        return alphas;        
    }
    private Vector ChangeAlphas(Equation<Matrix> equation, Vector alphas, out bool stop)
    {
        stop = true;

        Vector.Sum(equation.Solution, BufferVector,
            BufferVector);

        for (var i = 0; i < alphas.Count; i++)
        {
            var changeRatio = equation.Solution[i] / _previousSolution[i];

            if (CheckLocalConstraints(changeRatio) &&
                CheckGlobalConstraints(BufferVector[i])) continue;

            Console.WriteLine("Constraints weren't passed                          \r");

            alphas[i] *= 1.5;

            Console.WriteLine($"alpha{i} increased to {alphas[i]}                          \r");

            stop = false;
        }

        BufferVector.Copy(_previousSolution);

        return alphas;
    }

}