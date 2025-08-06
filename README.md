# LiteMediator

**LiteMediator** is a lightweight and dependency-free mediator library for .NET, inspired by MediatR but with minimal dependencies and high performance. It supports command/query dispatching, pipeline behaviors, and fluent validation—all with 97% code coverage and CI/CD support via GitHub Actions.

[![Build Status](https://github.com/faojul/LiteMediator/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/faojul/LiteMediator/actions/workflows/nuget-publish.yml)
[![NuGet Version](https://img.shields.io/nuget/v/LiteMediator.svg)](https://www.nuget.org/packages/LiteMediator)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Code Coverage](https://img.shields.io/badge/coverage-97%25-brightgreen)](#)

---

## ✨ Features

| Feature                           | Supported | Description                                  |
|-----------------------------------|-----------|----------------------------------------------|
| `IMediator.Send()`                | ✅ Yes    | Core command/request dispatching              |
| `IRequest<TResponse>`             | ✅ Yes    | Request/command pattern                      |
| `IRequestHandler<TReq, TRes>`     | ✅ Yes    | Custom request handler logic                 |
| `IPipelineBehavior<TReq, TRes>`   | ✅ Yes    | Middleware-style behaviors                   |
| `ValidationBehavior`             | ✅ Yes    | Built-in [FluentValidation](https://fluentvalidation.net/) support |
| `RegisterHandlersWithMediator()`  | ✅ Yes    | Auto DI registration of handlers             |
| `RegisterBehaviorsWithMediator()` | ✅ Yes    | Auto DI registration of behaviors            |
| `IMediator.Publish()`             | ✅ Yes    | Publish notifications to multiple handlers   |
| `INotificationHandler<T>`         | ✅ Yes    | Event handling                               |

---

## 📦 Installation

```bash
dotnet add package LiteMediator
````

🧩 How to Use
🔹 1. Monolith / Layered Architecture
In your Startup.cs or Program.cs:


```csharp
services.AddLiteMediator(Assembly.GetExecutingAssembly());
````

This registers:
- All IRequestHandler<>s
- All IPipelineBehavior<>s (including validation)
- IMediator service


✅ Minimal setup for monolithic or layered apps.

🔹 2. Modular Monolith / Microservices
For separation between Shared and Modules, you can use:

```csharp
// Shared module
services.AddLiteMediatorCore(typeof(Shared.AssemblyMarker).Assembly);

// Module1
services.AddLiteMediatorModule(typeof(Module1.AssemblyMarker).Assembly);

// Module2
services.AddLiteMediatorModule(typeof(Module2.AssemblyMarker).Assembly);
````


| Method                    | Usage                                             |
| ------------------------- | ------------------------------------------------- |
| `AddLiteMediator()`       | For monoliths                                     |
| `AddLiteMediatorCore()`   | Register shared `Mediator + Behaviors + Handlers` |
| `AddLiteMediatorModule()` | Register only module-specific handlers            |



✅ Built-in FluentValidation Support
If you're using FluentValidation:

1. Add your validators as normal:

````csharp

public class MyCommandValidator : AbstractValidator<MyCommand>
{
    public MyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}
````

2. Register them in DI (if not auto-registered):

````csharp
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
````
✅ Your validators will automatically be executed before calling the handler, thanks to the ValidationBehavior.


🔍 Example

````csharp

public class HelloRequest : IRequest<string>
{
    public string Name { get; set; }
}

public class HelloHandler : IRequestHandler<HelloRequest, string>
{
    public Task<string> Handle(HelloRequest request, CancellationToken cancellationToken)
        => Task.FromResult($"Hello, {request.Name}");
}
````

````csharp
var result = await mediator.Send(new HelloRequest { Name = "World" });
// => "Hello, World"
````

🧪 Testing
- ✅ 97% Unit Test Coverage
- ✅ Integration tests for Send, Publish, validation pipeline
- ✅ GitHub Actions CI/CD

🔧 CI/CD
LiteMediator uses GitHub Actions to automatically:
- Build & Test on every push
- Publish NuGet package on version tag (e.g., v1.0.1)

📁 Folder Structure
````
src/
  LiteMediator/                # Main library
tests/
  LiteMediator.Tests/          # xUnit integration and unit tests
.github/
  workflows/dotnet.yml         # CI/CD pipeline
````

📃 License
Licensed under the MIT License.

📎 Links
🔗 GitHub: LiteMediator
🔗 NuGet: LiteMediator Package
