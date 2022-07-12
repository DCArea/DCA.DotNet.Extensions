using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace DCA.DotNet.Extensions.CloudEvents;

internal delegate Task HandleCloudEventDelegate(IServiceProvider serviceProvider, CloudEvent @event, CancellationToken token);

public class Registry
{
    private readonly Dictionary<Type, CloudEventMetadata> _metadata = new();
    private readonly Dictionary<CloudEventMetadata, HandleCloudEventDelegate> _handlers = new();
    private readonly string _defaultPubSubName;
    private readonly string _defaultTopic;
    private readonly string _defaultSource;

    public Registry(string defaultPubSubName, string defaultTopic, string defaultSource)
    {
        _defaultPubSubName = defaultPubSubName;
        _defaultTopic = defaultTopic;
        _defaultSource = defaultSource;
    }

    internal void RegisterMetadata(Type eventDataType, CloudEventAttribute attribute)
    {
        var metadata = new CloudEventMetadata(
            PubSubName: attribute.PubSubName ?? _defaultPubSubName,
            Topic: attribute.Topic ?? _defaultTopic,
            Type: attribute.Type ?? eventDataType.Name,
            Source: attribute.Source ?? _defaultSource
        );
        _metadata.TryAdd(eventDataType, metadata);
    }

    internal CloudEventMetadata GetMetadata(Type eventDataType)
    {
        return _metadata[eventDataType];
    }

    internal bool TryGetHandler(CloudEventMetadata metadata, out HandleCloudEventDelegate? handler)
    {
        return _handlers.TryGetValue(metadata, out handler);
    }

    internal HandleCloudEventDelegate GetHandler(CloudEventMetadata metadata)
    {
        return _handlers[metadata];
    }

    internal void RegisterHandler<TData>(CloudEventMetadata metadata)
    {
        _handlers.TryAdd(metadata, Handle);

        static Task Handle(IServiceProvider serviceProvider, CloudEvent @event, CancellationToken token)
        {
            var typedEvent = new CloudEvent<TData>(
                Id: @event.Id,
                Source: @event.Source,
                Type: @event.Type,
                Time: @event.Time,
                Data: @event.Data.Deserialize<TData>()!,
                DataSchema: @event.DataSchema,
                Subject: @event.Subject
            );

            return serviceProvider.GetRequiredService<ICloudEventHandler<TData>>().HandleAsync(typedEvent, token);
        }
    }

    public IEnumerable<string> GetTopics(string pubSubName)
    {
        return _metadata.Values
            .Where(m => m.PubSubName == pubSubName)
            .Select(m => m.Topic);
    }

    public void Debug()
    {
        foreach (var (key, value) in _metadata)
        {
            Console.WriteLine($"{key}: {value}");
        }

        foreach (var (key, value) in _handlers)
        {
            Console.WriteLine($"{key}: {value}");
        }
    }

}