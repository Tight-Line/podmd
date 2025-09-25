using PodMD.Api.Configuration;
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

// Register application services
builder.Services.AddScoped<PodMD.Application.Services.EncryptionService>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IKubeClusterRepository, PodMD.Infrastructure.Repositories.KubeClusterRepository>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IKubeClusterService, PodMD.Application.Services.KubeClusterService>();
builder.Services.AddScoped<PodMD.Application.Interfaces.IAuthService, PodMD.Application.Services.AuthService>();

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
    .AddDbContextCheck<PodMD.Infrastructure.Persistence.ApplicationDbContext>();

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
