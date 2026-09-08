using EduDataGen.DAL.Exporters;
using EduDataGen.DAL.Repositories;
using EduDataGen.DAL.Workspace;
using EduDataGen.Engine;
using EduDataGen.Engine.Clamping;
using EduDataGen.Engine.Math;
using EduDataGen.Engine.Pipeline;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Register DAL Services
builder.Services.AddSingleton<IWorkspaceManager, WorkspaceManager>();
builder.Services.AddSingleton<IBlueprintRepository, BlueprintRepository>();
builder.Services.AddSingleton<IDualTableCsvExporter, DualTableCsvExporter>();

// Register Engine Services
builder.Services.AddTransient<IStochasticPipeline, StochasticPipeline>();
builder.Services.AddTransient<IAttenuatedAccumulator, AttenuatedAccumulator>();
builder.Services.AddTransient<IPhysiologicalClamper, PhysiologicalClamper>();
builder.Services.AddTransient<IDatasetGenerator, DatasetGenerator>();

var app = builder.Build();

// Initialize Workspace
var workspaceManager = app.Services.GetRequiredService<IWorkspaceManager>();
workspaceManager.InitializeWorkspace();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
