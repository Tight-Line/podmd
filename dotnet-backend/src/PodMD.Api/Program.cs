using Microsoft.EntityFrameworkCore;
using PodMD.Api.Configuration;
using PodMD.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

// Add services to the container
builder.Services
    .AddDatabase(builder.Configuration)
    .AddIdentityConfiguration()
    .AddJwtConfiguration(builder.Configuration);

// Configure encryption settings
builder.Services.Configure<PodMD.Application.Configuration.EncryptionSettings>(
    builder.Configuration.GetSection("Encryption"));

// Configure jwt settings
builder.Services.Configure<PodMD.Application.Configuration.JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

// Configure LLM settings
builder.Services.Configure<PodMD.Application.Configuration.LlmSettings>(
    builder.Configuration.GetSection("LLM"));

// Configure MinIO settings
builder.Services.Configure<PodMD.Application.Configuration.MinioSettings>(
    builder.Configuration.GetSection("MinIO"));

// Register application services
builder.Services.AddScoped<PodMD.Application.Services.EncryptionService>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IKubeClusterRepository, PodMD.Infrastructure.Repositories.KubeClusterRepository>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IKubeClusterService, PodMD.Application.Services.KubeClusterService>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IJenkinsServersRepository, PodMD.Infrastructure.Repositories.JenkinsServersRepository>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IJenkinsServersService, PodMD.Application.Services.JenkinsServersService>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IKnowledgeBasesRepository, PodMD.Infrastructure.Repositories.KnowledgeBasesRepository>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IKnowledgeBasesService, PodMD.Application.Services.KnowledgeBasesService>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IKubeClientFactory, PodMD.Application.Services.KubeClientFactory>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IKubeLogService, PodMD.Application.Services.KubeLogService>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IAuthService, PodMD.Application.Services.AuthService>();
builder.Services.AddScoped<PodMD.Application.Analysis.ILlmClient, PodMD.Infrastructure.Analysis.LlmClient>();
builder.Services.AddScoped<PodMD.Application.Analysis.IAnalysisService, PodMD.Application.Analysis.AnalysisService>();

// File storage services
builder.Services.AddScoped<PodMD.Domain.Interfaces.IFileStorage, PodMD.Application.Services.MinioFileStorage>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IKnowledgeFileRepository, PodMD.Infrastructure.Repositories.KnowledgeFileRepository>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IKnowledgeFileService, PodMD.Application.Services.KnowledgeFileService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PodMD.Infrastructure.Persistence.ApplicationDbContext>()
    .AddCheck<LlmHealthCheck>("LLM Service", Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PodMD.Infrastructure.Persistence.ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
