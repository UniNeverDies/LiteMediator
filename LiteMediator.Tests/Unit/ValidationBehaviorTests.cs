using FluentAssertions;
using FluentValidation;

namespace LiteMediator.Tests.Unit
{
    public class ValidationBehaviorTests
    {
        public class TestRequest : IRequest<string>
        {
            public string Name { get; set; } = "";
        }

        public class TestValidator : AbstractValidator<TestRequest>
        {
            public TestValidator()
            {
                RuleFor(x => x.Name).NotEmpty();
            }
        }

        [Fact]
        public async Task Handle_Should_Throw_ValidationException_When_Invalid()
        {
            // Arrange
            var validator = new TestValidator();
            var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });

            var request = new TestRequest { Name = "" };

            // Act
            Func<Task> act = async () =>
            {
                await behavior.Handle(request, () => Task.FromResult("OK"), default);
            };

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task Handle_Should_Invoke_Next_When_Valid()
        {
            var validator = new TestValidator();
            var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });

            var request = new TestRequest { Name = "Valid" };

            var result = await behavior.Handle(request, () => Task.FromResult("OK"), default);

            result.Should().Be("OK");
        }
    }

}
