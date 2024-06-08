using DirectProblem.Core.Base;
using DirectProblem.Core.GridComponents;

namespace DirectProblem.FEM.Assembling.Local;

public interface ILocalMatrixAssembler
{
    public Matrix AssembleStiffnessMatrix(Element element);
    public Matrix AssembleMassMatrix(Element element);
}