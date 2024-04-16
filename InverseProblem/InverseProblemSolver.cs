using DirectProblem;
using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.GridGenerator;
using DirectProblem.SLAE;
using DirectProblem.TwoDimensional.Assembling.Global;
using DirectProblem.TwoDimensional.Parameters;
using InverseProblem.Assembling;
using InverseProblem.SLAE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InverseProblem;

public class InverseProblemSolver
{

    private static readonly GaussElimination GaussElimination = new();
    private static readonly Regularizer Regularizer = new(GaussElimination);
    private static readonly DirectProblemSolver DirectProblemSolver = new();
    private readonly GridBuilder2D _gridBuilder2D;
    private SLAEAssembler _slaeAssembler;
    private Source[] _sources;
    private ReceiverLine[] _receivers;
    private Parameter[] _parameters;
    private Vector _trueValues;
    private Vector _initialValues;

    private double[] _truePotentialDifferences;

    private Area[] _areas;
    private double[] _sigmas;
    private double[] _previousSigmas;

    private FirstConditionValue[] _firstConditions;

    private Matrix _bufferMatrix;
    private Vector _bufferVector;
    public InverseProblemSolver(GridBuilder2D gridBuilder2D)
    {
        _gridBuilder2D = gridBuilder2D;
    }
    public InverseProblemSolver SetSource(Source[] sources)
    {
        _sources = sources;
        return this;
    }

    public InverseProblemSolver SetReceiver(ReceiverLine[] receivers)
    {
        _receivers = receivers;

        return this;
    }

    public InverseProblemSolver SetParameters(Parameter[] parameters, Vector trueValues, Vector initialValues)
    {
        _parameters = parameters;
        _trueValues = trueValues;
        _initialValues = initialValues;

        return this;
    }
    public InverseProblemSolver SetTruePotentialDifference(double[] truePotentialDifferences)
    {
        _truePotentialDifferences = truePotentialDifferences;

        return this;
    }

    public InverseProblemSolver SetDirectProblemParameters(Area[] areas,
        double[] sigmas, FirstConditionValue[] firstConditions)
    {
        _areas = areas;
        _sigmas = sigmas;
        _previousSigmas = sigmas;
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
            _sigmas,
            _firstConditions
            );

        _bufferMatrix = Matrix.CreateIdentityMatrix(_parameters.Length);
        _bufferVector = new Vector(_parameters.Length);
        Regularizer.BufferMatrix = _bufferMatrix;
        Regularizer.BufferVector = _bufferVector;
    }

    public Vector Solve()
    {
        Init();

        var functionality = 1d;
        Equation<Matrix> equation = null!;

        for (var i = 1; i <= MethodsConfig.MaxIterations && functionality > MethodsConfig.FuncEps; i++)
        {
            equation = _slaeAssembler.Build();

            var alphas = Regularizer.Regularize(equation);
            //var alphas = new Vector(1);


            Matrix.CreateIdentityMatrix(_bufferMatrix);

            Matrix.Sum(equation.Matrix, Matrix.Multiply(alphas, _bufferMatrix, _bufferMatrix), equation.Matrix);

            //Vector.Subtract(
            //    equation.RightPart, Vector.Multiply(
            //        alpha, Vector.Subtract(equation.Solution, _trueValues, _bufferVector),
            //        _bufferVector),
            //    equation.RightPart);


            _bufferMatrix = equation.Matrix.Copy(_bufferMatrix);
            _bufferVector = equation.RightPart.Copy(_bufferVector);

            _bufferVector = GaussElimination.Solve(_bufferMatrix, _bufferVector);  
            
            

            Vector.Sum(equation.Solution, _bufferVector, equation.Solution);
            UpdateParameters(equation.Solution);

            functionality = _slaeAssembler.CalculateFunctionality();
            for (var k = 0; k < _bufferVector.Count; k++)
            {
                Console.WriteLine($"{equation.Solution[k]} {_bufferVector[k]}");
            }

            CourseHolder.GetFunctionalityInfo(i, functionality);
            Console.WriteLine();
        }

        Console.WriteLine();

        return equation.Solution;
    }

    private void UpdateParameters(Vector vector)
    {

        for (var i = 0; i < _parameters.Length; i++)
        {
            _previousSigmas[i] = _sigmas[i]; //фиксируем предыдущее значение сигм
            _slaeAssembler.SetParameter(_parameters[i], vector[i]);
        }
    }

    //private double CalculateResidual(Equation<Matrix> equation)
    //{
    //    return Vector.Subtract(
    //        equation.RightPart,
    //        Matrix.Multiply(equation.Matrix, _bufferVector, _residualBufferVector),
    //        equation.RightPart)
    //        .Norm;
    //}
}
