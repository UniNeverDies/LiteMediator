using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteMediator.Tests.Shared
{
    public class TestRequest : IRequest<string>
    {
        public string Name { get; set; } = "";
    }

    public class TestHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> Handle(TestRequest request, CancellationToken cancellationToken)
            => Task.FromResult($"Hello {request.Name}");
    }

    public class TestValidator : AbstractValidator<TestRequest>
    {
        public TestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name must not be empty.");
        }
    }

    public class TestNotification : INotification { }

    public class FirstHandler : INotificationHandler<TestNotification>
    {
        private readonly List<string> _called;

        public FirstHandler(List<string> called)
        {
            _called = called;
        }

        public Task Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            _called.Add("First");
            return Task.CompletedTask;
        }
    }

    public class SecondHandler : INotificationHandler<TestNotification>
    {
        private readonly List<string> _called;

        public SecondHandler(List<string> called)
        {
            _called = called;
        }

        public Task Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            _called.Add("Second");
            return Task.CompletedTask;
        }
    }


    public class TestBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            => next();
    }

}
