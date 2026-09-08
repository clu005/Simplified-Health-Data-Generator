using EduDataGen.DAL.Workspace;
using FluentAssertions;
using Xunit;

namespace EduDataGen.Tests.DalTests;

public class WorkspaceManagerTests : IDisposable
{
    private readonly string _tempWorkspaceDir;

    public WorkspaceManagerTests()
    {
        _tempWorkspaceDir = Path.Combine(Path.GetTempPath(), "EduDataGen_Test_" + Guid.NewGuid());
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempWorkspaceDir))
        {
            Directory.Delete(_tempWorkspaceDir, true);
        }
    }

    [Fact]
    public void InitializeWorkspace_ShouldCreateAllDirectories()
    {
        // Arrange
        var manager = new WorkspaceManager(_tempWorkspaceDir);

        // Act
        manager.InitializeWorkspace();

        // Assert
        Directory.Exists(manager.WorkspaceRoot).Should().BeTrue();
        Directory.Exists(manager.BlueprintsPath).Should().BeTrue();
        Directory.Exists(manager.PresetsPath).Should().BeTrue();
        Directory.Exists(manager.DatasetsPath).Should().BeTrue();
        Directory.Exists(manager.ConfigsPath).Should().BeTrue();
    }

    [Fact]
    public void GetSanitizedPath_WithValidFileName_ShouldReturnFullPathInSubDirectory()
    {
        // Arrange
        var manager = new WorkspaceManager(_tempWorkspaceDir);
        manager.InitializeWorkspace();

        // Act
        string path = manager.GetSanitizedPath("blueprints", "test_blueprint.json");

        // Assert
        path.Should().StartWith(manager.BlueprintsPath);
        Path.GetFileName(path).Should().Be("test_blueprint.json");
    }

    [Fact]
    public void GetSanitizedPath_WithDirectoryTraversal_ShouldSanitizeFileName()
    {
        // Arrange
        var manager = new WorkspaceManager(_tempWorkspaceDir);
        manager.InitializeWorkspace();

        // Act
        string path = manager.GetSanitizedPath("blueprints", "../../etc/passwd");

        // Assert
        path.Should().StartWith(manager.BlueprintsPath);
        Path.GetFileName(path).Should().Be("passwd");
    }
}
