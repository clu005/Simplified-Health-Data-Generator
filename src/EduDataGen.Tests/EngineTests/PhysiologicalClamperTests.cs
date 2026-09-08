using EduDataGen.DAL.Models;
using EduDataGen.Engine.Clamping;
using FluentAssertions;

namespace EduDataGen.Tests.EngineTests;

public class PhysiologicalClamperTests
{
    [Fact]
    public void ClampValue_EnforcesHardMinAndHardMax()
    {
        var clamper = new PhysiologicalClamper();
        var limits = new MetricLimits
        {
            HardMin = 34.0,
            HardMax = 42.0
        };

        clamper.ClampValue(30.0, limits, "Gaussian").Should().Be(34.0);
        clamper.ClampValue(45.0, limits, "Gaussian").Should().Be(42.0);
        clamper.ClampValue(37.5, limits, "Gaussian").Should().Be(37.5);
    }

    [Fact]
    public void ClampValue_AppliesSoftSaturation_WhenThresholdExceeded()
    {
        var clamper = new PhysiologicalClamper();
        var limits = new MetricLimits
        {
            HardMin = 34.0,
            HardMax = 42.0,
            SoftSaturationThreshold = 40.0
        };

        double rawExcessive = 41.5;
        double clamped = clamper.ClampValue(rawExcessive, limits, "Gaussian");

        // Should be between soft saturation threshold (40.0) and HardMax (42.0), less than raw (41.5)
        clamped.Should().BeGreaterThan(40.0);
        clamped.Should().BeLessThan(rawExcessive);
        clamped.Should().BeLessThanOrEqualTo(42.0);
    }
}
