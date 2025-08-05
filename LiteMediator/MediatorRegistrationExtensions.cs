using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LiteMediator
{
    internal static class MediatorRegistrationExtensions
    {
        public static IServiceCollection RegisterHandlersWithMediator(this IServiceCollection services, Assembly assembly)
        {

            // Register IRequestHandler<,>
            var handlerTypes = assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
                .Where(t => t.Interface.IsGenericType && 
                            t.Interface.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            foreach (var item in handlerTypes)
            {
                services.AddScoped(item.Interface, item.Type);
            }

            // INotificationHandler<>
            var notificationHandlers = assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
                .Where(t => t.Interface.IsGenericType &&
                            t.Interface.GetGenericTypeDefinition() == typeof(INotificationHandler<>));

            foreach (var item in notificationHandlers)
            {
                services.AddScoped(item.Interface, item.Type);
            }

            return services;
        }
        public static IServiceCollection RegisterBehaviorsWithMediator(this IServiceCollection services, Assembly assembly)
        {
            // Register IPipelineBehavior<,>
            var behaviorTypes = assembly
                   .GetTypes()
                   .Where(t =>
                       !t.IsAbstract &&
                       !t.IsInterface
                   )
                   .SelectMany(t => t.GetInterfaces(), (impl, iface) => new { impl, iface })
                   .Where(x =>
                       x.iface.IsGenericType &&
                       x.iface.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)
                   );

            foreach (var bt in behaviorTypes)
            {
                if (bt.impl.IsGenericTypeDefinition)
                {
                    // Register open generics like ValidationBehavior<,>
                    services.AddScoped(typeof(IPipelineBehavior<,>), bt.impl);
                }
                else
                {
                    // Register closed generics like LoggingBehavior<CreateUserCommand, string>
                    services.AddScoped(bt.iface, bt.impl);
                }
            }

            return services;
        }
    }
}
