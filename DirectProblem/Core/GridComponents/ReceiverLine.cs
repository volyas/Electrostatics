namespace DirectProblem.Core.GridComponents;

public class ReceiverLine
{
    public Node2D PointM { get; init; }
    public Node2D PointN { get; init; }
    public double PotentialM { get; set; } = 0;
    public double PotentialN { get; set; } = 0;
    public double PotentialDifference => PotentialM - PotentialN;

    public ReceiverLine(Node2D pointM, Node2D pointN)
    {
        PointM = pointM;
        PointN = pointN;
    }

}