using Microsoft.EntityFrameworkCore;
using InsuranceManager.Application.Services;
using InsuranceManager.Domain.Entities;
using InsuranceManager.Domain.Ports;
using InsuranceManager.Infrastructure.Persistence;
using InsuranceManager.Infrastructure.Adapters;
using InsuranceManager.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<InsuranceDbContext>(options =>
    options.UseSqlite("Data Source=insurance.db"));

builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
builder.Services.AddScoped<IPolicyRepository, PolicyRepository>();

builder.Services.AddScoped<ProposalService>();
builder.Services.AddScoped<PolicyService>();

var app = builder.Build();

// Add API Key authentication middleware
app.UseApiKeyAuthentication();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();

app.Run();