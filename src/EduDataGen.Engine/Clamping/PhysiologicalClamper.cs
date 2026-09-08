using EduDataGen.DAL.Models;

namespace EduDataGen.Engine.Clamping;

public class PhysiologicalClamper : IPhysiologicalClamper
{
    public double ClampValue(double rawValue, MetricLimits limits, string distribution)
    {
        double hardMin = limits.HardMin;
        double hardMax = limits.HardMax;
        double val = rawValue;

        // Apply Soft Saturation curve if threshold is defined and exceeded
        if (limits.SoftSaturationThreshold.HasValue &&
            limits.SoftSaturationThreshold.Value < hardMax &&
            val > limits.SoftSaturationThreshold.Value)
        {
            double soft = limits.SoftSaturationThreshold.Value;
            double delta = val - soft;
            double span = hardMax - soft;
            if (span > 0)
            {
                val = soft + span * (1.0 - System.Math.Exp(-delta / span));
            }
        }

        // Hard clamping
        if (val < hardMin) val = hardMin;
        if (val > hardMax) val = hardMax;

        // Integer distribution rounding vs continuous rounding
        if (!string.IsNullOrEmpty(distribution) &&
            distribution.Equals("IntegerUniform", StringComparison.OrdinalIgnoreCase))
        {
            return System.Math.Round(val);
        }

        return System.Math.Round(val, 2);
    }
}
