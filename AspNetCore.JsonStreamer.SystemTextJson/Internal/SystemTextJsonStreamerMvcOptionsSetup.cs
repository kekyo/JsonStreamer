//////////////////////////////////////////////////////////////////////////////
//
// AspNetCore.JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using System;
using System.Buffers;

namespace AspNetCore.JsonStreamer.Internal;

internal sealed class SystemTextJsonStreamerMvcOptionsSetup : IConfigureOptions<MvcOptions>
{
    private readonly JsonOptions jsonOptions;
    private readonly ArrayPool<char> charPool;

    public SystemTextJsonStreamerMvcOptionsSetup(
        IOptions<JsonOptions> jsonOptions,
        ArrayPool<char> charPool)
    {
        if (jsonOptions == null)
        {
            throw new ArgumentNullException("jsonOptions");
        }
        if (charPool == null)
        {
            throw new ArgumentNullException("charPool");
        }
        this.jsonOptions = jsonOptions.Value;
        this.charPool = charPool;
    }

    public void Configure(MvcOptions options)
    {
        options.OutputFormatters.RemoveType<SystemTextJsonOutputFormatter>();
        options.OutputFormatters.Add(
            new SystemTextJsonStreamerOutputFormatter(
            this.jsonOptions.JsonSerializerOptions));
    }
}
