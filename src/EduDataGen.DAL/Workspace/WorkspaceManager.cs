namespace EduDataGen.DAL.Workspace;

public interface IWorkspaceManager
{
    string WorkspaceRoot { get; }
    string BlueprintsPath { get; }
    string PresetsPath { get; }
    string DatasetsPath { get; }
    string ConfigsPath { get; }
    void InitializeWorkspace();
    string GetSanitizedPath(string subDirectory, string fileName);
}

public class WorkspaceManager : IWorkspaceManager
{
    private readonly string _workspaceRoot;

    public string WorkspaceRoot => _workspaceRoot;
    public string BlueprintsPath => Path.Combine(_workspaceRoot, "blueprints");
    public string PresetsPath => Path.Combine(_workspaceRoot, "presets");
    public string DatasetsPath => Path.Combine(_workspaceRoot, "datasets");
    public string ConfigsPath => Path.Combine(_workspaceRoot, "configs");

    public WorkspaceManager(string? workspaceRoot = null)
    {
        _workspaceRoot = string.IsNullOrWhiteSpace(workspaceRoot)
            ? Path.Combine(Directory.GetCurrentDirectory(), "EduDataGen_Workspace")
            : Path.GetFullPath(workspaceRoot);
    }

    public void InitializeWorkspace()
    {
        Directory.CreateDirectory(BlueprintsPath);
        Directory.CreateDirectory(PresetsPath);
        Directory.CreateDirectory(DatasetsPath);
        Directory.CreateDirectory(ConfigsPath);
    }

    public string GetSanitizedPath(string subDirectory, string fileName)
    {
        string safeFileName = Path.GetFileName(fileName);
        string targetDir = Path.GetFullPath(Path.Combine(_workspaceRoot, subDirectory));

        if (!targetDir.StartsWith(_workspaceRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Directory traversal attempt blocked.");
        }

        Directory.CreateDirectory(targetDir);
        string fullPath = Path.GetFullPath(Path.Combine(targetDir, safeFileName));

        if (!fullPath.StartsWith(targetDir, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Path traversal attempt blocked.");
        }

        return fullPath;
    }
}
