// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Running;
using MetricBenchmarks;

var summary = BenchmarkRunner.Run<MeasurementPerf>();

