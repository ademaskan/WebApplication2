using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Services;
using BaşarsoftStaj.Data;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(); // This is essential for controllers to work
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // This will help you visualize your API

// Add Entity Framework with PostgreSQL for EFC classes
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IPointService, PointServiceADO>();
builder.Services.AddScoped<IPointService, PointServiceEFC>();
builder.Services.AddScoped<IPointService, PointServiceStatic>();

// Add these lines after your existing IPointService registrations
builder.Services.AddScoped<PointServiceADO>();
builder.Services.AddScoped<PointServiceEFC>();
builder.Services.AddScoped<PointServiceStatic>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
