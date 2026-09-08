using EduDataGen.DAL.Models;

namespace EduDataGen.Engine.Clamping;

public interface IPhysiologicalClamper
{
    double ClampValue(double rawValue, MetricLimits limits, string distribution);
}
