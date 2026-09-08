using EduDataGen.DAL.Models;

namespace EduDataGen.Engine.Math;

public class GaussianSampler : IGaussianSampler
{
    private readonly Random _random;
    private double? _nextGaussian;

    public int Seed { get; }

    public GaussianSampler(int seed)
    {
        Seed = seed;
        _random = new Random(seed);
    }

    public double NextDouble()
    {
        return _random.NextDouble();
    }

    /// <summary>
    /// Generates a Gaussian (Normal) distribution sample using Box-Muller transform.
    /// </summary>
    public double SampleGaussian(double mean, double stdDev)
    {
        if (stdDev <= 0)
        {
            return mean;
        }

        if (_nextGaussian.HasValue)
        {
            double val = _nextGaussian.Value;
            _nextGaussian = null;
            return mean + stdDev * val;
        }

        double u1 = _random.NextDouble();
        while (u1 <= 0.0)
        {
            u1 = _random.NextDouble();
        }

        double u2 = _random.NextDouble();

        double radius = System.Math.Sqrt(-2.0 * System.Math.Log(u1));
        double theta = 2.0 * System.Math.PI * u2;

        double z0 = radius * System.Math.Cos(theta);
        double z1 = radius * System.Math.Sin(theta);

        _nextGaussian = z1;
        return mean + stdDev * z0;
    }

    public double SampleUniform(double min, double max)
    {
        if (min >= max) return min;
        return min + _random.NextDouble() * (max - min);
    }

    public int SampleIntegerUniform(int min, int max)
    {
        if (min >= max) return min;
        return _random.Next(min, max + 1);
    }

    public DiscreteGrade SampleDiscreteWeighted(IEnumerable<DiscreteGrade> grades)
    {
        var gradeList = grades.ToList();
        if (gradeList.Count == 0)
        {
            return new DiscreteGrade { GradeName = "Moderate", Factor = 1.0, Weight = 1.0 };
        }

        double totalWeight = gradeList.Sum(g => g.Weight <= 0 ? 1.0 : g.Weight);
        double roll = _random.NextDouble() * totalWeight;

        double cumulative = 0;
        foreach (var grade in gradeList)
        {
            cumulative += (grade.Weight <= 0 ? 1.0 : grade.Weight);
            if (roll <= cumulative)
            {
                return grade;
            }
        }

        return gradeList.Last();
    }

    public double SampleSeverityFactor(SeverityDistributionSettings settings)
    {
        double minFactor = settings.MinFactor;
        double maxFactor = settings.MaxFactor;

        if (minFactor >= maxFactor)
        {
            return minFactor;
        }

        string dist = settings.Distribution ?? "Uniform";

        if (dist.Equals("Discrete", StringComparison.OrdinalIgnoreCase) &&
            settings.DiscreteGrades != null &&
            settings.DiscreteGrades.Count > 0)
        {
            var selected = SampleDiscreteWeighted(settings.DiscreteGrades);
            return selected.Factor;
        }

        if (dist.Equals("Beta", StringComparison.OrdinalIgnoreCase))
        {
            // Symmetric triangular/Beta(2,2) approximation: (u1 + u2) / 2.0
            double betaSample = (_random.NextDouble() + _random.NextDouble()) / 2.0;
            return minFactor + betaSample * (maxFactor - minFactor);
        }

        return SampleUniform(minFactor, maxFactor);
    }
}
