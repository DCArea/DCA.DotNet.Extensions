using System.Diagnostics.Metrics;
namespace DCA.DotNet.Extensions.BackgroundTask;

public static class Metrics
{
    public const string MeterName = "DCA.DotNet.Extensions.BackgroundTask";
    private static readonly Meter s_meter = new(MeterName, "1.0.0");
    public static Counter<int> CounterDispathedWorkItems { get; }
        = s_meter.CreateCounter<int>("dispatched-workitems-count");
    public static Counter<int> CounterProcessedWorkItems { get; }
        = s_meter.CreateCounter<int>("processed-workitems-count");
}
