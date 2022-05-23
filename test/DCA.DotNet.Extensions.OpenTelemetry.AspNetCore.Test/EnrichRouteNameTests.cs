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

public class EnrichRouteNameTests
{
    [Fact]
    public async Task EnrichNamedEndpoint()
    {
        var activityProcessor = new Mock<BaseProcessor<Activity>>();
        void ConfigureTestServices(IServiceCollection services)
        {
            Sdk.CreateTracerProviderBuilder()
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

        Assert.Equal("GetMyItem", activity!.DisplayName);
    }


    [Fact]
    public async Task EnrichUnnamedEndpoint()
    {
        var activityProcessor = new Mock<BaseProcessor<Activity>>();
        void ConfigureTestServices(IServiceCollection services)
        {
            _tracerProvider = Sdk.CreateTracerProviderBuilder()
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

        var response = await client.GetAsync("/my-items2/abc");
        response.EnsureSuccessStatusCode();
        WaitForProcessorInvocations(activityProcessor, 3);

        var activity = activityProcessor.Invocations.FirstOrDefault(invo => invo.Method.Name == "OnEnd")?.Arguments[0] as Activity;
        Assert.NotNull(activity);

        Assert.Equal("HTTP: GET /my-items2/{name}", activity!.DisplayName);
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
}
