using EduDataGen.DAL.Models;
using EduDataGen.Engine.Domain;

namespace EduDataGen.Engine.Math;

public interface IAttenuatedAccumulator
{
    double CalculateMetricValue(
        string metricName,
        BaselineMetric metric,
        List<ActiveConditionState> activeConditions,
        IGaussianSampler sampler,
        double gamma = 0.35);
}
