### Logging

NCalc provides flexible and customizable logging capabilities using [
`Microsoft.Extensions.Logging`](https://www.nuget.org/packages/Microsoft.Extensions.Logging).
This allows developers to integrate logging into their applications seamlessly, ensuring that key operations and events
within the `NCalc` library are tracked and available for troubleshooting or monitoring.

#### Key Features

- **Configurable Logging**: Logging behavior in `NCalc` can be controlled via configuration switches, allowing
  developers to enable or disable specific logging outputs.
- **Multiple Logging Providers**: By default, `NCalc` supports logging to the console and to trace sources, which can be
  extended to other logging providers if needed.

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