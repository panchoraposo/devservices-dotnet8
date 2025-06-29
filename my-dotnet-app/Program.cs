using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

string connectionString;

if (environment == "DevSpaces")
{
    var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "postgres";
    var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
    var db = Environment.GetEnvironmentVariable("DB_NAME") ?? "moviesdb";
    var user = Environment.GetEnvironmentVariable("DB_USER") ?? "devuser";
    var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "devpass";

    connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={password}";
    Console.WriteLine($"🔧 ConnectionString = {connectionString}");
}
else
{
    var postgres = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    await postgres.StartAsync();
    connectionString = postgres.GetConnectionString();
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment() || environment == "DevSpaces" || environment == "Local")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Movies API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.MapGet("/", () => "✅ .NET Dev Service activo con PostgreSQL");
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    context.Database.EnsureCreated();
    DbInitializer.Seed(context);
}

app.Run();