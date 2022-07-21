using System.Diagnostics.Metrics;
using BenchmarkDotNet.Attributes;

namespace MetricBenchmarks;

[MemoryDiagnoser]
public class MeasurementPerf
{
    private static readonly Meter s_meter = new("MeasurementPerf");
    private static readonly MeterListener s_listener1 = new();
    private static readonly MeterListener s_listener2 = new();

    static MeasurementPerf()
    {
        s_listener1.InstrumentPublished += (instrument, listener) =>
        {
            if (instrument == s_oGauge1)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        s_listener1.Start();

        s_listener2.InstrumentPublished += (instrument, listener) =>
        {
            if (instrument == s_oGauge2)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        s_listener2.Start();
    }

    private static int s_m;

    private static Measurement<int> ObserveValueWithTags()
    {
        s_m++;
        return new Measurement<int>(1,
            new KeyValuePair<string, object?>("t1", "v1"),
            new KeyValuePair<string, object?>("t2", "v2"),
            new KeyValuePair<string, object?>("t3", "v3"),
            new KeyValuePair<string, object?>("t4", "v4"),
            new KeyValuePair<string, object?>("t5", "v5"),
            new KeyValuePair<string, object?>("t6", "v6"));
    }

    private static readonly ObservableGauge<int> s_oGauge1 = s_meter.CreateObservableGauge<int>("ObservableCounterWithTags", ObserveValueWithTags);
    [Benchmark]
    public int ObserveWithTags()
    {
        for (int i = 0; i < 500; i++)
            s_listener1.RecordObservableInstruments();
        return s_m;
    }


    private static readonly ObservableGauge<int> s_oGauge2 = s_meter.CreateObservableGauge<int>("ObservableCounter", () => s_m++);
    [Benchmark]
    public int ObserveWithOutTags()
    {
        for (int i = 0; i < 500; i++)
            s_listener2.RecordObservableInstruments();
        return s_m;
    }
}
