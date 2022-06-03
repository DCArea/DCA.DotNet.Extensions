
Helpers for .NET

### DCA.DotNet.Extensions.OpenTelemetry.AspNetCore

An enrichment for [OpenTelemetry.Instrumentation.AspNetCore](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore/README.md), to avoid producing high cardinality span names which may break distributed tracing infrastructure.

*related issue: [opentelemetry-dotnet#2986](https://github.com/open-telemetry/opentelemetry-dotnet/issues/2986)*
*span naming practice: [opentelemetry-specification](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/api.md#span)

### DCA.DotNet.Extensions.Logging

A modified json console formatter for logging, which:
1. display unicode characters correctly
2. omit redundant fields (`State.Message` and `Scope.Message`)

### DCA.DotNet.Extensions.BackgroundTask

Background task dispatching and processing, inspired by [Queued background tasks](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio#queued-background-tasks)

