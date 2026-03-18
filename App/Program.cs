using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Sport.App.Data;
using Sport.App.Apis;
using Sport.App.Services;
using Sport.App.Football;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(); // will be using controllers.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// read connection string from config (user-secrets or env)
var connectionString = builder.Configuration.GetConnectionString("SportsVenues")
                       ?? "Server=127.0.0.1;Port=3306;Database=sports_venues;User=root;Password=;";

// register DbContext (Pomelo)
// Register the scaffolded DbContext so EF model matches the live database
builder.Services.AddDbContext<SportsVenuesScaffoldContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));


// register typed HttpClient for handball (separate host)
builder.Services.AddHttpClient<HandballClient>(client =>
{
    client.BaseAddress = new Uri("https://v1.handball.api-sports.io/");
});

builder.Services.AddHttpClient<FormulaOneClient>(client =>
{
    client.BaseAddress = new Uri("https://v1.formula-1.api-sports.io/");
});

builder.Services.AddFootball();

// register application services
builder.Services.AddScoped<IHandballService, HandballService>();
builder.Services.AddScoped<IFormulaOneService, FormulaOneService>();

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
