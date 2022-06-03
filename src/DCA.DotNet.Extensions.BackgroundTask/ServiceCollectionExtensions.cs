using System.Collections.ObjectModel;
using DCA.DotNet.Extensions.BackgroundTask;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class BackgroundTaskServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundTask(
        this IServiceCollection services,
        int workerCount = 1, int capacity = 10)
    {
        return services.AddBackgroundTask(options =>
        {
            options.Worker.WorkerCount = workerCount;
            options.DefaultQueueOptions.Capacity = capacity;
        });
    }

    public static IServiceCollection AddBackgroundTask(
        this IServiceCollection services,
        Action<BackgroudTaskOptions>? configureOptions)
    {
        var ob = services.AddOptions<BackgroudTaskOptions>();
        if (configureOptions != null)
        {
            ob.Configure(configureOptions);
        }
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<BackgroudTaskOptions>>().Value;
            var queues = options.Queues
                .ToDictionary(q => q.Name, q => new BackgroundTaskQueue(q) as IBackgroundTaskQueue);
            return new ReadOnlyDictionary<string, IBackgroundTaskQueue>(queues);
        });
        services.AddSingleton<IBackgroundTaskDispatcher, BackgroundTaskDispatcher>();
        services.AddHostedService<BackgroundTaskWorkerPool>();
        return services;
    }
}
