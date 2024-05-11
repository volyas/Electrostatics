using DirectProblem.GridGenerator.Intervals.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectProblem.GridGenerator.Intervals.Splitting;
    public readonly record struct UniformStepSplitter : IIntervalSplitter
    {
        private readonly double _step;

        public UniformStepSplitter(double step)
        {
            _step = step;
        }
        public IEnumerable<double> EnumerateValues(Interval interval)
        {
            var stepNumber = 0;
            var value = interval.Begin + stepNumber * _step;

            while (interval.Has(value))
            {
                yield return value;

                stepNumber++;
                value = interval.Begin + stepNumber * _step;

                if (!(interval.End < value)) continue;
            }
        }
    }
