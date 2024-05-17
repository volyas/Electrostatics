using DirectProblem;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.GridGenerator;
using DirectProblem.GridGenerator.Intervals.Splitting;
using DirectProblem.TwoDimensional.Assembling.Boundary;
using DirectProblem.TwoDimensional.Assembling.Local;
using DirectProblem.TwoDimensional;
using DirectProblem.TwoDimensional.Parameters;
using InverseProblem.Assembling;
using DirectProblem.Core.Base;
using InverseProblem;
using InverseProblem.SLAE;
using System.Globalization;
using DirectProblem.IO;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
// инициализируем и задаём истинную сетку
var gridBuilder2D = new GridBuilder2D();
var rSplitParameters = new AxisSplitParameter(new[]
        { 0d, 0.1, 5d, 8d, 10d },
                    new UniformSplitter(4),
                    new NonUniformStepSplitter(0.025, 1.05),
                    new NonUniformStepSplitter(0.260, 1.05),
                    new NonUniformStepSplitter(0.380, 1.05)
        );
var zSplitParameters = new AxisSplitParameter(new[]
        { -10d, -6d, -5d, -4d, 0d },
                    new NonUniformStepSplitter(0.025, 1 / 1.05),
                    new UniformStepSplitter(0.025),
                    new UniformStepSplitter(0.025),
                    new NonUniformStepSplitter(0.025, 1.05)
        );

var areas = new Area[]
{
    new(7, new Node2D(0d, -10d), new Node2D(0.1, 0d)),
                new(6, new Node2D(5d, -4d), new Node2D(8d, 0d)),
                new(1, new Node2D(0.1, -4d), new Node2D(5d, 0d)),
                new(5, new Node2D(5d, -5d), new Node2D(8d, -4d)),
                new(2, new Node2D(5d, -6d), new Node2D(8d, -5d)),
                new(1, new Node2D(0.1, -6d), new Node2D(5d, -4d)),
                new(3, new Node2D(0.1, -10d), new Node2D(5d, -6d)),
                new(0, new Node2D(5d, -10d), new Node2D(8d, -6d)),
                new(2, new Node2D(8d, -5d), new Node2D(10d, 0d)),
                new(4, new Node2D(8d, -10d), new Node2D(10d, -5d))
};
var trueGrid = gridBuilder2D
.SetRAxis(rSplitParameters)
.SetZAxis(zSplitParameters)
.SetAreas(areas)
.Build();

var trueSigmas = new MaterialFactory
(
    new List<double> { 0.01, 0.025, 0.08, 0.1, 0.2, 1d / 3, 0.045, 0.5 }
);

var localBasisFunctionsProvider = new LocalBasisFunctionsProvider(trueGrid, new LinearFunctionsProvider());

var firstBoundaryProvider = new FirstBoundaryProvider(trueGrid);
var conditions = firstBoundaryProvider.GetConditions(trueGrid.Nodes.RLength - 1, trueGrid.Nodes.ZLength - 1);

var current = 1d;
var sources = new Source[10];
var receivesrLines = new ReceiverLine[sources.Length];
var truePotentialDifferences = new double[sources.Length];
var centersZ = new double[sources.Length];
for (var i = 0; i < sources.Length; i++)
{
    sources[i] = new Source(new Node2D(0.05, -2 - 0.5 * i), current);
    receivesrLines[i] = new ReceiverLine(
        new Node2D(sources[i].Point.R, sources[i].Point.Z - 0.5),
        new Node2D(sources[i].Point.R, sources[i].Point.Z - 1)
    );
    centersZ[i] = (sources[i].Point.Z + receivesrLines[i].PointN.Z) / 2;
}

var directProblemSolver = new DirectProblemSolver();

for (var i = 0; i < sources.Length; i++)
{
    var trueSolution = directProblemSolver
        .SetGrid(trueGrid)
        .SetMaterials(trueSigmas)
        .SetSource(sources[i])
        .SetFirstConditions(conditions)
        .Solve();

    var femSolution = new FEMSolution(trueGrid, trueSolution, localBasisFunctionsProvider);

    var potentialM = femSolution.Calculate(receivesrLines[i].PointM);
    var potentialN = femSolution.Calculate(receivesrLines[i].PointN);

    truePotentialDifferences[i] = potentialM - potentialN;

    CourseHolder.GetInfo(i, 0);
    Console.WriteLine();
}
Console.WriteLine("DirectProblem solved!\n");

var sigmas = new[] { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1 };
var resultO = new ResultIO("../InverseProblem/Results/");
resultO.WriteConductivity($"trueConductivity.txt", sigmas);
var targetParameters = new InverseProblem.Assembling.Parameter[]
{
    new (ParameterType.Sigma, 0),
    new (ParameterType.Sigma, 1),
    new (ParameterType.Sigma, 2),
    new (ParameterType.Sigma, 3),
    new (ParameterType.Sigma, 4),
    new (ParameterType.Sigma, 5),
    new (ParameterType.Sigma, 6),
    new (ParameterType.Sigma, 7)
};
var initialValues = new Vector(new[] { 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1, 0.1 });
var gaussElimination = new GaussElimination();
var regularizer = new Regularizer(gaussElimination, targetParameters);

var inverseProblemSolver = new InverseProblemSolver(gridBuilder2D, gaussElimination, regularizer);
    var solution = inverseProblemSolver
        .SetSources(sources)
        .SetReceivers(receivesrLines)
        .SetParameters(targetParameters, initialValues)
        .SetTruePotentialDifference(truePotentialDifferences)
        .SetDirectProblemParameters(areas, rSplitParameters, zSplitParameters, sigmas, conditions)
        .Solve();

    foreach (var value in solution)
    {
        Console.WriteLine(value);
    }
