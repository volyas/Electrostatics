using DirectProblem;
using DirectProblem.Core.Boundary;
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
 .SetRAxis(new AxisSplitParameter(new[]
        { 0, 0.1, 5.1, 10.1 },
        new UniformSplitter(4),
        new UniformSplitter(2),
        new NonUniformSplitter(2, 1.05)
    ))
.SetZAxis(new AxisSplitParameter(new[]
        { -10d, -6d, -4d, 0d },
        new NonUniformSplitter(2, 0.05),
        new UniformSplitter(2),
        new NonUniformSplitter(2, 1.05))
)
   //вариант с новым разбиением
   .SetAreas(new Area[]
   {
       //скважина
       new(6, new Node2D(0d, -10d), new Node2D(0.1, 0d)),
       //первый слой
       new(6, new Node2D(0.1, -10d), new Node2D(10.1, 0d))
   })
   //   .SetRAxis(new AxisSplitParameter(new[]
   //        { 0, 0.1, 1.1, 20.1, 100.1 },
   //        new UniformSplitter(4),
   //        new UniformSplitter(40),
   //        new ProportionalSplitter(15, 1.45),
   //        new ProportionalSplitter(5, 1.35)
   //    ))
   //.SetZAxis(new AxisSplitParameter(new[]
   //        { -260d, -160d, -135d, -125d, -100d, 0d },
   //        new ProportionalSplitter(5, 1 / 1.5),
   //        new ProportionalSplitter(15, 1 / 1.48),
   //        new UniformSplitter(390),
   //        new ProportionalSplitter(15, 1.48),
   //        new ProportionalSplitter(5, 1.5))
   //)


   //годное разбиение, его можно брать
   //.SetRAxis(new AxisSplitParameter(new[]
   //        { 0, 0.1, 1.1, 20.1, 100.1 },
   //        new UniformSplitter(4),
   //        new UniformSplitter(40),
   //        new ProportionalSplitter(15, 1.45),
   //        new ProportionalSplitter(5, 1.35)
   //    ))
   //.SetZAxis(new AxisSplitParameter(new[]
   //        { -260d, -160d, -135d, -131d, -130d, -125d, -100d, 0d },
   //        new ProportionalSplitter(5, 1 / 1.5),
   //        new ProportionalSplitter(15, 1 / 1.48),
   //        new UniformSplitter(156),
   //        new UniformSplitter(39),
   //        new UniformSplitter(195),
   //        new ProportionalSplitter(15, 1.48),
   //        new ProportionalSplitter(5, 1.5))
   //)
   //   //вариант с новым разбиением
   //   .SetAreas(new Area[]
   //   {
   //       //скважина
   //       new(0, new Node2D(0d, -260d), new Node2D(0.1, 0d)),
   //       //первый слой
   //       new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //       //второй слой
   //       new(2, new Node2D(0.1, -130d), new Node2D(100.1, -100d)),
   //       //искомый элемент
   //       new(4, new Node2D(0.1, -131d), new Node2D(20.1, -130d)),
   //       //третий слой
   //       new(2, new Node2D(20.1, -131d), new Node2D(100.1, -130d)),
   //       //четвертый слой
   //       new(2, new Node2D(0.1, -160d), new Node2D(100.1, -131d)),
   //       //пятый слой
   //       new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //   })
   //однородное
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(6, new Node2D(0d, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(6, new Node2D(0.1, -260d), new Node2D(100.1, 0d))
   //})
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

//var sources = new Source[59];
//var receiverLines = new ReceiverLine[59];
//var potentialDifferences = new double[59];
//var centersZ = new double[59];

//for (var i = 0; i < 59; i++)
//{
//    sources[i] = new Source(new Node2D(0.05, -100 - 1 * i), current);
//    receiverLines[i] = new ReceiverLine(
//        new Node2D(sources[i].Point.R, sources[i].Point.Z - 1), new Node2D(sources[i].Point.R, sources[i].Point.Z - 2)
//    );
//    centersZ[i] = (sources[i].Point.Z + receiverLines[i].PointN.Z) / 2;
//}

var sources = new Source[10];
var receiverLines = new ReceiverLine[10];
var potentialDifferences = new double[10];
var centersZ = new double[10];

for (var i = 0; i < 10; i++)
{
    sources[i] = new Source(new Node2D(0.05, 0 - 1 * i), current);
    receiverLines[i] = new ReceiverLine(
        new Node2D(sources[i].Point.R, sources[i].Point.Z - 1), new Node2D(sources[i].Point.R, sources[i].Point.Z - 2)
    );
    centersZ[i] = (sources[i].Point.Z + receiverLines[i].PointN.Z) / 2;
}

var localBasisFunctionsProvider = new LocalBasisFunctionsProvider(grid, new LinearFunctionsProvider());


var firstBoundaryProvider = new FirstBoundaryProvider(grid);

var firstConditions = firstBoundaryProvider.GetConditions(10, 6);
//var firstConditions = firstBoundaryProvider.GetConditions(64, 430);
//var firstConditions = firstBoundaryProvider.GetConditions(12, 440);

var directProblemSolver = new DirectProblemSolver();

var resultO = new ResultIO("../DirectProblem/Results/");

//for (var i = 0; i < sources.Length; i++)
//{
//    var solution = directProblemSolver
//                .SetGrid(grid)
//                .SetMaterials(materialFactory)
//                .SetSource(sources[i])
//                ////.SetSource(sources[33])
//                //.SetSource(sources[5])
//                //.SetSource(new Source(new Node2D(0.05, -128), 1))
//                .SetFirstConditions(firstConditions)
//                .Solve();

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
    .Solve();
resultO.WriteResult(solution, "v2.dat");
var femSolution = new FEMSolution(grid, solution, localBasisFunctionsProvider);

var potential = new double[9];
for (var i = 0; i < 9; i++)
{
    potential[i] = femSolution.Calculate(new Node2D(0.1, -135 + i));
    Console.WriteLine(potential[i]);
}

//

resultO.Write($"potentialDifferences.txt", potentialDifferences, centersZ);