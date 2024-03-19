using DirectProblem;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.GridGenerator;
using DirectProblem.GridGenerator.Intervals.Splitting;
using DirectProblem.IO;
using DirectProblem.TwoDimensional;
using DirectProblem.TwoDimensional.Assembling;
using DirectProblem.TwoDimensional.Assembling.Boundary;
using DirectProblem.TwoDimensional.Assembling.Global;
using DirectProblem.TwoDimensional.Assembling.Local;
using DirectProblem.TwoDimensional.Parameters;
using System.Globalization;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

var gridBuilder2D = new GridBuilder2D();

//-110.24319388881395
//-120.03858175448441
//-140.11800433374964
//-149.95696963486563
//-122.1055746274845
//-127.08331406406634
//-129.06444524208246

var grid = gridBuilder2D
   //.SetRAxis(new AxisSplitParameter(
   //        new[] { 0, 0.1, 10.1, 20.1, 400.1 },
   //        new UniformSplitter(10),
   //        new UniformSplitter(50),
   //        new ProportionalSplitter(15, 1.95),
   //        new ProportionalSplitter(5, 1.55)
   //    )
   //)
   //.SetZAxis(new AxisSplitParameter(
   //        new[] { -260d, -160d, -135d,  -125d, -100d, 0d },
   //        new ProportionalSplitter(5, 1/1.5),
   //        new ProportionalSplitter(5, 1/1.95),
   //        new UniformSplitter(50),
   //        new ProportionalSplitter(5, 1.95),
   //        new ProportionalSplitter(5, 1.5)
   //    )
   //)
   //более менее сетка по точности, но считает долго
   //.SetRAxis(new AxisSplitParameter(
   //        new[] { 0, 0.1, 2.1, 20.1, 100.1 },
   //        new UniformSplitter(10),
   //        new UniformSplitter(35),
   //        new ProportionalSplitter(15, 1.95),
   //        new ProportionalSplitter(5, 1.55)
   //    )
   //)
   //.SetZAxis(new AxisSplitParameter(
   //        new[] { -260d, -160d, -135d, -125d, -100d, 0d },
   //        new ProportionalSplitter(5, 1 / 1.5),
   //        new ProportionalSplitter(5, 1 / 1.95),
   //        new UniformSplitter(100),
   //        new ProportionalSplitter(5, 1.95),
   //        new ProportionalSplitter(5, 1.5)
   //    )
   //)
   .SetRAxis(new AxisSplitParameter(new[]
        { 0, 0.1, 2.1, 20.1, 100.1 },
        new UniformSplitter(4),
        new UniformSplitter(80),
        new ProportionalSplitter(15, 1.35),
        new ProportionalSplitter(5, 1.55)
    ))
.SetZAxis(new AxisSplitParameter(new[]
        { -260d, -160d, -135d, -125d, -100d, 0d },
        new ProportionalSplitter(5, 1 / 1.5),
        new ProportionalSplitter(10, 1 / 1.55),
        new UniformSplitter(400),
        new ProportionalSplitter(10, 1.55),
        new ProportionalSplitter(5, 1.5))
)
   //// .SetRAxis(new AxisSplitParameter(
   //        new[] { 0, 0.1, 5.1, 20.1, 100.1 },
   //        new UniformSplitter(2),
   //        new UniformSplitter(25),
   //        new ProportionalSplitter(10, 1.55),
   //        new ProportionalSplitter(5, 1.95)
   //    )
   //)
   //.SetZAxis(new AxisSplitParameter(
   //        new[] { -260d, -160d, -135, -130d, -129d, -125, -100d, 0d },
   //        new ProportionalSplitter(5, 0.05),
   //        new ProportionalSplitter(10, Math.Pow(0.05, 0.125)),
   //        new UniformSplitter(25),
   //        new UniformSplitter(5),
   //        new UniformSplitter(20),
   //        new ProportionalSplitter(10, Math.Pow(1.95, 0.125)),
   //        new ProportionalSplitter(5, 1.95)
   //    )
   //)
   //вариант с новым разбиением
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0d, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(2, new Node2D(0.1, -129d), new Node2D(100.1, -100d)),
   //    //искомый элемент
   //    new(4, new Node2D(0.1, -130d), new Node2D(20.1, -129d)),
   //    //третий слой
   //    new(2, new Node2D(20.1, -130d), new Node2D(100.1, -129d)),
   //    //четвертый слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(100.1, -130d)),
   //    //пятый слой
   //    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   //однородное
   .SetAreas(new Area[]
   {
       //скважина
       new(6, new Node2D(0d, -260d), new Node2D(0.1, 0d)),
       //первый слой
       new(6, new Node2D(0.1, -260d), new Node2D(100.1, 0d))
   })
   .Build();

var gridO = new GridIO("../DirectProblem/Results/");
gridO.WriteMaterials(grid, "nvkat2d.dat");
gridO.WriteElements(grid, "nvtr.dat");
gridO.WriteNodes(grid, "rz.dat");

var materialFactory = new MaterialFactory
(
    new List<double> { 0.5, 0.1, 0.05, 0.2, 1d / 3, 0d, 1d }
);

var current = 1d;

var sources = new Source[59];
var receiverLines = new ReceiverLine[59];
var potentialDifferences = new double[59];
var centersZ = new double[59];

for (var i = 0; i < 59; i++)
{
    sources[i] = new Source(new Node2D(0.05, -100 - 1 * i), current);
    receiverLines[i] = new ReceiverLine(
        new Node2D(sources[i].Point.R, sources[i].Point.Z - 1), new Node2D(sources[i].Point.R, sources[i].Point.Z - 2)
    );
    centersZ[i] = (sources[i].Point.Z + receiverLines[i].PointN.Z) / 2;
}

var localBasisFunctionsProvider = new LocalBasisFunctionsProvider(grid, new LinearFunctionsProvider());

var localAssembler =
    new LocalAssembler(
        new LocalMatrixAssembler(grid),
        materialFactory);

var inserter = new Inserter();
var globalAssembler = new GlobalAssembler<Node2D>(grid, new MatrixPortraitBuilder(), localAssembler, inserter,
    new GaussExcluder(), localBasisFunctionsProvider);

var firstBoundaryProvider = new FirstBoundaryProvider(grid);

var firstConditions = firstBoundaryProvider.GetConditions(104, 430);
//var firstConditions = firstBoundaryProvider.GetConditions(12, 440);

var directProblemSolver = new DirectProblemSolver(grid, globalAssembler, firstConditions, receiverLines);

var resultO = new ResultIO("../DirectProblem/Results/");

//for (var i = 0; i < sources.Length; i++)
//{
//    var solution = directProblemSolver
//        .SetSource(sources[i])
//        ////.SetSource(sources[33])
//        //.SetSource(sources[30])
//        //.SetSource(new Source(new Node2D(0.05, -128), 1))
//        .AssembleSLAE()
//        .Solve();

//    if (i == 31)
//    {
//        resultO.WriteResult(solution, "v2.dat");
//    }

//    var femSolution = new FEMSolution(grid, solution, localBasisFunctionsProvider);

//    var potentialM = femSolution.Calculate(receiverLines[i].PointM);
//    var potentialN = femSolution.Calculate(receiverLines[i].PointN);

//    //var potential = femSolution.Calculate(new Node2D(0.1, -130));

//    potentialDifferences[i] = potentialM - potentialN;

//    CourseHolder.GetInfo(i, 0);
//}

//для вычисления значения в точке

var solution = directProblemSolver
    .SetSource(new Source(new Node2D(0.05, -131), 1))
    .AssembleSLAE()
    .Solve();

var femSolution = new FEMSolution(grid, solution, localBasisFunctionsProvider);

var potential = new double[9];
for (var i = 0; i < 9; i++)
{
    potential[i] = femSolution.Calculate(new Node2D(0.1, -135 + i));
    Console.WriteLine(potential[i]);
}

//

resultO.Write($"potentialDifferences.txt", potentialDifferences, centersZ);