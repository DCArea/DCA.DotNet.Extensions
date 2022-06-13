using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DCA.DotNet.Extensions.BackgroundTask;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static System.Console;

namespace BackgroundTaskPerfTest;

public class PerfTest
{
    public static async Task RunAsync()
    {
        int producer = 10;
        int repeat = 10000000;
        int worker = 10;
        int queue = 10;
        int capacity = -1;

        Counter.Init();

        var services = new ServiceCollection().AddLogging();
        services.AddBackgroundTask(options =>
        {
            var queues = Enumerable.Range(1, queue)
                .Select((_, i) => new BackgroundTaskQueueOptions
                {
                    Name = i.ToString(),
                    Capacity = capacity,
                    Priority = 0
                })
                .ToList();
            options.Queues.AddRange(queues);
            options.Worker.WorkerCount = worker;
            options.DefaultQueueOptions.Capacity = capacity;
        });

        var sp = services.BuildServiceProvider();
        var dispatcher = sp.GetRequiredService<IBackgroundTaskDispatcher>();

        var sw = Stopwatch.StartNew();
        var dispatchTasks = Enumerable.Range(1, producer)
            .Select(i => Task.Run(() => DispatchFor(i)))
            .ToList();
        await Task.WhenAll(dispatchTasks);
        sw.Stop();
        WriteLine($"Dispatched {Counter.Dispatched} in {sw.Elapsed.TotalSeconds}s");

        var pool = (BackgroundTaskWorkerPool)sp.GetServices<IHostedService>().Single(svc => svc is BackgroundTaskWorkerPool);

        sw.Restart();
        await pool.StartAsync(default);
        await pool.ExecuteTask!;
        sw.Stop();
        WriteLine($"Processed {Counter.Processed} in {sw.Elapsed.TotalSeconds}s");

        await pool.StopAsync(default);

        async Task DispatchFor(int producer)
        {
            for (int i = 0; i < repeat; i++)
                await dispatcher.DispatchAsync<ILogger<PerfTest>>((l, ct) => ValueTask.CompletedTask);
        }

    }
}
