using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;
using DCA.DotNet.Extensions.BackgroundTask;

namespace BackgroundTaskPerfTest;
public static class Counter
{
    private static int s_processed = 0;
    private static int s_dispatched = 0;

    public static int Dispatched => s_dispatched;
    public static int Processed => s_processed;

    public static void Init()
    {
        var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, listener) =>
        {
            System.Console.WriteLine($"{instrument.Name} {instrument}");
            if (instrument.Meter.Name == Metrics.MeterName)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        listener.SetMeasurementEventCallback<int>(OnMeasurementRecorded);
        listener.Start();
    }

    private static void OnMeasurementRecorded(Instrument instrument, int measurement, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
    {
        if (instrument.Name == Metrics.CounterDispathedWorkItems.Name)
        {
            Interlocked.Add(ref s_dispatched, measurement);
        }
        else if (instrument.Name == Metrics.CounterProcessedWorkItems.Name)
        {
            Interlocked.Add(ref s_processed, measurement);
        }
    }

}
