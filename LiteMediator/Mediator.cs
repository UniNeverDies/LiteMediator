using Microsoft.Extensions.DependencyInjection;

namespace LiteMediator
{
    internal class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;
        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var requestType = request.GetType();
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

            // Resolve the handler
            dynamic handler = _serviceProvider.GetRequiredService(handlerType);

            // Resolve pipeline behaviors
            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
            var behaviors = _serviceProvider.GetServices(behaviorType).Cast<dynamic>().ToList();

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

            var handlers = _serviceProvider.GetServices(handlerType).Cast<dynamic>().ToList();

            foreach (var handler in handlers)
            {
                await handler.Handle((dynamic)notification, cancellationToken);
            }
        }
    }
}
