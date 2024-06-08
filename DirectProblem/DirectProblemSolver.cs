using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.SLAE.Preconditions;
using DirectProblem.SLAE.Solvers;
using DirectProblem.TwoDimensional.Assembling.Boundary;
using DirectProblem.TwoDimensional.Assembling;
using DirectProblem.TwoDimensional.Assembling.Global;
using DirectProblem.TwoDimensional.Assembling.Local;
using DirectProblem.TwoDimensional.Parameters;
using DirectProblem.FEM.Assembling.Local;

namespace DirectProblem;

public class DirectProblemSolver
{
    private static readonly MCG MCG = new(new LLTPreconditioner(), new LLTSparse());
    private static readonly MatrixPortraitBuilder MatrixPortraitBuilder = new();
    private static readonly GaussExcluder GaussExcluder = new();
    private static readonly LinearFunctionsProvider LinearFunctionsProvider = new();
    private static readonly Inserter Inserter = new();
    private GlobalAssembler<Node2D> _globalAssembler;
    private Grid<Node2D> _grid;
    private MaterialFactory _materialFactory;
    private FirstConditionValue[] _firstConditions;
    private Source _sources;

    private Equation<SymmetricSparseMatrix> _equation;
    private LocalAssembler _localAssembler;
    private LocalBasisFunctionsProvider _localBasisFunctionsProvider;

    private void InitLocal()
    {
        _localBasisFunctionsProvider = new LocalBasisFunctionsProvider(_grid, LinearFunctionsProvider);
        _localAssembler = new LocalAssembler
            (
                new LocalMatrixAssembler(_grid),
                _materialFactory
            );
        _localAssembler.SetGrid(_grid);
    }
    

    private void InitGlobal()
    {
        _globalAssembler = new GlobalAssembler<Node2D>(_grid, MatrixPortraitBuilder, _localAssembler, Inserter,
            GaussExcluder, _localBasisFunctionsProvider);
    }
    public DirectProblemSolver SetGrid(Grid<Node2D> grid)
    {
        _grid = grid;
        return this;
    }

    public DirectProblemSolver SetMaterials(double[] sigmas)
    {
        _materialFactory = new MaterialFactory(sigmas);
        return this;
    }
    public DirectProblemSolver SetMaterials(MaterialFactory sigmas)
    {
        _materialFactory = sigmas;
        return this;
    }
    public DirectProblemSolver SetSource(Source sources)
    {
        _sources = sources;
        return this;
    }

    public DirectProblemSolver SetFirstConditions(FirstConditionValue[] firstConditions)
    {
        _firstConditions = firstConditions;
        return this;
    }

    public Vector Solve()
    {
        InitLocal();
        InitGlobal();
        AssembleSLAE();
        var solution = MCG.Solve(_equation);

        return solution;
    }


    private void AssembleSLAE()
    {
        _equation = _globalAssembler
            .AssembleEquation()
            //.ApplySource(_sources)
            .ApplyFirstConditions(_firstConditions)
            .BuildEquation();

        var preconditionMatrix = _globalAssembler.AllocatePreconditionMatrix();
        MCG.SetPrecondition(preconditionMatrix);
    }
}