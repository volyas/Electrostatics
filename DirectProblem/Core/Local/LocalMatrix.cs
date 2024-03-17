using DirectProblem.Core.Base;

namespace DirectProblem.Core.Local;

public class LocalMatrix
{
    public int[] Indexes { get; }
    public Matrix Matrix { get; }

    public LocalMatrix(int[] indexes, Matrix matrix)
    {
        Matrix = matrix;
        Indexes = indexes;
    }

    public double this[int i, int j]
    {
        get => Matrix[i, j];
        set => Matrix[i, j] = value;
    }
}