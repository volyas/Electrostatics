﻿using DirectProblem.Core.Base;
using DirectProblem.Core.Global;
using InverseProblem.Assembling;

namespace InverseProblem.SLAE;

public class Regularizer
{
    private readonly GaussElimination _gaussElimination;
    public Matrix BufferMatrix { get; set; }
    public Vector BufferVector { get; set; }
    public Vector ResidualBufferVector { get; set; }

    public Regularizer(GaussElimination gaussElimination)
    {
        _gaussElimination = gaussElimination;
    }
    private double CalculateAlpha(Matrix matrix)
    {
        var n = matrix.CountRows;
        var alpha = 0d;

        for (var i = 0; i < n; i++)
        {
            alpha += matrix[i, i];
        }

        alpha /= 10e8; //уточнить

        return alpha;
    }
    public double Regularize(Equation<Matrix> equation, double[] sigmas, double[] previousSigmas)
    {
        var alpha = CalculateAlpha(equation.Matrix);

        alpha = FindLocalConstraint(sigmas, previousSigmas, alpha);

        alpha = FindGlobalConstraint(equation, alpha, sigmas);

        return alpha;
    }
       

    private void AssembleSLAE(Equation<Matrix> equation, double alpha)
    {
        Matrix.CreateIdentityMatrix(BufferMatrix);

        Matrix.Sum(equation.Matrix, Matrix.Multiply(alpha, BufferMatrix, BufferMatrix), BufferMatrix);

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

    private double FindLocalConstraint(double[] sigmas, double[] previousSigmas, double alpha)
    {
        var ratio = 0d;

        for (int i = 0; i < sigmas.Length; i++)
        {
            ratio = sigmas[i] / previousSigmas[i];
            ratio = Math.Max(ratio, 1d / ratio);

            if (ratio >= 2)
            {
                alpha *= 1.5;
                break;
            }
        }

        return alpha;
    }
    private double FindGlobalConstraint(Equation<Matrix> equation, double alpha, double[] sigmas)
    {
        AssembleSLAE(equation, alpha);

        BufferVector = _gaussElimination.Solve(BufferMatrix, BufferVector);

        var sum = 0d;
        for (int i = 0; i < sigmas.Length; i++)
        {
            sum = sigmas[i] + BufferVector[i];            

            if (sum is >= 5 or <= 1e-3)
            {
                alpha *= 1.5;
                break;
            }
        }
        return alpha;
    }
    private double FindPossibleAlpha(Equation<Matrix> equation, double alpha, out double residual)
    {
        for (; ; )
        {
            try
            {
                AssembleSLAE(equation, alpha);

                BufferVector = _gaussElimination.Solve(BufferMatrix, BufferVector);

                residual = CalculateResidual(equation, alpha);

                break;
            }
            catch { }
            finally
            {
                alpha *= 1.5;
            }
        }

        return alpha;
    }

    private double FindBestAlpha(Equation<Matrix> equation, double alpha, double residual)
    {
        var ratio = 1d;

        do
        {
            try
            {
                AssembleSLAE(equation, alpha);

                BufferVector = _gaussElimination.Solve(BufferMatrix, BufferVector);

                var currentResidual = CalculateResidual(equation, alpha);

                ratio = currentResidual / residual;
            }
            catch { }
            finally
            {
                alpha *= 1.5;
            }
        } while (ratio is < 1.999d or > 3d);

        return alpha / 1.5;
    }
}