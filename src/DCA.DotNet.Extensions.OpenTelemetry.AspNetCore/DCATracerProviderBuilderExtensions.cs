using DCA.DotNet.Extensions.OpenTelemetry.AspNetCore;
using OpenTelemetry.Instrumentation.AspNetCore;

namespace OpenTelemetry.Trace;

public static class DCATracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddEncrichedAspNetCoreInstrumentation(
        this TracerProviderBuilder builder,
        Action<AspNetCoreInstrumentationOptions>? configureAspNetCoreInstrumentationOptions = null)
    {
        var configuredOptions = configureAspNetCoreInstrumentationOptions;
        configureAspNetCoreInstrumentationOptions = options =>
        {
            var configuredEnrich = options.Enrich;
            options.Enrich = (activity, eventName, data) =>
            {
                AspNetCoreInstrumentationEnrichments.EnrichRouteName.Invoke(activity, eventName, data);
                AspNetCoreInstrumentationEnrichments.AttachTraceContextInHeader.Invoke(activity, eventName, data);
                configuredEnrich?.Invoke(activity, eventName, data);
            };

            configuredOptions?.Invoke(options);
        };

        return builder.AddAspNetCoreInstrumentation(configureAspNetCoreInstrumentationOptions);
    }
}
