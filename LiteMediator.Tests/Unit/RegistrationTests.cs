using FluentAssertions;
using LiteMediator.Tests.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace LiteMediator.Tests.Unit
{
    public class RegistrationTests
    {
        [Fact]
        public void Should_Register_IRequestHandler()
        {
            var services = new ServiceCollection();
            services.RegisterHandlersWithMediator(typeof(TestHandler).Assembly);

            var sp = services.BuildServiceProvider();
            var handler = sp.GetRequiredService<IRequestHandler<TestRequest, string>>();

            handler.Should().NotBeNull();
        }

        [Fact]
        public void Should_Register_IPipelineBehavior()
        {
            List<string> called = new List<string>();
            var services = new ServiceCollection();
            services.AddSingleton(called);
            services.RegisterBehaviorsWithMediator(typeof(TestBehavior<TestRequest, string>).Assembly);

            var sp = services.BuildServiceProvider();
            var behavior = sp.GetRequiredService<IPipelineBehavior<TestRequest, string>>();

            behavior.Should().NotBeNull();
        }
    }

}
