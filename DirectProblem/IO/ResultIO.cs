using DirectProblem.Core.Base;
using DirectProblem.Core.GridComponents;
using System.Net.NetworkInformation;

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

    public void WriteDirectProblemResult(string fileName, double[] potentialDifferences, double[] centersZ)
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
    public void WriteBinaryResult(Vector solution, string fileName)
    {
        using var binaryWriter = new BinaryWriter(File.Open(_path + fileName, FileMode.OpenOrCreate));

        for (var i = 0; i < solution.Count; i++)
        {
            binaryWriter.Write(solution[i]);
        }
    }
    public void WriteInverseProblemResult(ReceiverLine[] receivers, Vector solution, string fileName)
    {
        using var streamWriter = new StreamWriter(_path + fileName);

        foreach (var receiver in receivers)
        {
            streamWriter.Write($"{receiver.PointM.Z} ");
        }

        streamWriter.WriteLine();

        for (var i = 0; i < solution.Count; i++)
        {   
            streamWriter.Write($"{solution[i]} ");
        }
    }
    public void WriteConductivity(string fileName, double[] sigmas)
    {
        using var streamWriter = new StreamWriter(_path + fileName);

        foreach (var sigma in sigmas)
        {
            streamWriter.Write($"{sigma} ");
        }
    }

}