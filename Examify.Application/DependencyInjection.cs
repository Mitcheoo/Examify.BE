// Examify.Application/DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Examify.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Đăng ký AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Đăng ký MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        return services;
    }
}