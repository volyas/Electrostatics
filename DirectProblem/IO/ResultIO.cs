using DirectProblem.Core.Base;
using DirectProblem.Core.GridComponents;

namespace DirectProblem.IO;

public class ResultIO
{
    private readonly string _path;

    public ResultIO(string path)
    {
        _path = path;
    }

    public void Write(string fileName, ReceiverLine[] receiverLines)
    {
        using var streamWriter = new StreamWriter(_path + fileName);

        foreach (var receiverLine in receiverLines)
        {
            streamWriter.Write($"{receiverLine.PointM.Z} ");
        }
        streamWriter.WriteLine();

        foreach (var receiverLine in receiverLines)
        {
            streamWriter.Write($"{receiverLine.PotentialDifference} ");
        }
    }

    public void Write(string fileName, double[] potentialDifferences, double[] centersZ)
    {
        using var streamWriter = new StreamWriter(_path + fileName);

        foreach (var centerZ in centersZ)
        {
            streamWriter.Write($"{centerZ} ");
        }
        streamWriter.WriteLine();

        foreach (var potentialDifference in potentialDifferences)
        {
            streamWriter.Write($"{potentialDifference} ");
        }
    }
    public void WriteResult(Vector solution, string fileName)
    {
        using var binaryWriter = new BinaryWriter(File.Open(_path + fileName, FileMode.OpenOrCreate));

        for (var i = 0; i < solution.Count; i++)
        {
            binaryWriter.Write(solution[i]);
        }
    }

}