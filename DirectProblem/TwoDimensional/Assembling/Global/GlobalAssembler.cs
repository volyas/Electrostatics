using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.Core.Local;
using DirectProblem.FEM.Assembling;
using DirectProblem.FEM.Assembling.Global;
using DirectProblem.FEM.Assembling.Local;
using DirectProblem.SLAE;
using DirectProblem.TwoDimensional.Assembling.Local;
using DirectProblem.TwoDimensional.Assembling.MatrixTemplates;

namespace DirectProblem.TwoDimensional.Assembling.Global;

public class GlobalAssembler<TNode>
{
    private readonly Grid<Node2D> _grid;
    private readonly IMatrixPortraitBuilder<SymmetricSparseMatrix> _matrixPortraitBuilder;
    private readonly ILocalAssembler _localAssembler;
    private readonly IInserter<SymmetricSparseMatrix> _inserter;
    private readonly IGaussExcluder<SymmetricSparseMatrix> _gaussExcluder;
    private readonly LocalBasisFunctionsProvider _localBasisFunctionsProvider;
    private readonly Matrix _massTemplate;
    private readonly int[] _indexes = new int[2];
    private readonly Vector _bufferThetaVector = new(2);
    private readonly Vector _bufferVector = new(2);
    private Equation<SymmetricSparseMatrix> _equation;
    private SymmetricSparseMatrix _preconditionMatrix;

    public GlobalAssembler
    (
        Grid<Node2D> grid,
        IMatrixPortraitBuilder<SymmetricSparseMatrix> matrixPortraitBuilder,
        ILocalAssembler localAssembler,
        IInserter<SymmetricSparseMatrix> inserter,
        IGaussExcluder<SymmetricSparseMatrix> gaussExcluder,
        LocalBasisFunctionsProvider localBasisFunctionsProvider
    )
    {
        _grid = grid;
        _matrixPortraitBuilder = matrixPortraitBuilder;
        _localAssembler = localAssembler;
        _inserter = inserter;
        _gaussExcluder = gaussExcluder;
        _localBasisFunctionsProvider = localBasisFunctionsProvider;
        _massTemplate = MassMatrixTemplateProvider.MassMatrix;
    }

    public GlobalAssembler<TNode> AssembleEquation()
    {
        var globalMatrix = _matrixPortraitBuilder.Build(_grid);
        _preconditionMatrix = globalMatrix.Clone();
        _equation = new Equation<SymmetricSparseMatrix>(
            globalMatrix,
            new Vector(_grid.Nodes.Length),
            new Vector(_grid.Nodes.Length)
        );
        globalMatrix.Copy(_equation.Matrix);
        _equation.RightPart.Clear();
        foreach (var element in _grid)
        {
            var localMatrix = _localAssembler.AssembleMatrix(element);
            var localVector = _localAssembler.AssembleVector(element);

            _inserter.InsertMatrix(_equation.Matrix, localMatrix);
            _inserter.InsertVector(_equation.RightPart, localVector);
        }

        return this;
    }

    public GlobalAssembler<TNode> ApplySource(Source source)
    {
        var element = _grid.Elements.First(x => ElementHas(x, source.Point));

        var theta = source.Current / (2 * Math.PI * source.Point.R * element.Height);

        element.GetBoundNodeIndexes(Bound.Right, _indexes);

        for (var i = 0; i < _bufferThetaVector.Count; i++)
        {
            _bufferThetaVector[i] = theta;
        }

        var mass = Matrix.Multiply(element.Height * source.Point.R / 6d,
            _massTemplate);

        Matrix.Multiply(mass, _bufferThetaVector, _bufferVector);

        _inserter.InsertVector(_equation.RightPart, new LocalVector(_indexes, _bufferVector));

        return this;
    }

    public GlobalAssembler<TNode> ApplyFirstConditions(FirstConditionValue[] conditions)
    {
        foreach (var condition in conditions)
        {
            _gaussExcluder.Exclude(_equation, condition);
        }

        return this;
    }

    public Equation<SymmetricSparseMatrix> BuildEquation()
    {
        return _equation;
    }

    public SymmetricSparseMatrix AllocatePreconditionMatrix()
    {
        _preconditionMatrix = _equation.Matrix.Clone();
        return _preconditionMatrix;
    }

    private bool ElementHas(Element element, Node2D node)
    {
        var lowerLeftCorner = _grid.Nodes[element.NodesIndexes[0]];
        var upperRightCorner = _grid.Nodes[element.NodesIndexes[^1]];
        return (node.R > lowerLeftCorner.R ||
                Math.Abs(node.R - lowerLeftCorner.R) < MethodsConfig.EpsDouble) &&
               (node.Z > lowerLeftCorner.Z ||
                Math.Abs(node.Z - lowerLeftCorner.Z) < MethodsConfig.EpsDouble) &&
               (node.R < upperRightCorner.R ||
                Math.Abs(node.R - upperRightCorner.R) < MethodsConfig.EpsDouble) &&
               (node.Z < upperRightCorner.Z ||
                Math.Abs(node.Z - upperRightCorner.Z) < MethodsConfig.EpsDouble);
    }
}