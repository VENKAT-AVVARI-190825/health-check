using HealthCheck.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<HealthStatusService>();

var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Concat(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [])
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins.Length > 0 ? allowedOrigins : ["https://localhost"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("FrontendPolicy");

app.MapGet("/api/health", (HealthStatusService service) => Results.Ok(service.GetStatus()));
app.MapGet("/api/health/detail", (HealthStatusService service) => Results.Ok(service.GetDetail()));
app.MapHealthChecks("/healthz").AllowAnonymous();
app.MapHealthChecks("/admin/health").AllowAnonymous();

app.MapControllers();

app.Run();
