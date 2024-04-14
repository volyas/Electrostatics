using DirectProblem;
using DirectProblem.Core;
using DirectProblem.Core.Base;
using DirectProblem.Core.Boundary;
using DirectProblem.Core.Global;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.GridGenerator;
using DirectProblem.TwoDimensional.Assembling.Local;
using DirectProblem.TwoDimensional;
using DirectProblem.GridGenerator.Intervals.Splitting;

namespace InverseProblem.Assembling;

public class SLAEAssembler {
    private static readonly LinearFunctionsProvider LinearFunctionsProvider = new();

    private readonly GridBuilder2D _gridBuilder2D;
    private readonly DirectProblemSolver _directProblemSolver;

    private readonly Source _source;
    private readonly ReceiverLine _receiversLine;
    private readonly Parameter[] _parameters;
    private readonly double _truePotentialDifference;
    private double _weightsSquare;
    private readonly double _potentialDifference;
    private readonly double[] _derivativesPotentialDifferences;

    private Area[] _areas;
    private double[] _sigmas;
    private FirstConditionValue[] _firstConditions;
    private readonly Equation<Matrix> _equation;

    private Grid<Node2D> _grid;
    private LocalBasisFunctionsProvider _localBasisFunctionsProvider;
    private FEMSolution _femSolution;
    public SLAEAssembler(
            GridBuilder2D gridBuilder2D,
            DirectProblemSolver directProblemSolver,
            Source source,
            ReceiverLine receiversLine,
            Parameter[] parameters,
            Vector initialValues,
            double truePotentialDifference,
            Area[] areas,
            double[] sigmas,
            FirstConditionValue[] firstConditions)
    {
        _gridBuilder2D = gridBuilder2D;
        _directProblemSolver = directProblemSolver;
        _source = source;
        _receiversLine = receiversLine;
        _parameters = parameters;
        _truePotentialDifference = truePotentialDifference;
        _areas = areas;
        _sigmas = sigmas;
        _firstConditions = firstConditions;

        CalculateWeights();

        _equation = new Equation<Matrix>(new Matrix(parameters.Length), initialValues,
            new Vector(parameters.Length));

        _potentialDifference = new double();
        _derivativesPotentialDifferences = new double[parameters.Length]; 

        for (var i = 0; i < parameters.Length; i++)
        {
            _derivativesPotentialDifferences[i] = new double();
        }
    }

    private void CalculateWeights()
    {
       _weightsSquare = Math.Pow(1d / _truePotentialDifference, 2);
    }
    public void SetParameter(Parameter parameter, double value)
    {
        _sigmas[parameter.Index] = value;
    }
    public double GetParameter(Parameter parameter)
    {
        return _sigmas[parameter.Index];
    }
    private void AssembleMatrix()
    {
        for (var q = 0; q < _equation.Matrix.CountRows; q++)
        {
            for (var s = 0; s < _equation.Matrix.CountColumns; s++)
            {
                _equation.Matrix[q, s] = 0;

                    _equation.Matrix[q, s] += _weightsSquare * _derivativesPotentialDifferences[q] *
                                              _derivativesPotentialDifferences[s];
            }
        }
    }
    private void AssembleRightPart()
    {
        for (var q = 0; q < _equation.Matrix.CountRows; q++)
        {
            _equation.RightPart[q] = 0;
                        
                _equation.RightPart[q] -= _weightsSquare *
                                          (_potentialDifference - _truePotentialDifference) *
                                          _derivativesPotentialDifferences[q];
        }
    }
    private void AssembleDirectProblem()
    {
        _grid = _gridBuilder2D
            .SetRAxis(new AxisSplitParameter(new[]
                    { 0, 0.1, 1.1, 20.1, 100.1 },
                    new UniformSplitter(4),
                    new UniformSplitter(40),
                    new ProportionalSplitter(15, 1.45),
                    new ProportionalSplitter(5, 1.35)
             ))
            .SetZAxis(new AxisSplitParameter(new[]
                    { -260d, -160d, -135d, -131d, -130d, -125d, -100d, 0d },
                    new ProportionalSplitter(5, 1 / 1.5),
                    new ProportionalSplitter(15, 1 / 1.48),
                    new UniformSplitter(156),
                    new UniformSplitter(39),
                    new UniformSplitter(195),
                    new ProportionalSplitter(15, 1.48),
                    new ProportionalSplitter(5, 1.5))
             )
            .SetAreas(_areas)
            .Build();

    }
    private void CalculatePotentialDifferences(
        double potentialDifference,
        Source source,
        ReceiverLine receiversLine,
        DirectProblemSolver directProblemSolver
        )
    {
            var solution = directProblemSolver
                .SetGrid(_grid)
                .SetMaterials(_sigmas)
                .SetSource(source)
                .SetFirstConditions(_firstConditions)
                .Solve();
        _localBasisFunctionsProvider = new LocalBasisFunctionsProvider(_grid, LinearFunctionsProvider);
        _femSolution = new FEMSolution(_grid, solution, _localBasisFunctionsProvider);
            var potentialM = _femSolution.Calculate(receiversLine.PointM);
            var potentialN = _femSolution.Calculate(receiversLine.PointN);

            potentialDifference = potentialM - potentialN;
        
    }
    private void CalculateDerivatives()
    {
        //решаем прямую задачу с начальными параметрами
        AssembleDirectProblem();
        CalculatePotentialDifferences(
            _potentialDifference,
            _source,
            _receiversLine,
            _directProblemSolver
            );

        //считаем производные по каждому параметру
        for (var i = 0; i < _parameters.Length; i++)
        {
            var parameterValue = GetParameter(_parameters[i]);
            var delta = parameterValue / 10;
            SetParameter(_parameters[i], parameterValue + delta);

            CalculatePotentialDifferences(
                _derivativesPotentialDifferences[i],
                _source,
                _receiversLine,
                _directProblemSolver
                );

            SetParameter(_parameters[i], parameterValue); //ввернули параметр на место

                _derivativesPotentialDifferences[i] =
                    (_derivativesPotentialDifferences[i] - _potentialDifference) / delta;
        }
    }    

    private void AssembleSLAE()
    {
        CalculateDerivatives();
        AssembleMatrix();
        AssembleRightPart();
    }
    public Equation<Matrix> Build()
    {
        AssembleSLAE();
        return _equation;
    }

}
