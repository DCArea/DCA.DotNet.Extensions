using Dapr.Actors;

namespace DCA.DotNet.Extensions.Dapr.Actors.Test;

public interface ITestActor : IActor
{
    Task Ping();
}
