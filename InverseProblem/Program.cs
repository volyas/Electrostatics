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
                new(0, new Node2D(0.1, -4d), new Node2D(5d, 0d)),
                new(3, new Node2D(5d, -5d), new Node2D(8d, 0d)),
                new(2, new Node2D(8d, -4d), new Node2D(10d, 0d)),
                new(5, new Node2D(0.1, -5d), new Node2D(5d, -4d)),
                new(4, new Node2D(0.1, -6d), new Node2D(5d, -5d)),
                new(1, new Node2D(8d, -5d), new Node2D(10d, -4d)),
                new(6, new Node2D(5d, -6d), new Node2D(10d, -5d)),
                new(1, new Node2D(0.1, -10d), new Node2D(8d, -6d)),
                new(3, new Node2D(8d, -10d), new Node2D(10d, -6d))
};

//var rSplitParameters = new AxisSplitParameter(new[]
//        { 0, 0.1, 1.1, 20.1, 100.1 },
//        new UniformSplitter(4),
//        new UniformSplitter(40),
//        new ProportionalSplitter(15, 1.45),
//        new ProportionalSplitter(5, 1.35)
//        );
//var zSplitParameters = new AxisSplitParameter(new[]
//        { -260d, -160d, -135d, -131d, -130d, -125d, -100d, 0d },
//        new ProportionalSplitter(5, 1 / 1.5),
//        new ProportionalSplitter(15, 1 / 1.48),
//        new UniformSplitter(156),
//        new UniformSplitter(39),
//        new UniformSplitter(195),
//        new ProportionalSplitter(15, 1.48),
//        new ProportionalSplitter(5, 1.5)
//        );

//var areas = new Area[]
//{
//    //скважина
//    new(0, new Node2D(0d, -260d), new Node2D(0.1, 0d)),
//    //первый слой
//    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
//    //второй слой
//    new(2, new Node2D(0.1, -130d), new Node2D(100.1, -100d)),
//    //искомый элемент
//    new(4, new Node2D(0.1, -131d), new Node2D(20.1, -130d)),
//    //третий слой
//    new(2, new Node2D(20.1, -131d), new Node2D(100.1, -130d)),
//    //четвертый слой
//    new(2, new Node2D(0.1, -160d), new Node2D(100.1, -131d)),
//    //пятый слой
//    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
//};

var trueGrid = gridBuilder2D
.SetRAxis(rSplitParameters)
.SetZAxis(zSplitParameters)
.SetAreas(areas)
.Build();

//var trueSigmas = new MaterialFactory
//(
//    new List<double> { 0.01, 0.025, 0.1, 0.2, 1d / 3, 0.5, 0d, 1d }
//);
var trueSigmas = new MaterialFactory
(
    new List<double> { 0.01, 0.025, 0.08, 0.1, 0.2, 1d / 3, 0.45, 0.5 }
);

var localBasisFunctionsProvider = new LocalBasisFunctionsProvider(trueGrid, new LinearFunctionsProvider());

var firstBoundaryProvider = new FirstBoundaryProvider(trueGrid);
var conditions = firstBoundaryProvider.GetConditions(trueGrid.Nodes.RLength - 1, trueGrid.Nodes.ZLength - 1);

// инициализируем источники и приёмники
////var current = 1d;
////var sources = new Source[59];
////var receivesrLine = new ReceiverLine[59];
////var truePotentialDifferences = new double[59];
////var centersZ = new double[59];

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

//for (var i = 0; i < 59; i++)
//{
//    sources[i] = new Source(new Node2D(0.05, -100 - 1 * i), current);
//    receivesrLine[i] = new ReceiverLine(
//        new Node2D(sources[i].Point.R, sources[i].Point.Z - 1),
//        new Node2D(sources[i].Point.R, sources[i].Point.Z - 2)
//    );
//    centersZ[i] = (sources[i].Point.Z + receivesrLine[i].PointN.Z) / 2;
//}
var directProblemSolver = new DirectProblemSolver();
//var resultO = new ResultIO("../DirectProblem/Results/");
// ищем решение для каждого источника
for (var i = 0; i < sources.Length; i++)
{
    var trueSolution = directProblemSolver
        .SetGrid(trueGrid)
        .SetMaterials(trueSigmas)
        .SetSource(sources[i])
        .SetFirstConditions(conditions)
        .Solve();

    //if (i == 31)
    //{
    //    resultO.WriteResult(trueSolution, "v2.dat");
    //}

    var femSolution = new FEMSolution(trueGrid, trueSolution, localBasisFunctionsProvider);

    var potentialM = femSolution.Calculate(receivesrLines[i].PointM);
    var potentialN = femSolution.Calculate(receivesrLines[i].PointN);

    //var potential = femSolution.Calculate(new Node2D(0.1, -130));

    truePotentialDifferences[i] = potentialM - potentialN;

    CourseHolder.GetInfo(i, 0);
    Console.WriteLine();
}
Console.WriteLine("DirectProblem solved!\n");
// задаём параметры области для обратной задачи

var sigmas = new[] { 0.01, 0.025, 0.08, 0.1, 0.2, 1d / 3, 0.45, 0.5 };

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

//var trueValues = new Vector(new[] { 0.05, 1d / 3 });
//var initialValues = new Vector(new[] { 0.005, 0.08 });
//var trueValues = new Vector(new[] { 1d / 3 });
//var initialValues = new Vector(new[] { 0.08 }); //50 итераций 0,3019425292860777
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

    //if (i == 31)
    //{
    //    resultO.WriteResult(solution, "v2_inverse.dat");
    //}

    foreach (var value in solution)
    {
        Console.WriteLine(value);
    }
