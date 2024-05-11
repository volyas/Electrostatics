using DirectProblem.GridGenerator.Intervals.Core;

namespace DirectProblem.GridGenerator.Intervals.Splitting;

public class ProportionalSplitter : IIntervalSplitter
{
    public double DischargeRatio { get; }

    private readonly double _lengthCoefficient;

    public ProportionalSplitter(int steps, double dischargeRatio)
    {
        if (Math.Abs(dischargeRatio - 1d) < 1e-15)
            throw new NotSupportedException();

        DischargeRatio = dischargeRatio;
        _lengthCoefficient = (DischargeRatio - 1d) / (Math.Pow(DischargeRatio, steps) - 1d);
    }

    public IEnumerable<double> EnumerateValues(Interval interval)
    {
        var step = interval.Length * _lengthCoefficient;

        var stepNumber = 0;
        var value = interval.Begin;

        while (interval.Has(value))
        {
            yield return value;
            var nextValue = interval.Begin + step * (Math.Pow(DischargeRatio, stepNumber + 1) - 1d) / (DischargeRatio - 1d);

            value = nextValue;
            stepNumber++;
        }
    }
}