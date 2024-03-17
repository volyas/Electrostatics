using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.SLAE.Preconditions;
using DirectProblem.SLAE.Solvers;
using DirectProblem.TwoDimensional.Assembling.Global;
using DirectProblem.TwoDimensional.Assembling.Local;

namespace DirectProblem;

public class DirectProblemSolver
{
    private static readonly LinearFunctionsProvider LinearFunctionsProvider = new();
    private static readonly MCG MCG = new(new LLTPreconditioner(), new LLTSparse());
    private readonly Grid<Node2D> _grid;
    private readonly GlobalAssembler<Node2D> _globalAssembler;
    private readonly LocalBasisFunctionsProvider _localBasisFunctionsProvider;
    private readonly FirstConditionValue[] _firstConditions;
    private readonly ReceiverLine[] _receiverLines;
    private Source _source;
    private Equation<SymmetricSparseMatrix> _equation;

    public DirectProblemSolver
    (
        Grid<Node2D> grid,
        GlobalAssembler<Node2D> globalAssembler,
        FirstConditionValue[] firstConditions,
        ReceiverLine[] receiverLines
    )
    {
        _grid = grid;
        _globalAssembler = globalAssembler;
        _localBasisFunctionsProvider = new LocalBasisFunctionsProvider(grid, LinearFunctionsProvider);
        _firstConditions = firstConditions;
        _receiverLines = receiverLines;
    }

    public DirectProblemSolver SetSource(Source source)
    {
        _source = source;

        return this;
    }

    public DirectProblemSolver AssembleSLAE()
    {
        _equation = _globalAssembler
            .AssembleEquation()
            .ApplySource(_source)
            .ApplyFirstConditions(_firstConditions)
            .BuildEquation();

        var preconditionMatrix = _globalAssembler.AllocatePreconditionMatrix();
        MCG.SetPrecondition(preconditionMatrix);

        return this;
    }

    public Vector Solve()
    {
        var solution = MCG.Solve(_equation);

        return solution;
    }
}