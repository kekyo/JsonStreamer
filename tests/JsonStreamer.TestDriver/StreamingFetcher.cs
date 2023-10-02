//////////////////////////////////////////////////////////////////////////////
//
// JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace JsonStreamer;

public static class StreamingFetcher
{
    private static HttpClient httpClient = new();

    public static async IAsyncEnumerable<T> StreamingFetchAsync<T>(
        Uri url, [EnumeratorCancellation] CancellationToken ct)
    {
        using var response = await httpClient.GetAsync(
           url,
           // Important continuous deserializing (will make no-buffering)
           HttpCompletionOption.ResponseHeadersRead,
           ct);

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
}
