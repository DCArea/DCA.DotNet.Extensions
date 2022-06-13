namespace DCA.DotNet.Extensions.Dapr.Actors.Test;

public abstract class FakeHttpMessageHandler : HttpMessageHandler
{
    public FakeHttpMessageHandler()
    {
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return InternalSendAsync(request);
    }

    public abstract Task<HttpResponseMessage> InternalSendAsync(HttpRequestMessage request);
}
