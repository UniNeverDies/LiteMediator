using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LiteMediator.Tests.Unit
{
    public class LiteMediatorExtensionsTests
    {
        [Fact]
        public void AddLiteMediator_Should_Register_All_Handlers_And_Behaviors()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddLiteMediator(Assembly.GetExecutingAssembly());

            // Assert
            var provider = services.BuildServiceProvider();
            var mediator = provider.GetService<IMediator>();

            mediator.Should().NotBeNull();
        }

        [Fact]
        public void AddLiteMediatorCore_Should_Register_Core_And_Mediator()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddLiteMediatorCore(Assembly.GetExecutingAssembly());

            // Assert
            var provider = services.BuildServiceProvider();
            var mediator = provider.GetService<IMediator>();

            mediator.Should().NotBeNull();
        }

        [Fact]
        public void AddLiteMediatorModule_Should_Register_Module_Handlers_Only()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddLiteMediatorModule(Assembly.GetExecutingAssembly());

            // Assert
            var provider = services.BuildServiceProvider();
            var mediator = provider.GetService<IMediator>();

            mediator.Should().BeNull("because AddLiteMediatorModule should not register the IMediator itself");
        }

        [Fact]
        public void AddLiteMediator_Should_Not_Register_Mediator_Twice()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddScoped<IMediator, DummyMediator>(); // Pre-register

            // Act
            services.AddLiteMediator(Assembly.GetExecutingAssembly());

            // Assert
            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>();

            mediator.Should().BeOfType<DummyMediator>();
        }

        public class DummyMediator : IMediator
        {
            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
                Task.FromResult(default(TResponse)!);

            public Task Publish(INotification notification, CancellationToken cancellationToken = default) =>
                Task.CompletedTask;
        }
    }
}
