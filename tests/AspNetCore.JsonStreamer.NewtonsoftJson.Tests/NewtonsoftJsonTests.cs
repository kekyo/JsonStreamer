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
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.JsonStreamer;

public sealed class NewtonsoftJsonTests
{
    private static HttpClient httpClient = new();

    private async IAsyncEnumerable<T> StreamingFetchAsync<T>(
        Uri url, [EnumeratorCancellation] CancellationToken ct)
    {
        using var response = await httpClient.GetAsync(
           url, HttpCompletionOption.ResponseHeadersRead, ct);

        response.EnsureSuccessStatusCode();

        var encoding = response.Content.Headers.ContentEncoding.
            Select(Encoding.GetEncoding).
            Where(e => e != null).
            FirstOrDefault() ?? Encoding.UTF8;

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var tr = new StreamReader(stream, encoding, true);

        var readLineTask = tr.ReadLineAsync().WaitAsync(ct);
        try
        {
            while (true)
            {
                var line = await readLineTask;
                readLineTask = null;
                if (line == null)
                {
                    break;
                }

                readLineTask = tr.ReadLineAsync().WaitAsync(ct);

                var jt = JToken.Parse(line);
                var item = jt.ToObject<T>()!;

                yield return item;
            }
        }
        finally
        {
            if (readLineTask != null)
            {
                readLineTask.Dispose();
            }
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
            this.StreamingFetchAsync<TestModel>(testWebApplication.Url, default))
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
