using Examify.Core.Interfaces;
using Examify.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Examify.Infrastructure
{
    public static class InfrastructureServiceCollectionExtensions
    {
        /// <summary>
        /// Register infrastructure-layer services (repositories, file storage, etc.).
        /// Add real registrations here as needed.
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Example:
            // services.AddScoped<IMyRepository, MyRepository>();
            return services;
        }
    }
}