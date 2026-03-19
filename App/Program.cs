using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
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

// Register the scaffolded DbContext so EF model matches the live database
builder.Services.AddDbContext<SportsVenuesScaffoldContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// register feature modules
builder.Services.AddFootball();
builder.Services.AddFormulaOne();
builder.Services.AddHandball();
builder.Services.AddVenues();
builder.Services.AddHockey();
builder.Services.AddVenueFixture();
builder.Services.AddHybridCache();

var app = builder.Build();

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
