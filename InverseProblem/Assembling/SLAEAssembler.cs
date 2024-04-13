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

namespace InverseProblem.Assembling;

public class SLAEAssembler {
    private static readonly LinearFunctionsProvider LinearFunctionsProvider = new();

    private readonly GridBuilder2D _gridBuilder2D;
    private readonly DirectProblemSolver _directProblemSolver;

    private readonly Source[] _sources;
    private readonly ReceiverLine[] _receiversLines;
    private readonly Parameter[] _parameters;
    private readonly double[] _truePotentialDifferences;
    private double[] _weightsSquares;
    private readonly double[] _potentialDifferences;
    private readonly double[][] _derivativesPotentialDifferences;

    private Area[] _areas;
    private List<double> _sigmas;
    private FirstConditionValue[] _firstConditions;
    private readonly Equation<Matrix> _equation;

    private Grid<Node2D> _grid;
    private LocalBasisFunctionsProvider _localBasisFunctionsProvider;
    private FEMSolution _femSolution;
    public SLAEAssembler(
            GridBuilder2D gridBuilder2D,
            DirectProblemSolver directProblemSolver,
            Source[] sources,
            ReceiverLine[] receiversLines,
            Parameter[] parameters,
            Vector initialValues,
            double[] truePotentialDifferences,
            Area[] areas,
            List<double> sigmas,
            FirstConditionValue[] firstConditions)
    {
        _gridBuilder2D = gridBuilder2D;
        _directProblemSolver = directProblemSolver;
        _sources = sources;
        _receiversLines = receiversLines;
        _parameters = parameters;
        _truePotentialDifferences = truePotentialDifferences;
        _areas = areas;
        _sigmas = sigmas;
        _firstConditions = firstConditions;

        CalculateWeights();

        _equation = new Equation<Matrix>(new Matrix(parameters.Length), initialValues,
            new Vector(parameters.Length));

        _potentialDifferences = new double[_receiversLines.Length];
        _derivativesPotentialDifferences = new double[parameters.Length][]; //двумерный массив

        for (var i = 0; i < parameters.Length; i++)
        {
            _derivativesPotentialDifferences[i] = new double[_receiversLines.Length];
        }
    }

    private void CalculateWeights()
    {
        _weightsSquares = new double[_truePotentialDifferences.Length];

        for (var i = 0; i < _receiversLines.Length; i++)
        {
            _weightsSquares[i] = Math.Pow(1d / _truePotentialDifferences[i], 2);
        }
    }
    private void SetParameter(Parameter parameter, double value)
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

                for (var i = 0; i < _receiversLines.Length; i++)
                {
                    _equation.Matrix[q, s] += _weightsSquares[i] * _derivativesPotentialDifferences[q][i] *
                                              _derivativesPotentialDifferences[s][i];
                }
            }
        }
    }
    private void AssembleRightPart()
    {
        for (var q = 0; q < _equation.Matrix.CountRows; q++)
        {
            _equation.RightPart[q] = 0;

            for (var i = 0; i < _receiversLines.Length; i++)
            {
                _equation.RightPart[q] -= _weightsSquares[i] *
                                          (_potentialDifferences[i] - _truePotentialDifferences[i]) *
                                          _derivativesPotentialDifferences[q][i];
            }
        }
    }

    private void CalculatePotentialDifference(
        double[] potentialDifferences,
        Source[] sources,
        ReceiverLine[] receiversLines,
        DirectProblemSolver directProblemSolver
        )
    {
        for (var i = 0; i < sources.Length; i++)
        {
            var solution = directProblemSolver
                .SetSource(sources[i])
                .AssembleSLAE()
                .Solve();

            _femSolution = new FEMSolution(_grid, solution, _localBasisFunctionsProvider);
            var potentialM = _femSolution.Calculate(receiversLines[i].PointM);
            var potentialN = _femSolution.Calculate(receiversLines[i].PointN);

            potentialDifferences[i] = potentialM - potentialN;
        }
        
    }
    private void CalculateDerivatives()
    {
        //решаем прямую задачу с начальными параметрами

        CalculatePotentialDifference(
            _potentialDifferences,
            _sources,
            _receiversLines,
            _directProblemSolver
            );

        //считаем производные по каждому параметру
        for (var i = 0; i < _parameters.Length; i++)
        {
            var parameterValue = GetParameter(_parameters[i]);
            var delta = parameterValue / 10;
            SetParameter(_parameters[i], parameterValue + delta);

            CalculatePotentialDifference(
                _derivativesPotentialDifferences[i],
                _sources,
                _receiversLines,
                _directProblemSolver
                );

            SetParameter(_parameters[i], parameterValue); //ввернули параметр на место

            for (var j = 0; j < _receiversLines.Length; j++)
            {
                _derivativesPotentialDifferences[i][j] =
                    (_derivativesPotentialDifferences[i][j] - _potentialDifferences[j]) / delta;
            }
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
