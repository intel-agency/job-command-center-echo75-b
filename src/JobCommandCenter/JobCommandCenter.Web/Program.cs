using JobCommandCenter.Data;
using JobCommandCenter.Shared.Models;
using JobCommandCenter.Shared.Services;
using JobCommandCenter.Web.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults (OpenTelemetry, health checks, etc.)
builder.AddServiceDefaults();

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

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map default health check endpoint
app.MapHealthChecks("/health");

app.Run();
