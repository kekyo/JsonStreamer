//////////////////////////////////////////////////////////////////////////////
//
// AspNetCore.JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using AspNetCore.JsonStreamer.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for adding Newtonsoft.Json to <see cref="MvcCoreBuilder"/>.
/// </summary>
public static class NewtonsoftJsonStreamerMvcCoreBuilderExtensions
{
    /// <summary>
    /// Configures Newtonsoft.Json specific features with streaming such as input and output formatters.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcCoreBuilder"/>.</param>
    /// <returns>The <see cref="IMvcCoreBuilder"/>.</returns>
    public static IMvcCoreBuilder AddNewtonsoftJsonStreamer(this IMvcCoreBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.AddNewtonsoftJson();
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, NewtonsoftJsonStreamerMvcOptionsSetup>());
        return builder;
    }

    /// <summary>
    /// Configures Newtonsoft.Json specific features with streaming such as input and output formatters.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcCoreBuilder"/>.</param>
    /// <param name="setupAction">Callback to configure <see cref="MvcNewtonsoftJsonOptions"/>.</param>
    /// <returns>The <see cref="IMvcCoreBuilder"/>.</returns>
    public static IMvcCoreBuilder AddNewtonsoftJsonStreamer(
        this IMvcCoreBuilder builder,
        Action<MvcNewtonsoftJsonOptions> setupAction)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        builder.AddNewtonsoftJson(setupAction);
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, NewtonsoftJsonStreamerMvcOptionsSetup>());
        return builder;
    }
}
