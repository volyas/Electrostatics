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
    private static readonly MCG MCG = new(new LLTPreconditioner(), new LLTSparse());
    private readonly GlobalAssembler<Node2D> _globalAssembler;
    private readonly FirstConditionValue[] _firstConditions;
    private Source _source;
    private Equation<SymmetricSparseMatrix> _equation;

    public DirectProblemSolver
    (
        GlobalAssembler<Node2D> globalAssembler,
        FirstConditionValue[] firstConditions
    )
    {
        _globalAssembler = globalAssembler;
        _firstConditions = firstConditions;
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