using Dapr.Actors.Runtime;

namespace DCA.DotNet.Extensions.Dapr.Actors.Test;
public class TestActor : Actor, ITestActor
{
    public TestActor(ActorHost host) : base(host)
    {
    }

    public Task Ping()
    {
        return Task.CompletedTask;
    }
}
