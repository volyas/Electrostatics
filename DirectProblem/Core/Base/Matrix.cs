namespace DirectProblem.Core.Base;

public class Matrix
{
    public double[,] Values { get; }

    public Matrix(double[,] matrix)
    {
        Values = matrix;
    }
    public Matrix(int size) : this(new double[size, size]) { }

    public Matrix(int rows, int columns) : this(new double[rows, columns]) { }

    public int CountRows => Values.GetLength(0);
    public int CountColumns => Values.GetLength(1);

    public double this[int i, int j]
    {
        get => Values[i, j];
        set => Values[i, j] = value;
    }

    public static Matrix Sum(Matrix matrix1, Matrix matrix2, Matrix? result = null)
    {
        if (matrix1.CountRows != matrix2.CountColumns || matrix1.CountColumns != matrix2.CountRows)
            throw new ArgumentOutOfRangeException(
                $"{nameof(matrix1)} and {nameof(matrix2)} must have same size");

        result ??= new Matrix(matrix1.CountRows);

        for (var i = 0; i < matrix1.CountRows; i++)
        {
            for (var j = 0; j < matrix1.CountColumns; j++)
            {
                result[i, j] = matrix1[i, j] + matrix2[i, j];
            }
        }

        return result;
    }

    public static Matrix Multiply(double coefficient, Matrix matrix, Matrix? result = null)
    {
        result ??= new Matrix(matrix.CountRows, matrix.CountColumns);

        for (var i = 0; i < matrix.CountRows; i++)
        {
            for (var j = 0; j < matrix.CountColumns; j++)
            {
                result[i, j] = matrix[i, j] * coefficient;
            }
        }

        return result;
    }

    public static Vector Multiply(Matrix matrix, Vector vector, Vector? result = null)
    {
        if (matrix.CountRows != vector.Count)
            throw new ArgumentOutOfRangeException(
                $"{nameof(matrix.CountRows)} and {nameof(vector)} must have same size");

        result ??= new Vector(vector.Count);

        for (var i = 0; i < matrix.CountRows; i++)
        {
            result[i] = 0d;

            for (var j = 0; j < matrix.CountColumns; j++)
            {
                result[i] += matrix[i, j] * vector[j];
            }
        }

        return result;
    }

    public static Span<double> Multiply(Matrix matrix, Span<double> vector, Span<double> result)
    {
        if (matrix.CountRows != vector.Length || vector.Length != result.Length)
            throw new ArgumentOutOfRangeException(
                $"{nameof(matrix.CountRows)}, {nameof(vector)} and {nameof(result)} must have same size");

        for (var i = 0; i < matrix.CountRows; i++)
        {
            for (var j = 0; j < matrix.CountColumns; j++)
            {
                result[i] += matrix[i, j] * vector[j];
            }
        }

        return result;
    }

    public void SwapRows(int row1, int row2)
    {
        for (var i = 0; i < CountColumns; i++)
        {
            (Values[row2, i], Values[row1, i]) = (Values[row1, i], Values[row2, i]);
        }
    }

    public Matrix Clone()
    {
        var clone = (double[,])Values.Clone();

        return new Matrix(clone);
    }

    public Matrix Copy(Matrix matrix)
    {
        if (matrix.CountRows != CountColumns || matrix.CountColumns != CountRows)
            throw new ArgumentOutOfRangeException(
                $"{nameof(Values)} and {nameof(matrix)} must have same size");

        Array.Copy(Values, matrix.Values, matrix.Values.Length);

        return matrix;
    }

    public void Clear()
    {
        Array.Clear(Values);
    }

    public static Matrix CreateIdentityMatrix(int size)
    {
        var matrix = new Matrix(size);

        for (var i = 0; i < matrix.CountRows; i++)
        {
            matrix[i, i] = 1d;
        }

        return matrix;
    }

    public static Matrix CreateIdentityMatrix(Matrix matrix)
    {
        matrix.Clear();

        for (var i = 0; i < matrix.CountRows; i++)
        {
            matrix[i, i] = 1d;
        }

        return matrix;
    }
}