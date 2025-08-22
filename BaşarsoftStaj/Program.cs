using System.Text.Json;
using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Services;
using BaşarsoftStaj.Data;
using Microsoft.EntityFrameworkCore;
using BaşarsoftStaj.Utils;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Spatial4n.Shapes.Nts;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new GeometryConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 




var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseNetTopologySuite()));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IPointRepository, PointRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();




builder.Services.AddScoped<PointServiceADO>();
builder.Services.AddScoped<PointServiceEFC>();
builder.Services.AddScoped<PointServiceStatic>();

// Choose which service implementation to use for IPointService
// Simply change this line to switch between implementations:
builder.Services.AddScoped<IPointService, PointServiceEFC>();

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
