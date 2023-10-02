//////////////////////////////////////////////////////////////////////////////
//
// JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JsonStreamer.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions methods for configuring MVC via an <see cref="IMvcBuilder"/>.
/// </summary>
public static class SystemTextJsonStreamerMvcBuilderExtensions
{
    /// <summary>
    /// Configures System.Text.Json specific features with streaming such as input and output formatters.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddJsonStreamer(this IMvcBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, SystemTextJsonStreamerMvcOptionsSetup>());
        return builder;
    }

    /// <summary>
    /// Configures System.Text.Json specific features with streaming such as input and output formatters.
    /// </summary>
    /// <param name="builder">The <see cref="IMvcBuilder"/>.</param>
    /// <param name="setupAction">Callback to configure <see cref="JsonOptions"/>.</param>
    /// <returns>The <see cref="IMvcBuilder"/>.</returns>
    public static IMvcBuilder AddJsonStreamer(
        this IMvcBuilder builder,
        Action<JsonOptions> setupAction)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (setupAction == null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, SystemTextJsonStreamerMvcOptionsSetup>());
        return builder;
    }
}
