using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL database
var postgres = builder.AddPostgres("postgres");
var db = postgres.AddDatabase("jobdb");

// Add the Web project (Blazor Server)
builder.AddProject<JobCommandCenter_Web>("web")
    .WithReference(db);

// Add the Harvester project (must run as host process, not containerized)
// This is critical for CDP access to localhost:9222
builder.AddProject<JobCommandCenter_Harvester>("harvester")
    .WithReference(db);

builder.Build().Run();
