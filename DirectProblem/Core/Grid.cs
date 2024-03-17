using DirectProblem.Core.GridComponents;

namespace DirectProblem.Core;

public class Grid<TPoint>
{
    public TPoint[] Nodes { get; }
    public Element[] Elements { get; }
    public Area[]? Areas { get; }

    public IEnumerator<Element> GetEnumerator() => ((IEnumerable<Element>)Elements).GetEnumerator();

    public Grid(TPoint[] nodes, Element[] elements)
    {
        Nodes = nodes;
        Elements = elements;
    }

    public Grid(TPoint[] nodes, Element[] elements, Area[] areas) : this(nodes, elements)
    {
        Areas = areas;
    }
}