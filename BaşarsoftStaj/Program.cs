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
builder.Services.AddScoped<IShapeRepository, ShapeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();




builder.Services.AddScoped<ShapeServiceADO>();
builder.Services.AddScoped<ShapeServiceEFC>();
builder.Services.AddScoped<ShapeServiceStatic>();

// Choose which service implementation to use for IPointService
// Simply change this line to switch between implementations:
builder.Services.AddScoped<IShapeService, ShapeServiceEFC>();


var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Your React app's address
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins); 

app.MapControllers();

app.Run();
