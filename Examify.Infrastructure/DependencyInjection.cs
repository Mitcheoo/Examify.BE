// Examify.Infrastructure/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using Examify.Core.Interfaces;
using Examify.Infrastructure.Repositories;
using Examify.Infrastructure.Services;
using Examify.Infrastructure.External;

namespace Examify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        // ✅ CHỈ ĐĂNG KÝ SERVICE, KHÔNG ĐĂNG KÝ CLIENT
        services.AddScoped<IAIGradingService, AIGradingService>();

        return services;
    }
}