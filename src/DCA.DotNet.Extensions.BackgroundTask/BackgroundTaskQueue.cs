using System.Threading.Channels;

namespace DCA.DotNet.Extensions.BackgroundTask;

public delegate ValueTask BackgroundWorkItem(IServiceProvider services, CancellationToken cancellationToken);

public interface IBackgroundTaskQueue
{
    string Name { get; }
    int Priority { get; }
    public ChannelReader<BackgroundWorkItem> Reader { get; }

    ValueTask QueueBackgroundWorkItemAsync(BackgroundWorkItem workItem);

    ValueTask<BackgroundWorkItem> DequeueAsync(
        CancellationToken cancellationToken);
}

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<BackgroundWorkItem> _queue;

    public ChannelReader<BackgroundWorkItem> Reader => _queue.Reader;
    public string Name { get; }
    public int Priority { get; }

    public BackgroundTaskQueue(BackgroundTaskQueueOptions options)
    {
        Name = options.Name;
        Priority = options.Priority;
        if (options.Capacity <= 0)
        {
            var channelOptions = new UnboundedChannelOptions()
            {
            };
            _queue = Channel.CreateUnbounded<BackgroundWorkItem>(channelOptions);
        }
        else
        {
            var channelOptions = new BoundedChannelOptions(options.Capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<BackgroundWorkItem>(channelOptions);
        }
    }

    public ValueTask QueueBackgroundWorkItemAsync(
        BackgroundWorkItem workItem!!)
    {
        return _queue.Writer.WriteAsync(workItem);
    }

    public ValueTask<BackgroundWorkItem> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}
