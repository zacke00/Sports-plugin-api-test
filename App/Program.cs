using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Data.Services;
using Sport.App.Football;
using Sport.App.FormulaOne;
using Sport.App.Handball;
using Sport.App.Hockey;
using Sport.App.Venues;
using Sport.App.VenueFixture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// read connection string from config (user-secrets or env)
var connectionString = builder.Configuration.GetConnectionString("SportsVenues")
                       ?? "Server=127.0.0.1;Port=3306;Database=sports_venues;User=root;Password=;";

// Register the code-first DbContext
builder.Services.AddDbContext<SportsVenuesContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysql =>
    {
        mysql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
        mysql.CommandTimeout(30);
    })
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

// register feature modules
builder.Services.AddFootball();
builder.Services.AddFormulaOne();
builder.Services.AddHandball();
builder.Services.AddVenues();
builder.Services.AddHockey();
builder.Services.AddVenueFixture();
builder.Services.AddHybridCache();
builder.Services.AddHostedService<IndexOptimizationService>();
builder.Services.AddHostedService<FixtureSyncService>();

var app = builder.Build();

// Apply pending EF Core migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SportsVenuesContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithName("Health");

app.Run();
