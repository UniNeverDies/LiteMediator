using FluentAssertions;
using LiteMediator.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace LiteMediator.Tests.Unit
{
    public class MediatorTests
    {
        [Fact]
        public async Task Send_Should_Invoke_Handler_And_Return_Result()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IRequestHandler<TestRequest, string>, TestHandler>();
            services.AddScoped<IMediator, Mediator>();

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            // Act
            var result = await mediator.Send(new TestRequest { Name = "Mediator" });

            // Assert
            result.Should().Be("Hello Mediator");

        }

        [Fact]
        public async Task Publish_Should_Invoke_All_Handlers()
        {
            var services = new ServiceCollection();
            services.AddScoped<IMediator, Mediator>();
            services.AddScoped<INotificationHandler<TestNotification>, FirstHandler>();
            services.AddScoped<INotificationHandler<TestNotification>, SecondHandler>();

            var called = new List<string>();
            services.AddSingleton(called);

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            var notification = new TestNotification();

            await mediator.Publish(notification);

            called.Should().Contain("First");
            called.Should().Contain("Second");
        }

    }
}