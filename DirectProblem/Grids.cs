using DirectProblem.Core.GridComponents;
using DirectProblem.Core;
using DirectProblem.GridGenerator;
using DirectProblem.GridGenerator.Intervals.Splitting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectProblem;
public class Grids
{
    private static readonly GridBuilder2D GridBuilder = new GridBuilder2D();
    public static Grid<Node2D> GetModel1()
    {
        var grid = GridBuilder
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

        return grid;
    }
    public static Grid<Node2D> GetHomogeneousModel()
    {
        var grid = GridBuilder
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
                new(6, new Node2D(0d, -260d), new Node2D(0.1, 0d)),
                //первый слой
                new(6, new Node2D(0.1, -260d), new Node2D(100.1, 0d))
            })
            .Build();

        return grid;
    }
    public static Grid<Node2D> GetModel2()
    {
        var grid = GridBuilder
               .SetRAxis(new AxisSplitParameter(new[]
                    { 0, 0.1, 1.1, 20.1, 100.1 },
                    new UniformSplitter(4),
                    new UniformSplitter(40),
                    new ProportionalSplitter(15, 1.45),
                    new ProportionalSplitter(5, 1.35)
                ))
            .SetZAxis(new AxisSplitParameter(new[]
                    { -260d, -160d, -135d, -125d, -100d, 0d },
                    new ProportionalSplitter(5, 1 / 1.5),
                    new ProportionalSplitter(15, 1 / 1.48),
                    new UniformSplitter(390),
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

        return grid;
    }
    
}
