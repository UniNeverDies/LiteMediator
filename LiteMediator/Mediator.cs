using Microsoft.Extensions.DependencyInjection;

namespace LiteMediator
{
    internal class Mediator(IServiceProvider serviceProvider) : IMediator
    {
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var requestType = request.GetType();
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

            // Resolve the handler
            dynamic handler = serviceProvider.GetRequiredService(handlerType);

            // Resolve pipeline behaviors
            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
            var behaviors = serviceProvider.GetServices(behaviorType).Cast<dynamic>().ToList();

            // Create the final delegate (handler call)
            RequestHandlerDelegate<TResponse> handlerDelegate = () =>
                handler.Handle((dynamic)request, cancellationToken);

            // Compose the behaviors (in reverse order)
            foreach (var behavior in behaviors.AsEnumerable().Reverse())
            {
                var next = handlerDelegate;
                handlerDelegate = () => behavior.Handle((dynamic)request, next, cancellationToken);
            }

            return await handlerDelegate();
        }

        public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
        {
            var notificationType = notification.GetType();
            var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

            var handlers = serviceProvider.GetServices(handlerType).Cast<dynamic>().ToList();

            foreach (var handler in handlers)
            {
                await handler.Handle((dynamic)notification, cancellationToken);
            }
        }
    }
}
