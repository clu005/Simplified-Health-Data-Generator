using EduDataGen.DAL.Models;
using EduDataGen.Engine.Domain;

namespace EduDataGen.Engine;

public interface IDatasetGenerator
{
    Task<DatasetGenerationResult> RenderDatasetAsync(
        SimulationBlueprint blueprint,
        int? overrideTotalRecords = null,
        int? overrideSeed = null,
        string? customTimestamp = null);
}
