using JobCommandCenter.Data;
using JobCommandCenter.Harvester.Workers;
using JobCommandCenter.Shared.Models;
using JobCommandCenter.Shared.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("jobdb")));

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
