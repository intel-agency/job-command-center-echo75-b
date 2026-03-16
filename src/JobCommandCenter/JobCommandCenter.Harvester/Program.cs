using JobCommandCenter.Data;
using JobCommandCenter.Harvester.Workers;
using JobCommandCenter.Shared.Models;
using JobCommandCenter.Shared.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add database context with connection pooling and retry logic
var connectionString = builder.Configuration.GetConnectionString("jobdb")
    ?? throw new InvalidOperationException("Connection string 'jobdb' not found.");
builder.Services.AddJobCommandCenterData(connectionString);

// Add scoring engine with default configuration
builder.Services.AddSingleton(new ScoringConfig
{
    RemotePoints = 50,
    TopApplicantPoints = 30,
    PromotedPenalty = 20,
    KeywordWeights = new Dictionary<string, int>
    {
        { "senior", 10 },
        { "lead", 15 },
        { "architect", 20 }
    }
});
builder.Services.AddSingleton<ScoringEngine>();

// Add the harvester worker
builder.Services.AddHostedService<HarvestWorker>();

var host = builder.Build();
host.Run();
