using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenTelemetry;
using OpenTelemetry.Trace;
using Xunit;

namespace DCA.DotNet.Extensions.OpenTelemetry.AspNetCore.Test;

[Collection("OpenTelemetry")]
public class EnrichTraceIdHeaderTests: IDisposable
{
    private TracerProvider? _traceProvider;

    [Fact]
    public async Task EnrichTraceIdHeader()
    {
        var activityProcessor = new Mock<BaseProcessor<Activity>>();
        void ConfigureTestServices(IServiceCollection services)
        {
            _traceProvider = Sdk.CreateTracerProviderBuilder()
                .AddEncrichedAspNetCoreInstrumentation()
                // .AddAspNetCoreInstrumentation()
                .AddProcessor(activityProcessor.Object)
                .Build();
        }

        using var client = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(ConfigureTestServices);
            })
            .CreateClient();

        var response = await client.GetAsync("/my-items/abc");
        response.EnsureSuccessStatusCode();
        WaitForProcessorInvocations(activityProcessor, 3);

        var activity = activityProcessor.Invocations.FirstOrDefault(invo => invo.Method.Name == "OnEnd")?.Arguments[0] as Activity;
        Assert.NotNull(activity);

        Assert.Equal(activity!.TraceId.ToString(), response.Headers.GetValues("trace-id").Single());
    }


    private static void WaitForProcessorInvocations(Mock<BaseProcessor<Activity>> activityProcessor, int invocationCount)
    {
        // We need to let End callback execute as it is executed AFTER response was returned.
        // In unit tests environment there may be a lot of parallel unit tests executed, so
        // giving some breezing room for the End callback to complete
        Assert.True(SpinWait.SpinUntil(
            () =>
            {
                Thread.Sleep(10);
                return activityProcessor.Invocations.Count >= invocationCount;
            },
            TimeSpan.FromSeconds(5)));
    }

    public void Dispose()
    {
        _traceProvider?.Dispose();
    }
}
