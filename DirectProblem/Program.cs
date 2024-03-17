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
   .SetRAxis(new AxisSplitParameter(
           new[] { 0, 0.1, 100.1 },
           new UniformSplitter(2),
           new UniformSplitter(10)
       )
   )
   .SetZAxis(new AxisSplitParameter(
           new[] { -260d, -160d, -130d, -100d, 0d },
           new UniformSplitter(100),
           new ProportionalSplitter(120, Math.Pow(0.95, 0.5)),
           new ProportionalSplitter(120, Math.Pow(1.05, 0.5)),
           new UniformSplitter(100)
       )
   )
   //искомый объект вплотную к скважине
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(2, new Node2D(0.1, -120.03858175448441), new Node2D(20.1, -100d)),
   //    new(2, new Node2D(20.1, -130d), new Node2D(100.1, -100d)),
   //    //искомый элемент
   //    new(4, new Node2D(0.1, -140.11800433374964), new Node2D(20.1, -120.03858175448441)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(20.1, -140.11800433374964)),
   //    new(2, new Node2D(20.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   //искомый объект близко к скважине (R = 10.1)
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(2, new Node2D(0.1, -130d), new Node2D(10.1, -100d)),
   //    new(2, new Node2D(10.1, -120.03858175448441), new Node2D(30.1, -100d)),
   //    new(2, new Node2D(30.1, -130d), new Node2D(100.1, -100d)),
   //    //искомый элемент
   //    new(4, new Node2D(10.1, -140.11800433374964), new Node2D(30.1, -120.03858175448441)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(10.1, -130d)),
   //    new(2, new Node2D(10.1, -160d), new Node2D(30.1, -140.11800433374964)),
   //    new(2, new Node2D(30.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d)),
   //})
   //искомый объект далеко от скважины (R = 40.1)
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(3, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(3, new Node2D(0.1, -130d), new Node2D(40.1, -100d)),
   //    new(3, new Node2D(40.1, -120.03858175448441), new Node2D(60.1, -100d)),
   //    new(3, new Node2D(60.1, -130d), new Node2D(100.1, -100d)),
   //    //искомый элемент
   //    new(4, new Node2D(40.1, -140.11800433374964), new Node2D(60.1, -120.03858175448441)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(40.1, -130d)),
   //    new(2, new Node2D(40.1, -160d), new Node2D(60.1, -140.11800433374964)),
   //    new(2, new Node2D(60.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(3, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   //искомые объекты вплотную к скважине
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(2, new Node2D(0.1, -120.03858175448441), new Node2D(20.1, -100d)),
   //    new(2, new Node2D(20.1, -130d), new Node2D(100.1, -100d)),
   //    //искомый элемент
   //    new(4, new Node2D(0.1, -130d), new Node2D(20.1, -120.03858175448441)),
   //    //искомый элемент
   //    new(4, new Node2D(0.1, -149.95696963486563), new Node2D(20.1, -140.11800433374964)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -140.11800433374964), new Node2D(20.1, -130d)),
   //    new(2, new Node2D(0.1, -160d), new Node2D(20.1, -149.95696963486563)),
   //    new(2, new Node2D(0.1, -160d), new Node2D(20.1, -130d)),
   //    new(2, new Node2D(20.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   //искомые объекы близко к скважине (R = 10.1)
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(3, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(3, new Node2D(0.1, -130d), new Node2D(10.1, -100d)),
   //    new(3, new Node2D(10.1, -120.03858175448441), new Node2D(30.1, -100d)),
   //    new(3, new Node2D(30.1, -130d), new Node2D(100.1, -100d))
   //    //искомый элемент
   //    new(4, new Node2D(10.1, -130d), new Node2D(30.1, -120.03858175448441)),
   //    //искомый элемент
   //    new(4, new Node2D(10.1, -149.95696963486563), new Node2D(30.1, -140.11800433374964)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(10.1, -130d)),
   //    new(2, new Node2D(10.1, -140.11800433374964), new Node2D(30.1, -130d)),
   //    new(2, new Node2D(10.1, -160d), new Node2D(30.1, -149.95696963486563)),
   //    new(2, new Node2D(30.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(3, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   //искомые объекты далеко от скважины (R = 40.1)
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(3, new Node2D(0.1, -130d), new Node2D(40.1, -100d)),
   //    new(3, new Node2D(40.1, -120.03858175448441), new Node2D(100.1, -100d))
   //    //искомый элемент
   //    new(4, new Node2D(40.1, -130d), new Node2D(60.1, -120.03858175448441)),
   //    //искомый элемент
   //    new(4, new Node2D(40.1, -149.95696963486563), new Node2D(60.1, -140.11800433374964)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(40.1, -130d)),
   //    new(2, new Node2D(40.1, -140.11800433374964), new Node2D(60.1, -130d)),
   //    new(2, new Node2D(40.1, -160d), new Node2D(60.1, -149.95696963486563)),
   //    new(2, new Node2D(60.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(3, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   //искомый объект маленький вплотную к скважине в начале третьего слоя
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(2, new Node2D(0.1, -130d), new Node2D(40.1, -100d)),
   //    new(2, new Node2D(40.1, -130d), new Node2D(60.1, -100d)),
   //    new(2, new Node2D(40.1, -130d), new Node2D(100.1, -100d)),
   //    //искомый элемент
   //    new(4, new Node2D(0.1, -134.04918983380892), new Node2D(40.1, -132.016289384065)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(40.1, -134.04918983380892)),
   //    new(2, new Node2D(0.1, -132.016289384065), new Node2D(40.1, -130d)),
   //    new(2, new Node2D(40.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   //искомый объект 2 маленький вплотную к скважине
   .SetAreas(new Area[]
   {
       //скважина
       new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
       //первый слой
       new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
       //второй слой
       new(2, new Node2D(0.1, -130d), new Node2D(20.1, -129.06444524208246)),
       new(2, new Node2D(0.1, -127.08331406406634), new Node2D(20.1, -100d)),
       new(2, new Node2D(20.1, -130d), new Node2D(100.1, -100d)),
       //искомый элемент
       new(4, new Node2D(0.1, -129.06444524208246), new Node2D(20.1, -127.08331406406634)),
       //третий слой
       new(2, new Node2D(0.1, -160d), new Node2D(100.1, -130d)),
       //четвертый слой
       new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   })
   //искомый объект 2 маленький близко к скважине
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(2, new Node2D(0.1, -130d), new Node2D(10.1, -100d)),
   //    new(2, new Node2D(10.1, -130d), new Node2D(30.1, -129.06444524208246)),
   //    new(2, new Node2D(10.1, -127.08331406406634), new Node2D(30.1, -100d)),
   //    new(2, new Node2D(30.1, -130d), new Node2D(100.1, -100d)),
   //    //искомый элемент
   //    new(4, new Node2D(10.1, -129.06444524208246), new Node2D(30.1, -127.08331406406634)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   ////искомые объект маленький далеко от скважины
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(2, new Node2D(0.1, -130d), new Node2D(40.1, -100d)),
   //    new(2, new Node2D(40.1, -127.08331406406634), new Node2D(60.1, -100d)),
   //    new(2, new Node2D(40.1, -130d), new Node2D(60.1, -129.06444524208246)),
   //    new(2, new Node2D(60.1, -130d), new Node2D(100.1, -100d)),
   //    //искомый элемент
   //    new(4, new Node2D(40.1, -129.06444524208246), new Node2D(60.1, -127.08331406406634)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   //искомые объекты вплотную к скважине
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(2, new Node2D(0.1, -130d), new Node2D(20.1, -129.06444524208246)),
   //    new(2, new Node2D(0.1, -127.08331406406634), new Node2D(20.1, -100d)),
   //    new(2, new Node2D(20.1, -130d), new Node2D(100.1, -100d)),
   //    //искомый элемент
   //    new(4, new Node2D(0.1, -134.04918983380892), new Node2D(20.1, -132.016289384065)),
   //    //искомый элемент
   //    new(4, new Node2D(0.1, -129.06444524208246), new Node2D(20.1, -127.08331406406634)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(20.1, -134.04918983380892)),
   //    new(2, new Node2D(0.1, -132.016289384065), new Node2D(20.1, -130d)),
   //    new(2, new Node2D(20.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   //искомые объекты близко к скважине
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(2, new Node2D(0.1, -130d), new Node2D(10.1, -100d)),
   //    new(2, new Node2D(10.1, -130d), new Node2D(30.1, -129.06444524208246)),
   //    new(2, new Node2D(10.1, -127.08331406406634), new Node2D(30.1, -100d)),
   //    new(2, new Node2D(30.1, -130d), new Node2D(100.1, -100d)),
   //    //искомый элемент
   //    new(4, new Node2D(10.1, -134.04918983380892), new Node2D(30.1, -132.016289384065)),
   //    //искомый элемент
   //    new(4, new Node2D(10.1, -129.06444524208246), new Node2D(30.1, -127.08331406406634)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(10.1, -130d)),
   //    new(2, new Node2D(10.1, -160d), new Node2D(30.1, -134.04918983380892)),
   //    new(2, new Node2D(10.1, -132.016289384065), new Node2D(30.1, -130d)),
   //    new(2, new Node2D(30.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   //искомые объекты далеко от скважины
   //.SetAreas(new Area[]
   //{
   //    //скважина
   //    new(0, new Node2D(0, -260d), new Node2D(0.1, 0d)),
   //    //первый слой
   //    new(1, new Node2D(0.1, -100d), new Node2D(100.1, 0d)),
   //    //второй слой
   //    new(2, new Node2D(0.1, -130d), new Node2D(40.1, -100d)),
   //    new(2, new Node2D(40.1, -130d), new Node2D(60.1, -129.06444524208246)),
   //    new(2, new Node2D(40.1, -127.08331406406634), new Node2D(60.1, -100d)),
   //    new(2, new Node2D(60.1, -130d), new Node2D(100.1, -100d)),
   //    //искомый элемент
   //    new(4, new Node2D(40.1, -134.04918983380892), new Node2D(60.1, -132.016289384065)),
   //    //искомый элемент
   //    new(4, new Node2D(40.1, -129.06444524208246), new Node2D(60.1, -127.08331406406634)),
   //    //третий слой
   //    new(2, new Node2D(0.1, -160d), new Node2D(40.1, -130d)),
   //    new(2, new Node2D(40.1, -160d), new Node2D(60.1, -134.04918983380892)),
   //    new(2, new Node2D(40.1, -132.016289384065), new Node2D(60.1, -130d)),
   //    new(2, new Node2D(60.1, -160d), new Node2D(100.1, -130d)),
   //    //четвертый слой
   //    new(1, new Node2D(0.1, -260d), new Node2D(100.1, -160d))
   //})
   //.SetAreas(new Area[]
   //{
   //    new(6, new Node2D(0, -260d), new Node2D(100.1, 0d)),
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

var firstConditions = firstBoundaryProvider.GetConditions(12, 440);

var directProblemSolver = new DirectProblemSolver(grid, globalAssembler, firstConditions, receiverLines);

var resultO = new ResultIO("../DirectProblem/Results/");

for (var i = 0; i < sources.Length; i++)
{
    var solution = directProblemSolver
        .SetSource(sources[i])
        ////.SetSource(sources[33])
        //.SetSource(sources[30])
        //.SetSource(new Source(new Node2D(0.05, -129), 1))
        .AssembleSLAE()
        .Solve();

    if(i == 28)
    {
        resultO.WriteResult(solution, "v2.dat");
    }

    var femSolution = new FEMSolution(grid, solution, localBasisFunctionsProvider);

    var potentialM = femSolution.Calculate(receiverLines[i].PointM);
    var potentialN = femSolution.Calculate(receiverLines[i].PointN);

    //var potential = femSolution.Calculate(new Node2D(2, -129));

    potentialDifferences[i] = potentialM - potentialN;

    CourseHolder.GetInfo(i, 0);
}

resultO.Write($"potentialDifferences.txt", potentialDifferences, centersZ);