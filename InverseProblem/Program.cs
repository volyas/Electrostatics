using DirectProblem;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.GridGenerator;
using DirectProblem.GridGenerator.Intervals.Splitting;
using DirectProblem.TwoDimensional.Assembling.Boundary;
using DirectProblem.TwoDimensional.Assembling.Local;
using DirectProblem.TwoDimensional;
using DirectProblem.Core;
using DirectProblem.TwoDimensional.Assembling.Global;
using DirectProblem.TwoDimensional.Assembling;
using DirectProblem.TwoDimensional.Parameters;
using DirectProblem.IO;
using System.Reflection.Metadata;
using InverseProblem.Assembling;
using DirectProblem.Core.Base;
using InverseProblem;

// инициализируем и задаём истинную сетку
var gridBuilder2D = new GridBuilder2D();
var trueGrid = gridBuilder2D
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
.SetAreas(new Area[]
{
    //скважина
    new(0, new Node2D(0d, -260d), new Node2D(0.1, 0d)),
    //первый слой
    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
    //второй слой
    new(2, new Node2D(0.1, -130d), new Node2D(100.1, -100d)),
    //искомый элемент
    new(4, new Node2D(0.1, -131d), new Node2D(20.1, -130d)),
    //третий слой
    new(2, new Node2D(20.1, -131d), new Node2D(100.1, -130d)),
    //четвертый слой
    new(2, new Node2D(0.1, -160d), new Node2D(100.1, -131d)),
    //пятый слой
    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
})
.Build();

var trueSigmas = new MaterialFactory
(
    new List<double> { 0.5, 0.1, 0.05, 0.2, 1d / 3, 0d, 1d }
);

var localBasisFunctionsProvider = new LocalBasisFunctionsProvider(trueGrid, new LinearFunctionsProvider());

var firstBoundaryProvider = new FirstBoundaryProvider(trueGrid);
var firstConditions = firstBoundaryProvider.GetConditions(64, 430);

// инициализируем источники и приёмники
////var current = 1d;
////var sources = new Source[59];
////var receivesrLine = new ReceiverLine[59];
////var truePotentialDifferences = new double[59];
////var centersZ = new double[59];

var current = 1d;
var sources = new Source[1];
var receivesrLines = new ReceiverLine[1];
var truePotentialDifferences = new double[1];
var centersZ = new double[1];
for (var i = 0; i < 1; i++)
{
    sources[i] = new Source(new Node2D(0.05, -131 - 1 * i), current);
    receivesrLines[i] = new ReceiverLine(
        new Node2D(sources[i].Point.R, sources[i].Point.Z - 1),
        new Node2D(sources[i].Point.R, sources[i].Point.Z - 2)
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
var resultO = new ResultIO("../DirectProblem/Results/");
// ищем решение для каждого источника
var noise = 1.05d;
for (var i = 0; i < sources.Length; i++)
{
    var trueSolution = directProblemSolver
        .SetGrid(trueGrid)
        .SetMaterials(trueSigmas)
        .SetSource(sources[i])
        .SetFirstConditions(firstConditions)
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
}
Console.WriteLine("DirectProblem solved!\n");
// задаём параметры области для обратной задачи
var areas = new Area[]
{
    //скважина
    new(0, new Node2D(0d, -260d), new Node2D(0.1, 0d)),
    //первый слой
    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
    //второй слой
    new(2, new Node2D(0.1, -130d), new Node2D(100.1, -100d)),
    //искомый элемент
    new(4, new Node2D(0.1, -131d), new Node2D(20.1, -130d)),
    //третий слой
    new(2, new Node2D(20.1, -131d), new Node2D(100.1, -130d)),
    //четвертый слой
    new(2, new Node2D(0.1, -160d), new Node2D(100.1, -131d)),
    //пятый слой
    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
};
var sigmas = new[] { 0.5, 0.1, 0.04, 0.2, 1d/3, 0d, 1d };



var targetParameters = new InverseProblem.Assembling.Parameter[]
{
    //new (ParameterType.Sigma, 2),
    new (ParameterType.Sigma, 2)
};

//var trueValues = new Vector(new[] { 0.05, 1d / 3 });
//var initialValues = new Vector(new[] { 0.005, 0.08 });
//var trueValues = new Vector(new[] { 1d / 3 });
//var initialValues = new Vector(new[] { 0.08 }); //50 итераций 0,3019425292860777
var trueValues = new Vector(new[] { 0.05 });
var initialValues = new Vector(new[] { 0.04 });
var inverseProblemSolver = new InverseProblemSolver(gridBuilder2D);
    var solution = inverseProblemSolver
        .SetSource(sources)
        .SetReceiver(receivesrLines)
        .SetParameters(targetParameters, trueValues, initialValues)
        .SetTruePotentialDifference(truePotentialDifferences)
        .SetDirectProblemParameters(areas, sigmas, firstConditions)
        .Solve();

    //if (i == 31)
    //{
    //    resultO.WriteResult(solution, "v2_inverse.dat");
    //}

    foreach (var value in solution)
    {
        Console.WriteLine(value);
    }
