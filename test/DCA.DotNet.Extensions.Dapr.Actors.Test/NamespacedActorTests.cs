using System.Net;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using FluentAssertions;
using RichardSzalay.MockHttp;
using Xunit;

namespace DCA.DotNet.Extensions.Dapr.Actors.Test;

public class NamespacedActorTests
{
    [Fact]
    public void ShouldRegisterNamespacedActor()
    {
        var options = new ActorRuntimeOptions();
        options.Actors.RegisterNamespacedActor<TestActor>("my-namespace");
        options.Actors.Single().Type.ActorTypeName.Should().Be("my-namespace:TestActor");
    }

    [Fact]
    public async void ShouldCreateProxyForNamespacedActor()
    {
        var http = new MockHttpMessageHandler();
        http.When(HttpMethod.Put, "http://127.0.0.1:3500/v1.0/actors/my-namespace:TestActor/abc/method/Ping")
            .Respond(HttpStatusCode.NoContent);
        var proxy = new ActorProxyFactory(handler: http);
        var actor = proxy.CreateNamespacedActorProxy<ITestActor>(new ActorId("abc"), "my-namespace", "TestActor");
        await actor.Ping();
    }
}
