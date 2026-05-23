using Examify.Application;
using Examify.Core.Interfaces;
using Examify.Infrastructure;
using Examify.Infrastructure.Data;
using Examify.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Oliver Scheer Sample API",
        Version = "v1",
        Description = "API to to demonstrate some features."
    });

    // Add JWT-Authentication in Swagger UI
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Geben Sie 'Bearer {Token}' ein, um sich zu authentifizieren."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Swagger Start
    // JSON: /swagger/v1/swagger.json
    // UI: /swagger
    app.UseSwagger();
    app.UseSwaggerUI();
    // Swagger End

    // Scalar Start
    // UI: /scalar/v1
    app.MapScalarApiReference();
    // JSON  --> /scalar/v1
    app.MapOpenApi();
    // Scalar end
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();