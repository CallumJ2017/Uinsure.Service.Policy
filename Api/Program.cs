using Api.Extensions;
using Application;
using Asp.Versioning;
using Infrastructure.EntityFramework;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Configuration"));
builder.Configuration.AddJsonFile("ConnectionStrings.json");

builder.Services.AddEntityFramworkInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddFluentValidation();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Uinsure.Service.Policy",
        Version = "v1.0",
        Description = "API for Policy Management.",
    });
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
