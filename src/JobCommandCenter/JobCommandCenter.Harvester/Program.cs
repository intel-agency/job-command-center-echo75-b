using JobCommandCenter.Data;
using JobCommandCenter.Harvester.Configuration;
using JobCommandCenter.Harvester.Services;
using JobCommandCenter.Harvester.Workers;
using JobCommandCenter.Shared.Models;
using JobCommandCenter.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

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

// Configure Chrome CDP options
builder.Services.AddSingleton<ChromeCdpOptions>(sp =>
{
    var options = new ChromeCdpOptions();
    builder.Configuration.GetSection("ChromeCdp").Bind(options);
    return options;
});

// Register Chrome validator
builder.Services.AddHttpClient<IChromeValidator, ChromeValidator>();

// Register CDP connection factory
builder.Services.AddSingleton<ICdpConnectionFactory, CdpConnectionFactory>();

// Register tab manager
builder.Services.AddSingleton<ITabManager, TabManager>();

// Add the harvester worker
builder.Services.AddHostedService<HarvestWorker>();

var host = builder.Build();
host.Run();
