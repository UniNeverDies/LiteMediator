using FluentAssertions;
using LiteMediator.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteMediator.Tests.Unit
{
    public class PipelineBehaviorSequenceTests
    {

        public class BehaviorA<TRequest, TResponse>(List<string> log) : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
        {
            public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            {
                log.Add("Before A");
                var response = await next();
                log.Add("After A");
                return response;
            }
        }

        public class BehaviorB<TRequest, TResponse>(List<string> log) : IPipelineBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
        {
            public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            {
                log.Add("Before B");
                var response = await next();
                log.Add("After B");
                return response;
            }
        }

        [Fact]
        public async Task Behaviors_Execute_In_Correct_Order()
        {
            var log = new List<string>();

            var services = new ServiceCollection();
            services.AddSingleton(log);
            services.AddScoped(typeof(IRequestHandler<TestRequest, string>), typeof(TestHandler));
            services.AddScoped(typeof(IPipelineBehavior<TestRequest, string>), typeof(BehaviorA<TestRequest, string>));
            services.AddScoped(typeof(IPipelineBehavior<TestRequest, string>), typeof(BehaviorB<TestRequest, string>));
            services.AddScoped<IMediator, Mediator>();

            var sp = services.BuildServiceProvider();
            var mediator = sp.GetRequiredService<IMediator>();

            var result = await mediator.Send(new TestRequest { Name = "World" });

            result.Should().Be("Hello World");

            // Check the order of execution
            // Expected order:
            // Before B (outer-most)
            // Before A
            // Handler
            // After A
            // After B (outer-most)
            log.Should().ContainInOrder("Before A", "Before B", "After B", "After A");
        }
    }

}
