using System.Text.Json;
using EduDataGen.DAL.Models;
using EduDataGen.DAL.Workspace;

namespace EduDataGen.DAL.Repositories;

public interface IBlueprintRepository
{
    Task SaveBlueprintAsync(SimulationBlueprint blueprint, string? customFileName = null);
    Task<SimulationBlueprint?> LoadBlueprintAsync(string fileName);
    Task<IEnumerable<string>> ListBlueprintsAsync();
    Task<IEnumerable<string>> ListPresetsAsync();
}

public class BlueprintRepository : IBlueprintRepository
{
    private readonly IWorkspaceManager _workspaceManager;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public BlueprintRepository(IWorkspaceManager workspaceManager)
    {
        _workspaceManager = workspaceManager;
        _workspaceManager.InitializeWorkspace();
    }

    public async Task SaveBlueprintAsync(SimulationBlueprint blueprint, string? customFileName = null)
    {
        string fileName = customFileName ?? $"{blueprint.Scenario}_{blueprint.Seed}.json";
        string filePath = _workspaceManager.GetSanitizedPath("blueprints", fileName);
        string json = JsonSerializer.Serialize(blueprint, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<SimulationBlueprint?> LoadBlueprintAsync(string fileName)
    {
        string filePath = _workspaceManager.GetSanitizedPath("blueprints", fileName);
        if (!File.Exists(filePath))
        {
            filePath = _workspaceManager.GetSanitizedPath("presets", fileName);
            if (!File.Exists(filePath))
            {
                return null;
            }
        }

        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<SimulationBlueprint>(json, JsonOptions);
    }

    public Task<IEnumerable<string>> ListBlueprintsAsync()
    {
        if (!Directory.Exists(_workspaceManager.BlueprintsPath))
            return Task.FromResult(Enumerable.Empty<string>());

        var files = Directory.GetFiles(_workspaceManager.BlueprintsPath, "*.json")
                             .Select(Path.GetFileName)
                             .Where(f => f != null)
                             .Select(f => f!);
        return Task.FromResult(files);
    }

    public Task<IEnumerable<string>> ListPresetsAsync()
    {
        if (!Directory.Exists(_workspaceManager.PresetsPath))
            return Task.FromResult(Enumerable.Empty<string>());

        var files = Directory.GetFiles(_workspaceManager.PresetsPath, "*.json")
                             .Select(Path.GetFileName)
                             .Where(f => f != null)
                             .Select(f => f!);
        return Task.FromResult(files);
    }
}
