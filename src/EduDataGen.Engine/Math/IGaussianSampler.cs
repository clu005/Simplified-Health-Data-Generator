using EduDataGen.DAL.Models;

namespace EduDataGen.Engine.Math;

public interface IGaussianSampler
{
    int Seed { get; }
    double NextDouble();
    double SampleGaussian(double mean, double stdDev);
    double SampleUniform(double min, double max);
    int SampleIntegerUniform(int min, int max);
    DiscreteGrade SampleDiscreteWeighted(IEnumerable<DiscreteGrade> grades);
    double SampleSeverityFactor(SeverityDistributionSettings settings);
}
