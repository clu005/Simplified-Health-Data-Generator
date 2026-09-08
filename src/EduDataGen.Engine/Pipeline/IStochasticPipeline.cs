using EduDataGen.DAL.Models;
using EduDataGen.Engine.Domain;
using EduDataGen.Engine.Math;

namespace EduDataGen.Engine.Pipeline;

public interface IStochasticPipeline
{
    PatientSample DeterminePathogenesis(SimulationBlueprint blueprint, IGaussianSampler sampler, string patientId);
}
