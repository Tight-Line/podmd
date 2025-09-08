using Microsoft.EntityFrameworkCore;
using PodMD.Api.Database;
using PodMD.Api.Endpoints;
using PodMD.Api.Exceptions;
using PodMD.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(o =>
{
    o.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance =
            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "PodMD API";
    config.Version = "1.0.0";
});

builder.Services.AddDbContext<AppDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("DbContext"));
});

builder.RegisterConfigs();

builder.Services.RegisterAppServices();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapApiKeyEndpoints();
app.MapConfigurationEndpoints();
app.MapClusterEndpoints();
app.MapJenkinsServerEndpoints();
app.MapKnowledgeBaseEndpoints();
app.MapDiagnoseEndpoints();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseEnvironmentSettings();

app.Run();