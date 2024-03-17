namespace DirectProblem.Core.GridComponents;

public class Source
{
    public Node2D Point { get; init; }
    public double Current { get; init; } = 0;
    public double Potential { get; set; } = 0;

    public Source(Node2D point)
    {
        Point = point;
    }

    public Source(Node2D point, double current)
    {
        Point = point;
        Current = current;
    }
};