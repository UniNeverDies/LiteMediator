using FluentAssertions;
using FluentValidation;
using LiteMediator.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteMediator.Tests.Integration
{
    public class IntegrationTests
    {
        [Fact]
        public async Task Send_Should_Call_Handler_And_ValidationBehavior()
        {
            // Arrange
            List<string> called = [];
            var services = new ServiceCollection();
            services.AddSingleton(called);

            var validator = new TestValidator();
            var behavior = new ValidationBehavior<TestRequest, string>([validator]);

            services.AddScoped<IValidator<TestRequest>>(_ => new TestValidator());
            services.AddScoped<IPipelineBehavior<TestRequest, string>>(
                sp =>
                {
                    var validator = sp.GetRequiredService<IValidator<TestRequest>>();
                    return new ValidationBehavior<TestRequest, string>([validator]);
                });

            services.AddLiteMediator(Assembly.GetExecutingAssembly());

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            // Act
            var response = await mediator.Send(new TestRequest { Name = "John" });

            // Assert
            response.Should().Be("Hello John");
        }

        [Fact]
        public async Task AddLiteMediator_Should_Resolve_Handler_And_Validator()
        {
            // Arrange
            List<string> called = [];
            var services = new ServiceCollection();
            services.AddSingleton(called);

            var validator = new TestValidator();
            var behavior = new ValidationBehavior<TestRequest, string>([validator]);

            services.AddScoped<IValidator<TestRequest>>(_ => new TestValidator());
            services.AddScoped<IPipelineBehavior<TestRequest, string>>(
                sp =>
                {
                    var validator = sp.GetRequiredService<IValidator<TestRequest>>();
                    return new ValidationBehavior<TestRequest, string>([validator]);
                });

            services.AddLiteMediator(Assembly.GetExecutingAssembly());

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            // Act
            var response = await mediator.Send(new TestRequest { Name = "John" });

            // Assert
            response.Should().Be("Hello John");
        }

        [Fact]
        public async Task AddLiteMediatorCore_Should_Work_Together()
        {
            // Arrange
            List<string> called = [];
            var services = new ServiceCollection();
            services.AddSingleton(called);

            var validator = new TestValidator();
            var behavior = new ValidationBehavior<TestRequest, string>([validator]);

            services.AddScoped<IValidator<TestRequest>>(_ => new TestValidator());
            services.AddScoped<IPipelineBehavior<TestRequest, string>>(
                sp =>
                {
                    var validator = sp.GetRequiredService<IValidator<TestRequest>>();
                    return new ValidationBehavior<TestRequest, string>([validator]);
                });

            services.AddLiteMediatorCore(Assembly.GetExecutingAssembly()); // Registers Mediator & Behaviors & Handlers

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            // Act
            var response = await mediator.Send(new TestRequest { Name = "John" });

            // Assert
            response.Should().Be("Hello John");
        }

        [Fact]
        public async Task AddLiteMediatorModule_Should_Work_Together()
        {
            // Arrange
            var called = new List<string>();
            var services = new ServiceCollection();
            services.AddSingleton(called);

            var validator = new TestValidator();
            var behavior = new ValidationBehavior<TestRequest, string>([validator]);

            services.AddScoped<IValidator<TestRequest>>(_ => new TestValidator());
            services.AddScoped<IPipelineBehavior<TestRequest, string>>(
                sp =>
                {
                    var validator = sp.GetRequiredService<IValidator<TestRequest>>();
                    return new ValidationBehavior<TestRequest, string>([validator]);
                });

            services.AddScoped<IMediator, Mediator>();
            services.AddLiteMediatorModule(Assembly.GetExecutingAssembly()); // Registers Handlers only

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            // Act
            var response = await mediator.Send(new TestRequest { Name = "John" });

            // Assert
            response.Should().Be("Hello John");
        }

        [Fact]
        public async Task Send_Should_Throw_When_Validation_Fails()
        {
            // Arrange
            var called = new List<string>();
            var services = new ServiceCollection();
            services.AddSingleton(called);

            services.AddLiteMediator(Assembly.GetExecutingAssembly(), typeof(ValidationBehavior<,>).Assembly);

            var validator = new TestValidator();
            var behavior = new ValidationBehavior<TestRequest, string>([validator]);

            services.AddScoped<IValidator<TestRequest>>(_ => new TestValidator());
            services.AddScoped<IPipelineBehavior<TestRequest, string>>(
                sp =>
                {
                    var validator = sp.GetRequiredService<IValidator<TestRequest>>();
                    return new ValidationBehavior<TestRequest, string>([validator]);
                });

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            // Act
            var ex = await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await mediator.Send(new TestRequest { Name = "" });
            });

            // Assert
            ex.Errors.Should().Contain("Name must not be empty.");
        }

        [Fact]
        public async Task Publish_Should_Invoke_All_NotificationHandlers()
        {
            // Arrange
            var called = new List<string>();
            var services = new ServiceCollection();
            services.AddSingleton(called);
            services.AddLiteMediator(Assembly.GetExecutingAssembly());

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            // Act
            await mediator.Publish(new TestNotification());

            // Assert
            called.Should().ContainInOrder("First", "Second");
        }


    }

}
