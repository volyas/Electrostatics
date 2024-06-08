using DirectProblem.Core.GridComponents;
using DirectProblem.Core.Local;

namespace DirectProblem.FEM.Assembling.Local;

public interface ILocalAssembler
{
    public LocalMatrix AssembleMatrix(Element element);
    public LocalVector AssembleVector(Element element);
}