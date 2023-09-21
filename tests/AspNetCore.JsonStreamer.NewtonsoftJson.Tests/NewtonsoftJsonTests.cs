//////////////////////////////////////////////////////////////////////////////
//
// AspNetCore.JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.JsonStreamer;

public sealed class NewtonsoftJsonTests
{
    private static HttpClient httpClient = new();

    private async IAsyncEnumerable<T> StreamingFetchAsync<T>(Uri url)
    {
        using var response = await httpClient.GetAsync(
           url,
           HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        var tr = new StreamReader(stream);

        while (true)
        {
            var line = await tr.ReadLineAsync();
            if (line == null)
            {
                break;
            }

            var jt = JToken.Parse(line);
            var item = jt.ToObject<T>()!;

            yield return item;
        }
    }

    [Test]
    public async Task StreamerTest()
    {
        var totalCount = 1000000;

        await using var testWebApplication = await TestWebApplication.CreateAsync(
            12345,
            mvcBuilder => mvcBuilder.AddNewtonsoftJsonStreamer(),
            totalCount,
            default);

        var count = 0;
        await foreach (var item in
            this.StreamingFetchAsync<TestModel>(testWebApplication.Url))
        {
            Assert.AreEqual(count, item.Index);

            if ((item.Index % 1000) == 0)
            {
                Trace.WriteLine($"NewtonsoftJsonTests: {item.Index}");
            }

            count++;
        }

        Trace.WriteLine($"NewtonsoftJsonTests: {count}");

        Assert.AreEqual(totalCount, count);
    }
}
