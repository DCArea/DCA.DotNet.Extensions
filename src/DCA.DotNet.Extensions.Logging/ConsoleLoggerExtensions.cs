// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Logging
{
    [UnsupportedOSPlatform("browser")]
    public static class ConsoleLoggerExtensions
    {
        /// <summary>
        /// Add a console log formatter named 'ejson' to the factory with default properties.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static ILoggingBuilder AddEnrichedJsonConsole(this ILoggingBuilder builder) =>
            builder.AddEnrichedJsonConsole(null);

        /// <summary>
        /// Add and configure a console log formatter named 'ejson' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configure">A delegate to configure the <see cref="ConsoleLogger"/> options for the enriched json log formatter.</param>
        public static ILoggingBuilder AddEnrichedJsonConsole(this ILoggingBuilder builder, Action<EnrichedJsonConsoleFormatterOptions> configure)
        {
            builder.AddConsole();
            if (configure is not null)
            {
                builder.Services.Configure(configure);
            }
            builder.AddConsoleFormatter<EnrichedJsonConsoleFormatter, EnrichedJsonConsoleFormatterOptions>();
            return builder;
        }
    }
}
