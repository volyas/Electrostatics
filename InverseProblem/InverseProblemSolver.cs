using DirectProblem;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.GridGenerator;
using DirectProblem.GridGenerator.Intervals.Splitting;
using DirectProblem.IO;
using DirectProblem.SLAE;
using InverseProblem.Assembling;
using InverseProblem.SLAE;

namespace InverseProblem;

public class InverseProblemSolver
{

    private readonly GaussElimination _gaussElimination;
    private readonly Regularizer _regularizer;
    private static readonly DirectProblemSolver DirectProblemSolver = new();
    private readonly GridBuilder2D _gridBuilder2D;
    private SLAEAssembler _slaeAssembler;
    private Source[] _sources;
    private ReceiverLine[] _receivers;
    private Parameter[] _parameters;
    private Vector _initialValues;

    private double[] _truePotentialDifferences;

    private Area[] _areas;
    private AxisSplitParameter _rSplitParameters;
    private AxisSplitParameter _zSplitParameters;
    private double[] _sigmas;

    private FirstConditionValue[] _firstConditions;

    private Matrix _leftPart;
    private Vector _bufferVector;
    public InverseProblemSolver(GridBuilder2D gridBuilder2D, GaussElimination gaussElimination, Regularizer regularizer)
    {
        _gridBuilder2D = gridBuilder2D;
        _gaussElimination = gaussElimination;
        _regularizer = regularizer;
    }
    public InverseProblemSolver SetSources(Source[] sources)
    {
        _sources = sources;
        return this;
    }

    public InverseProblemSolver SetReceivers(ReceiverLine[] receivers)
    {
        _receivers = receivers;

        return this;
    }

    public InverseProblemSolver SetParameters(Parameter[] parameters, Vector initialValues)
    {
        _parameters = parameters;
        _initialValues = initialValues;

        return this;
    }
    public InverseProblemSolver SetTruePotentialDifference(double[] truePotentialDifferences)
    {
        _truePotentialDifferences = truePotentialDifferences;

        return this;
    }

    public InverseProblemSolver SetDirectProblemParameters(
        Area[] areas,
        AxisSplitParameter rSplitParameters,
        AxisSplitParameter zSplitParameters,
        double[] sigmas,
        FirstConditionValue[] firstConditions)
    {
        _areas = areas;
        _rSplitParameters = rSplitParameters;
        _zSplitParameters = zSplitParameters;
        _sigmas = sigmas;
        _firstConditions = firstConditions;

        return this;
    }

    public void Init()
    {
        _slaeAssembler = new SLAEAssembler(
            _gridBuilder2D,
            DirectProblemSolver,
            _sources,
            _receivers,
            _parameters,
            _initialValues,
            _truePotentialDifferences,
            _areas,
            _rSplitParameters,
            _zSplitParameters,
            _sigmas,
            _firstConditions
            );

        _leftPart = Matrix.CreateIdentityMatrix(_parameters.Length);
        _bufferVector = new Vector(_parameters.Length);
        _regularizer.BufferMatrix = _leftPart;
        _regularizer.BufferVector = _bufferVector;
    }

    public Vector Solve()
    {
        Init();

        var functionality = 1d;
        Equation<Matrix> equation = null!;
        var resultO = new ResultIO("../InverseProblem/Results/");
        resultO.WriteInverseProblemResult(_receivers, equation.Solution, $"potentialDifferencesIteration_0.txt");
        for (var i = 1; i <= MethodsConfig.MaxIterations && functionality > MethodsConfig.FuncEps; i++)
        {
            equation = _slaeAssembler.Build();

            var alphas = _regularizer.Regularize(equation);
            //var alphas = new Vector(1);

            Matrix.CreateIdentityMatrix(_leftPart);

            Matrix.Sum(equation.Matrix, Matrix.Multiply(alphas, _leftPart, _leftPart), equation.Matrix);

            _leftPart = equation.Matrix.Copy(_leftPart);
            _bufferVector = equation.RightPart.Copy(_bufferVector);

            _bufferVector = _gaussElimination.Solve(_leftPart, _bufferVector);

            Vector.Sum(equation.Solution, _bufferVector, equation.Solution);
            UpdateParameters(equation.Solution);

            functionality = _slaeAssembler.CalculateFunctionality();
            
            for (var k = 0; k < _bufferVector.Count; k++)
            {
                Console.WriteLine($"{equation.Solution[k]} {_bufferVector[k]}");
                
            }
            
            resultO.WriteInverseProblemResult(_receivers, equation.Solution, $"potentialDifferencesIteration_{i}.txt");
            CourseHolder.GetFunctionalityInfo(i, functionality);
            
            resultO.WriteConductivity($"conductivity_{i}.txt", _sigmas);
            Console.WriteLine();
        }

        Console.WriteLine();

        return equation.Solution;
    }

    private void UpdateParameters(Vector vector)
    {
        for (var i = 0; i < _parameters.Length; i++)
        {
            _slaeAssembler.SetParameter(_parameters[i], vector[i]);
        }
    }
}
