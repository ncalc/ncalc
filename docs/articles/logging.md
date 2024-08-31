### Logging

NCalc provides flexible and customizable logging capabilities using [
`Microsoft.Extensions.Logging`](https://www.nuget.org/packages/Microsoft.Extensions.Logging).
You can use the built-in configuration or dependency injection to change the log behavior.

#### Default Configuration
Logging is disabled by default for performance reasons. The default logging configuration can be enabled using the following switches:

- **Enable Console Logging**:
  ```csharp
  AppContext.SetSwitch("NCalc.Logging.EnableConsole", true);
  ```

- **Enable Trace Logging**:
  ```csharp
  AppContext.SetSwitch("NCalc.Logging.EnableTrace", true);
  ```

These switches provide flexibility, allowing you to control where and how logging data is output.

#### Dependency Injection

NCalc leverages [`NCalc.DependencyInjection`](dependency_injection.md) to integrate with the .NET dependency injection (DI) system. This
means you can easily inject any `ILoggerProvider` into NCalc. For more detailed information about logging with `Microsoft.Extensions.Logging` using DI, please refer to
the [official Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging).