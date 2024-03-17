using DirectProblem.GridGenerator.Intervals.Core;

namespace DirectProblem.Calculus;

public class DoubleIntegralCalculator
{
    private readonly double[] _interpolationNodes = { -0.5773503, 0.5773503 };

    private readonly double[] _weights = { 1.0, 1.0 };

    private const int GaussMethodNumber = 2;

    private const int NumberOfSegments = 512;

    public double Calculate(Interval rInterval, Interval zInterval, Func<double, double, double> function)
    {
        var hr = rInterval.Length / NumberOfSegments;
        var hz = zInterval.Length / NumberOfSegments;

        var outerIntegralValue = 0.0;

        for (var i = 0; i < GaussMethodNumber; i++)
        {
            var sumOfOuterIntegral = 0.0;

            for (var r = 0; r < NumberOfSegments; r++)
            {
                var rI = (rInterval.Begin + r * hr + rInterval.Begin + (r + 1) * hr) / 2.0 + _interpolationNodes[i] * hr / 2.0;

                var innerIntegralValue = 0.0;

                for (var j = 0; j < GaussMethodNumber; j++)
                {
                    var sumOfInnerIntegral = 0.0;
                    for (var z = 0; z < NumberOfSegments; z++)
                    {
                        var zJ = (zInterval.Begin + z * hz + zInterval.Begin + (z + 1) * hz) / 2.0 + _interpolationNodes[j] * hz / 2.0;

                        sumOfInnerIntegral += hz * function(rI, zJ);
                    }

                    innerIntegralValue += sumOfInnerIntegral * _weights[j] / 2.0;
                }

                sumOfOuterIntegral += hr * innerIntegralValue;
            }
            outerIntegralValue += _weights[i] / 2.0 * sumOfOuterIntegral;
        }

        return outerIntegralValue;
    }
}