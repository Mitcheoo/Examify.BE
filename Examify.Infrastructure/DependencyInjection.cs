// Examify.Infrastructure/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using Examify.Core.Interfaces;
using Examify.Infrastructure.Repositories;
using Examify.Infrastructure.Services;

namespace Examify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ✅ THÊM DÒNG NÀY - Đăng ký AI Service
        services.AddScoped<IAIGradingService, AIGradingService>();

        // ✅ THÊM DÒNG NÀY - File Storage Service
        services.AddScoped<IFileStorageService, FileStorageService>();

        return services;
    }
}