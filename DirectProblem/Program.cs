using DirectProblem;
using DirectProblem.Core.GridComponents;
using DirectProblem.FEM;
using DirectProblem.GridGenerator;
using DirectProblem.IO;
using DirectProblem.TwoDimensional;
using DirectProblem.TwoDimensional.Assembling.Boundary;
using DirectProblem.TwoDimensional.Assembling.Local;
using DirectProblem.TwoDimensional.Parameters;
using System.Globalization;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

var gridBuilder2D = new GridBuilder2D();
var grid = Grids.GetModel7();
 

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
var receiverLines = new ReceiverLine[sources.Length];
var potentialDifferences = new double[sources.Length];
var centersZ = new double[sources.Length];

for (var i = 0; i < sources.Length; i++)
{
    sources[i] = new Source(new Node2D(0.05, 0 - 1 * i), current);
    receiverLines[i] = new ReceiverLine(
        new Node2D(sources[i].Point.R, sources[i].Point.Z - 1),
        new Node2D(sources[i].Point.R, sources[i].Point.Z - 2)
    );
    centersZ[i] = (sources[i].Point.Z + receiverLines[i].PointN.Z) / 2;
}

var localBasisFunctionsProvider = new LocalBasisFunctionsProvider(grid, new LinearFunctionsProvider());


var firstBoundaryProvider = new FirstBoundaryProvider(grid);
var conditions = firstBoundaryProvider.GetConditions(grid.Nodes.RLength - 1, grid.Nodes.ZLength - 1);


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
//                .SetFirstConditions(conditions)
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
    .SetGrid(grid)
    .SetMaterials(materialFactory)
    .SetSource(new Source(new Node2D(0.05, -5), 1))
    .SetFirstConditions(conditions)
    .Solve();
resultO.WriteResult(solution, "v2.dat");
var femSolution = new FEMSolution(grid, solution, localBasisFunctionsProvider);
for (var i = 0; i < sources.Length; i++)
{
    potentialDifferences[i] = femSolution.Calculate(new Node2D(0.1, -10 + i)); //не разности, а просто потенциалы
    Console.WriteLine(potentialDifferences[i]);
}

//

resultO.Write($"potentialDifferences.txt", potentialDifferences, centersZ);