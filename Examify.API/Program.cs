using Examify.Application;
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Infrastructure;
using Examify.Infrastructure.Data;
using Examify.Infrastructure.External;
using Examify.Infrastructure.Seed;
using Examify.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using YourApp.Controllers;

var builder = WebApplication.CreateBuilder(args);

// ========== ADD SERVICES ==========

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Examify API",
        Version = "v1",
        Description = "API for Examify - VSTEP English Proficiency Test"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your token}' to authenticate."
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

// PayPal
builder.Services.Configure<PayPalOptions>(builder.Configuration.GetSection("PayPal"));
builder.Services.AddHttpClient();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Infrastructure (UnitOfWork, Repositories, Services)
builder.Services.AddInfrastructure();

// Application (MediatR, AutoMapper)
builder.Services.AddApplication();

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Memory Cache
builder.Services.AddMemoryCache();

// Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Examify",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ExamifyClient",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "examify-super-secret-key-1234567890!@#$%"))
    };
});

// Token Service
builder.Services.AddScoped<ITokenService, TokenService>();

// ========== EXTERNAL SERVICES ==========

// Gemini API
builder.Services.AddHttpClient<IGeminiApiClient, GeminiApiClient>();

// Whisper API (Speech-to-Text)
builder.Services.AddHttpClient<IWhisperApiClient, WhisperApiClient>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = config["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1/";
    if (!baseUrl.EndsWith("/"))
        baseUrl += "/";

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(60);
});

// DeepSeek API (Đã comment, không dùng)
// builder.Services.AddHttpClient<IDeepSeekApiClient, DeepSeekApiClient>();
builder.Services.AddHttpClient<IOpenAIClient, OpenAIClient>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var baseUrl = config["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1/";
    if (!baseUrl.EndsWith("/"))
        baseUrl += "/";

    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(60);
});

// ========== BUILD APP ==========
builder.Services.AddAutoMapper(typeof(Program).Assembly);
var app = builder.Build();

// ========== CONFIGURE PIPELINE ==========

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Khởi tạo database và roles
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// ✅ THỨ TỰ MIDDLEWARE ĐÚNG
app.UseCors("AllowAngularApp");     // 1. CORS
app.UseAuthentication();            // 2. Authentication
app.UseAuthorization();             // 3. Authorization
    
app.MapControllers();

app.Run();