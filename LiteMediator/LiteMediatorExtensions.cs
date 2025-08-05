using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteMediator
{
    public static class LiteMediatorExtensions
    {
        //For Monolithic applications, Layered Architecture, or simple projects
        public static IServiceCollection AddLiteMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                services.RegisterHandlersWithMediator(assembly);
                services.RegisterBehaviorsWithMediator(assembly);
            }

            services.TryAddScoped<IMediator, Mediator>();
            return services;
        }

        //For Modular Monoliths, Microservices, or when you want to register shared and module-specific handlers separately
        public static IServiceCollection AddLiteMediatorCore(this IServiceCollection services, Assembly sharedAssembly)
        {
            services.RegisterHandlersWithMediator(sharedAssembly);
            services.RegisterBehaviorsWithMediator(sharedAssembly);
            services.TryAddScoped<IMediator, Mediator>();
            return services;
        }

        public static IServiceCollection AddLiteMediatorModule(this IServiceCollection services, Assembly moduleAssembly)
        {
            services.RegisterHandlersWithMediator(moduleAssembly);
            return services;
        }
    }
}
