using FitnessAppBackend.Data;
using FitnessAppBackend.Interfaces;
using FitnessAppBackend.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllers();

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("JSON READ FAILED: The connection string is null!");
}

builder.Services.AddDbContext<AppDbContext>
    (options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFoodRepository, FoodRepository>();
builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IGoalRepository, GoalRepository>();

var app = builder.Build();


// Configure the HTTP request pipeline:

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
