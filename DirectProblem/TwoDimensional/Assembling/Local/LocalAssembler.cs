using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.GridComponents;
using DirectProblem.Core.Local;
using DirectProblem.FEM.Assembling.Local;
using DirectProblem.TwoDimensional.Parameters;

namespace DirectProblem.TwoDimensional.Assembling.Local;

public class LocalAssembler : ILocalAssembler
{
    private readonly ILocalMatrixAssembler _localMatrixAssembler;
    private readonly MaterialFactory _materialFactory;
    private readonly Func<Node2D, double> _b;
    private readonly Vector _buffer;
    private readonly Vector _bufferVector;
    private Grid<Node2D> _grid;

    public LocalAssembler
    (
        ILocalMatrixAssembler localMatrixAssembler,
        MaterialFactory materialFactory
    )
    {
        _localMatrixAssembler = localMatrixAssembler;
        _materialFactory = materialFactory;
        _b = node => - Math.Exp(node.Z);
        _buffer = new Vector(4);
        _bufferVector = new Vector(4);
    }

    public LocalMatrix AssembleMatrix(Element element)
    {
        var matrix = GetStiffnessMatrix(element);
        var sigma = _materialFactory.GetById(element.MaterialId).Sigma;

        return new LocalMatrix(element.NodesIndexes, Matrix.Multiply(sigma, matrix, matrix));
    }
    public LocalVector AssembleVector(Element element)
    {
        var vector = GetVector(element);

        return new LocalVector(element.NodesIndexes, vector);
    }

    private Matrix GetStiffnessMatrix(Element element)
    {
        var stiffness = _localMatrixAssembler.AssembleStiffnessMatrix(element);

        return stiffness;
    }
    private Matrix GetMassMatrix(Element element)
    {
        var mass = _localMatrixAssembler.AssembleMassMatrix(element);

        return mass;
    }
    private Vector GetVector(Element element)
    {
        var mass = GetMassMatrix(element);

        for (var i = 0; i < _buffer.Count; i++)
        {
            _buffer[i] = _b(_grid.Nodes[element.NodesIndexes[i]]);
        }

        Matrix.Multiply(mass, _buffer, _bufferVector);
        _bufferVector.Copy(_buffer);

        return _buffer;
    }
    public LocalAssembler SetGrid(Grid<Node2D> grid)
    {
        _grid = grid;

        return this;
    }
}