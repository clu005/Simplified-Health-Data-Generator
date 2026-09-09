namespace EduDataGen.ConnectedService.Gemini;

using EduDataGen.DAL.Models;

public interface IGeminiBlueprintCompiler
{
    Task<SimulationBlueprint> CompileBlueprintAsync(
        string prompt,
        int? seed = null,
        int? totalRecords = null,
        double? healthyRatio = null,
        double? missingValueRate = null,
        CancellationToken cancellationToken = default);
}
