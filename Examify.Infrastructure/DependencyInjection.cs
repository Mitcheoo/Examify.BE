// Examify.Infrastructure/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using Examify.Core.Interfaces;
using Examify.Infrastructure.Repositories;

namespace Examify.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}