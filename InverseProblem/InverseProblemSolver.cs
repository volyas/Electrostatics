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
    private Source _source;
    private ReceiverLine _receiver;
    private Parameter[] _parameters;
    private Vector _trueValues;
    private Vector _initialValues;

    private double _truePotentialDifference;

    private Area[] _areas;
    private double[] _sigmas;
    private double[] _previousSigmas;

    private FirstConditionValue[] _firstConditions;

    private Matrix _bufferMatrix;
    private Vector _bufferVector;
    private Vector _residualBufferVector;
    public InverseProblemSolver(GridBuilder2D gridBuilder2D)
    {
        _gridBuilder2D = gridBuilder2D;
    }
    public InverseProblemSolver SetSource(Source source)
    {
        _source = source;
        return this;
    }

    public InverseProblemSolver SetReceiver(ReceiverLine receiver)
    {
        _receiver = receiver;

        return this;
    }

    public InverseProblemSolver SetParameters(Parameter[] parameters, Vector trueValues, Vector initialValues)
    {
        _parameters = parameters;
        _trueValues = trueValues;
        _initialValues = initialValues;

        return this;
    }
    public InverseProblemSolver SetTruePotentialDifference(double truePotentialDifference)
    {
        _truePotentialDifference = truePotentialDifference;

        return this;
    }

    public InverseProblemSolver SetDirectProblemParameters(Area[] areas,
        double[] sigmas, FirstConditionValue[] firstConditions)
    {
        _areas = areas;
        _sigmas = sigmas;
        _firstConditions = firstConditions;

        return this;
    }

    public void Init()
    {
        _slaeAssembler = new SLAEAssembler(
            _gridBuilder2D,
            DirectProblemSolver,
            _source,
            _receiver,
            _parameters,
            _initialValues,
            _truePotentialDifference,
            _areas,
            _sigmas,
            _firstConditions
            );

        _bufferMatrix = Matrix.CreateIdentityMatrix(_parameters.Length);
        _bufferVector = new Vector(_parameters.Length);
        _residualBufferVector = new Vector(_parameters.Length);
        Regularizer.BufferMatrix = _bufferMatrix;
        Regularizer.BufferVector = _bufferVector;
        Regularizer.ResidualBufferVector = _residualBufferVector;
    }

    public Vector Solve()
    {
        //Собрать сетку для базовых значений, учесть только А электрод
        //Посчитать разности потенциалов M-N для А, сделать аналогично для B, но потенциалы буду с минусом
        //Сложить разности при А и при Б
        Init();

        var residual = 1d;
        Equation<Matrix> equation = null!;

        for (var i = 1; i <= MethodsConfig.MaxIterations && residual > MethodsConfig.EpsDouble; i++)
        {
            equation = _slaeAssembler.Build();

            var alpha = Regularizer.Regularize(equation);

            Matrix.CreateIdentityMatrix(_bufferMatrix);

            Matrix.Sum(equation.Matrix, Matrix.Multiply(alpha, _bufferMatrix, _bufferMatrix), equation.Matrix);

            Vector.Subtract(
                equation.RightPart, Vector.Multiply(
                    alpha, Vector.Subtract(equation.Solution, _trueValues, _bufferVector),
                    _bufferVector),
                equation.RightPart);

            _bufferMatrix = equation.Matrix.Copy(_bufferMatrix);
            _bufferVector = equation.RightPart.Copy(_bufferVector);

            _bufferVector = GaussElimination.Solve(_bufferMatrix, _bufferVector);

            residual = CalculateResidual(equation);

            Vector.Sum(equation.Solution, _bufferVector, equation.Solution);

            UpdateParameters(equation.Solution);

            CourseHolder.GetInfo(i, residual);
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

    private double CalculateResidual(Equation<Matrix> equation)
    {
        return Vector.Subtract(
            equation.RightPart,
            Matrix.Multiply(equation.Matrix, _bufferVector, _residualBufferVector),
            equation.RightPart)
            .Norm;
    }
}
