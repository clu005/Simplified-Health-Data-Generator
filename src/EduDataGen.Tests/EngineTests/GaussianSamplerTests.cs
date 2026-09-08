using EduDataGen.DAL.Models;
using EduDataGen.Engine.Math;
using FluentAssertions;

namespace EduDataGen.Tests.EngineTests;

public class GaussianSamplerTests
{
    [Fact]
    public void Sampler_IsDeterministic_ForSameSeed()
    {
        var sampler1 = new GaussianSampler(2026);
        var sampler2 = new GaussianSampler(2026);

        for (int i = 0; i < 100; i++)
        {
            sampler1.NextDouble().Should().Be(sampler2.NextDouble());
            sampler1.SampleGaussian(37.0, 0.5).Should().Be(sampler2.SampleGaussian(37.0, 0.5));
            sampler1.SampleUniform(10, 50).Should().Be(sampler2.SampleUniform(10, 50));
            sampler1.SampleIntegerUniform(1, 10).Should().Be(sampler2.SampleIntegerUniform(1, 10));
        }
    }

    [Fact]
    public void SampleGaussian_CalculatesMeanAndStdDevCloseToParameters_OverLargeSample()
    {
        var sampler = new GaussianSampler(12345);
        double targetMean = 100.0;
        double targetStdDev = 15.0;
        int sampleSize = 10000;

        List<double> samples = new();
        for (int i = 0; i < sampleSize; i++)
        {
            samples.Add(sampler.SampleGaussian(targetMean, targetStdDev));
        }

        double calculatedMean = samples.Average();
        double calculatedStdDev = System.Math.Sqrt(samples.Select(x => System.Math.Pow(x - calculatedMean, 2)).Average());

        calculatedMean.Should().BeApproximately(targetMean, 0.5);
        calculatedStdDev.Should().BeApproximately(targetStdDev, 0.5);
    }

    [Fact]
    public void SampleDiscreteWeighted_SelectsAccordingToWeights()
    {
        var sampler = new GaussianSampler(999);
        var grades = new List<DiscreteGrade>
        {
            new() { GradeName = "Mild", Factor = 0.3, Weight = 80 },
            new() { GradeName = "Severe", Factor = 1.0, Weight = 20 }
        };

        int mildCount = 0;
        int severeCount = 0;

        for (int i = 0; i < 1000; i++)
        {
            var selected = sampler.SampleDiscreteWeighted(grades);
            if (selected.GradeName == "Mild") mildCount++;
            else if (selected.GradeName == "Severe") severeCount++;
        }

        mildCount.Should().BeGreaterThan(severeCount);
        (mildCount + severeCount).Should().Be(1000);
    }
}
