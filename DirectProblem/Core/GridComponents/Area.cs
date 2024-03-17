using DirectProblem.SLAE;

namespace DirectProblem.Core.GridComponents;

public class Area
{
    public List<int> ElementsIndexes { get; }
    public int MaterialId { get; private set; }
    public Node2D LowerLeftCorner { get; private set; }
    public Node2D UpperRightCorner { get; private set; }

    public Area(int materialId, Node2D lowerLeftCorner, Node2D upperRightCorner)
    {
        ElementsIndexes = new List<int>();
        MaterialId = materialId;
        LowerLeftCorner = lowerLeftCorner;
        UpperRightCorner = upperRightCorner;
    }

    public Area(List<int> elementsIndexes, Node2D lowerLeftCorner, Node2D upperRightCorner)
    {
        ElementsIndexes = elementsIndexes;
        LowerLeftCorner = lowerLeftCorner;
        UpperRightCorner = upperRightCorner;
    }

    public void Add(int index)
    {
        ElementsIndexes.Add(index);
    }

    public bool Has(Node2D elementLowerLeftCorner, Node2D elementUpperRightCorner)
    {
        return (elementLowerLeftCorner.R > LowerLeftCorner.R ||
                Math.Abs(elementLowerLeftCorner.R - LowerLeftCorner.R) < MethodsConfig.EpsDouble) &&
               (elementLowerLeftCorner.Z > LowerLeftCorner.Z ||
                Math.Abs(elementLowerLeftCorner.Z - LowerLeftCorner.Z) < MethodsConfig.EpsDouble) &&
               (elementUpperRightCorner.R < UpperRightCorner.R ||
                Math.Abs(elementUpperRightCorner.R - UpperRightCorner.R) < MethodsConfig.EpsDouble) &&
               (elementUpperRightCorner.Z < UpperRightCorner.Z ||
                Math.Abs(elementUpperRightCorner.Z - UpperRightCorner.Z) < MethodsConfig.EpsDouble);
    }
}