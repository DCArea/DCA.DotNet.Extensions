using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace DCA.DotNet.Extensions.BackgroundTask;

public class BackgroudTaskOptions : IOptions<BackgroudTaskOptions>
{
    BackgroudTaskOptions IOptions<BackgroudTaskOptions>.Value => this;
    public BackgroundTaskWorkerOptions Worker { get; } = new();
    public List<BackgroundTaskQueueOptions> Queues { get; set; } = new()
    {
        new (){ Name = Constants.DefaultQueueName}
    };
    public BackgroundTaskQueueOptions DefaultQueueOptions => Queues.Single(q => q.Name == Constants.DefaultQueueName);
}


public class BackgroundTaskQueueOptions
{
    [Required]
    public string Name { get; init; } = default!;
    public int Capacity { get; set; } = 10;
    public int Priority { get; set; } = 0;
}


public class BackgroundTaskWorkerOptions
{
    public int WorkerCount { get; set; } = Environment.ProcessorCount;
}
