using BaşarsoftStaj.Interfaces;
using BaşarsoftStaj.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Add services to the container
builder.Services.AddControllers(); // This is essential for controllers to work
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // This will help you visualize your API
builder.Services.AddSingleton<IPointService, PointService>(); // 

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
