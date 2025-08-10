# LiteMediator

A super-lightweight, high-performance alternative to MediatR with 97% test coverage, CI/CD via GitHub Actions, and FluentValidation integration built in.

[![Build Status](https://github.com/faojul/LiteMediator/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/faojul/LiteMediator/actions/workflows/nuget-publish.yml)
[![NuGet Version](https://img.shields.io/nuget/v/LiteMediator.Lite.svg?style=flat-square)](https://www.nuget.org/packages/LiteMediator.Lite/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Code Coverage](https://img.shields.io/badge/coverage-97%25-brightgreen)](#)

A lightweight .NET mediator library designed for clean CQRS architecture and fast performance.

---

## Why LiteMediator?
- Minimal overhead—no external dependencies
- Full Clean Architecture & CQRS support
- Integrates FluentValidation out of the box
- Easy setup—only one line to replace MediatR
- Reliable CI/CD & 97% unit test coverage

  
## ✨ Features

| Feature                                  | Supported | Description                                               |
| ---------------------------------------- | --------- | --------------------------------------------------------- |
| `IMediator.Send()`                       | ✅         | Core command/request dispatching                          |
| `IRequest<TResponse>`                    | ✅         | Request/command abstraction                               |
| `IRequestHandler<TRequest, TResponse>`   | ✅         | Custom request handler logic                              |
| `IPipelineBehavior<TRequest, TResponse>` | ✅         | Middleware-style behavior support ([customizing](#customizing-validation-behavior))                        |
| `ValidationBehavior` (FluentValidation)  | ✅         | Built-in validation using FluentValidation  ([guide](#customizing-validation-behavior))              |
| `INotification` + `Publish()`            | ✅         | Event publishing to multiple handlers                     |
| `INotificationHandler<T>`                | ✅         | Handle published domain events                            |
| `RegisterHandlersWithMediator()`         | ✅         | Auto-registration of all request handlers                 |
| `RegisterBehaviorsWithMediator()`        | ✅         | Auto-registration of pipeline behaviors                   |
| `AddLiteMediator()` extensions           | ✅         | Simplified registration for monoliths & modular monoliths |


---

## 🚀 Super Easy MediatR Replacement
✅ All your existing code can stay unchanged:
- `Send(request)` ✔️
- `Publish(notification)` ✔️
- `IRequest<T>` ✔️
- `IRequestHandler<T>` ✔️
- `INotification` ✔️
- `INotificationHandler<T>`  ✔️

The only change is in `Program.cs / Startup.cs`:
````csharp
// Replace this:
services.AddMediatR(...);

// With this:
services.AddLiteMediator(Assembly.GetExecutingAssembly());
````


## 📦 Installation

```bash
dotnet add package LiteMediator.Lite
````

## 🧩 How to Use


**1. Startup Setup**  
   Add `services.AddLiteMediator(...)` in your Program.cs or Startup.cs.


```csharp
services.AddLiteMediator(Assembly.GetExecutingAssembly());
````
   Or, Multiple Assemblies
```csharp
//You can pass multiple assemblies in one go
services.AddLiteMediator(
    typeof(SharedModule.SomeType).Assembly,
    typeof(ModuleA.SomeHandler).Assembly,
    typeof(ModuleB.SomeHandler).Assembly
);

````
    
This registers:
   - All IRequestHandler(s)
   - All IPipelineBehavior(s) (including validation)
   - IMediator service


✅ Minimal setup for monolithic or layered apps.



**2. Modular Support**  
   Use `AddLiteMediatorCore()` and `AddLiteMediatorModule()` for clean modular monoliths.

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



**3. Validation**  
   Just register your FluentValidation validators—they’ll run automatically.

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


## 🔍 Example

   **1. Sending a Request**
      
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

   **2. Publishing a Notification**
````csharp
public record UserCreated(string Name) : INotification;

public class SendWelcomeEmail : INotificationHandler<UserCreated>
{
    public Task Handle(UserCreated notification, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Welcome email sent to {notification.Name}");
        return Task.CompletedTask;
    }
}
````

````csharp
await mediator.Publish(new UserCreated("Alice"));
````

## 🛠 Customizing Validation Behavior
LiteMediator includes `ValidationBehavior<TRequest, TResponse>` using FluentValidation.
You can extend it to add logic, or implement `IPipelineBehavior` to replace it entirely.

   **1. Extending `ValidationBehavior`**

````csharp
using FluentValidation;
using LiteMediator;

public class MyCustomValidationBehavior<TRequest, TResponse>
    : ValidationBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public MyCustomValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        : base(validators) { }

    public override async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Custom before validation");
        var result = await base.Handle(request, next, cancellationToken);
        Console.WriteLine("Custom after validation");
        return result;
    }
}

````

   **2. Implementing `IPipelineBehavior` from Scratch**
````csharp
using LiteMediator;

public class MyNoOpValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Skipping validation");
        return next();
    }
}

````
  Registration:
````csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MyNoOpValidationBehavior<,>));
````

## 🧪 Testing
- ✅ 97% Unit Test Coverage
- ✅ Integration tests for Send, Publish, validation pipeline
- ✅ GitHub Actions CI/CD


## 🔧 CI/CD
LiteMediator uses GitHub Actions to automatically:
- Build & Test on every push
- Publish NuGet package on version tag (e.g., v1.0.1)


## 📁 Folder Structure
````
src/
  LiteMediator/                # Main library
tests/
  LiteMediator.Tests/          # xUnit integration and unit tests
.github/
  workflows/nuget-publish.yml         # CI/CD pipeline
````

## 📃 License
Licensed under the MIT License.

## Author
**Faojul Ahsan** – Senior .NET Backend Engineer passionate about clean architecture, API-first development, and remote collaboration. Seeking global remote opportunities.
