
Helpers for .NET

## DCA.DotNet.Extensions.OpenTelemetry.AspNetCore

An enrichment for [OpenTelemetry.Instrumentation.AspNetCore](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore/README.md), to avoid producing high cardinality span names which may break distributed tracing infrastructure.

*related issue: [opentelemetry-dotnet#2986](https://github.com/open-telemetry/opentelemetry-dotnet/issues/2986)*

*span naming practice: [opentelemetry-specification](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/api.md#span)*

## DCA.DotNet.Extensions.Logging

A modified json console formatter for logging, which:
1. display unicode characters correctly
2. omit redundant fields (`State.Message` and `Scope.Message`)

## DCA.DotNet.Extensions.BackgroundTask

Background task dispatching and processing, inspired by [Queued background tasks](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio#queued-background-tasks)

## DCA.DotNet.Extensions.Dapr.Actors

Support namespaced actors (formatted with `{namespace}:{actorType}`).

*related issues:*
* *[dapr#4711](https://github.com/dapr/dapr/issues/4711)*
* *[dapr#3167](https://github.com/dapr/dapr/issues/3167)*

## DCA.DotNet.Extensions.CloudEvents

Publish/Subscribe CloudEvents via Kafka, inspired by dapr pubsub component.

Configure pubsub:
```csharp
services.AddCloudEvents(defaultPubSubName: "kafka", defaultTopic: "my-topic")
    .Load(typeof(OrderCancelled).Assembly)
    .AddKafkaPubSub("kafka", options =>
    {
        options.ProducerConfig = new ProducerConfig
        {
            BootstrapServers = broker,
        };
    }, options =>
    {
        options.ConsumerConfig = new ConsumerConfig
        {
            BootstrapServers = broker,
            GroupId = consumerGroup,
        };
    });
```

Define cloud event:
```csharp
[CloudEvent]
public record OrderCancelled(Guid OrderId, string Reason);
```

Publish cloud event:
```csharp
await pubsub.PublishAsync(new OrderCancelled(order.Id, reason));
```

Subscribe and process cloud event:
``` csharp
public class OrderCancelledHandler : ICloudEventHandler<OrderCancelled>
{
    public async Task HandleAsync(CloudEvent<PingEvent> cloudEvent, CancellationToken token)
    {
        // ...
    }
}
```



