using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;

namespace DCA.DotNet.Extensions.OpenTelemetry.AspNetCore;

public static class AspNetCoreInstrumentationEnrichments
{
    // route name resolve priority:
    // 1. get route name from RouteNameMetadata - .WithName({name})
    // 2. get route name from DisplayName property of the endpoint - .WithDisplayName("{name}")
    //    default as {protocal}: {method} {pattern}
    // 3. {protocal}: {method} {pattern}
    // 4. {protocal}: {method} for non-endpoint requests
    public static Action<Activity, string, object> EnrichRouteName { get; } = (activity, eventName, data) =>
    {
        // reference:  https://github.com/open-telemetry/opentelemetry-dotnet/issues/2986
        HttpContext context;
        if (data is HttpRequest request)
        {
            context = request.HttpContext;
        }
        else if (data is HttpResponse response)
        {
            context = response.HttpContext;
        }
        else
        {
            return;
        }

        if (context.Features.Get<IEndpointFeature>()?.Endpoint is RouteEndpoint endpoint)
        {
            string? routeName = endpoint.Metadata.GetMetadata<RouteNameMetadata>()?.RouteName;
            if (!string.IsNullOrEmpty(routeName))
            {
                activity.DisplayName = routeName;
            }
            else if (!string.IsNullOrEmpty(endpoint.DisplayName))
            {
                activity.DisplayName = endpoint.DisplayName;
            }
            else
            {
                activity.DisplayName = $"{context.Request.Protocol.ToUpperInvariant()}: {context.Request.Method.ToUpperInvariant()} {endpoint.RoutePattern?.RawText?.ToLowerInvariant()}";
            }
        }
        else
        {
            activity.DisplayName = $"{context.Request.Protocol.ToUpperInvariant()}: {context.Request.Method.ToUpperInvariant()}";
        }
    };
}
