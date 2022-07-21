// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using MetricBenchmarks;

var _ = BenchmarkRunner.Run<MeasurementPerf>();

