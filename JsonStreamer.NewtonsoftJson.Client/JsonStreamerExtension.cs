//////////////////////////////////////////////////////////////////////////////
//
// JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

using JsonStreamer.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace JsonStreamer;

public static class JsonStreamerExtension
{
    private static readonly JsonSerializer defaultSerializer =
        Utilities.CreateDefaultSerializer();

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static JsonSerializer CreateDefaultSerializer() =>
        Utilities.CreateDefaultSerializer();

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static async IAsyncEnumerable<T> ReadStreamingAsync<T>(
        this Stream stream,
        Encoding encoding,
        JsonSerializer serializer,
        DuplicatePropertyNameHandling duplicatePropertyNameHandling,
        [EnumeratorCancellation] CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var ls = new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Ignore,
            LineInfoHandling = LineInfoHandling.Ignore,
            DuplicatePropertyNameHandling = duplicatePropertyNameHandling,
        };
        var tr = new StreamReader(stream, encoding, true);

        var task = tr.ReadLineAsync().WaitAsync(ct);
        try
        {
            while (true)
            {
                var line = await task;
                task = null;
                if (line == null)
                {
                    break;
                }

                task = tr.ReadLineAsync().WaitAsync(ct);

                var jt = JToken.Parse(line, ls);
                yield return jt.ToObject<T>(serializer)!;
            }
        }
        finally
        {
            if (task != null)
            {
                task.Dispose();
            }
        }
    }

    public static IAsyncEnumerable<T> ReadStreamingAsync<T>(
        this Stream stream,
        CancellationToken ct = default) =>
        ReadStreamingAsync<T>(
            stream,
            Encoding.UTF8,
            defaultSerializer,
            DuplicatePropertyNameHandling.Ignore,
            ct);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static async IAsyncEnumerable<T> ReadStreamingAsync<T>(
        this HttpContent content,
        Encoding encoding,
        JsonSerializer serializer,
        DuplicatePropertyNameHandling duplicatePropertyNameHandling,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
#if NET5_0_OR_GREATER
        await using var stream = await content.ReadAsStreamAsync(ct);
#elif NETSTANDARD2_1 || NETCOREAPP3_1
        await using var stream = await content.ReadAsStreamAsync().WaitAsync(ct);
#else
        using var stream = await content.ReadAsStreamAsync().WaitAsync(ct);
#endif

        var ls = new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Ignore,
            LineInfoHandling = LineInfoHandling.Ignore,
            DuplicatePropertyNameHandling = duplicatePropertyNameHandling,
        };
        var tr = new StreamReader(stream, encoding, true);

        var task = tr.ReadLineAsync().WaitAsync(ct);
        try
        {
            while (true)
            {
                var line = await task;
                task = null;
                if (line == null)
                {
                    break;
                }

                task = tr.ReadLineAsync().WaitAsync(ct);

                var jt = JToken.Parse(line, ls);
                yield return jt.ToObject<T>(serializer)!;
            }
        }
        finally
        {
            if (task != null)
            {
                task.Dispose();
            }
        }
    }

    public static IAsyncEnumerable<T> ReadStreamingAsync<T>(
        this HttpContent content,
        CancellationToken ct = default) =>
        ReadStreamingAsync<T>(
            content,
            Encoding.UTF8,
            defaultSerializer,
            DuplicatePropertyNameHandling.Ignore,
            ct);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static async IAsyncEnumerable<T> SendStreamingAsync<T>(
        this HttpClient httpClient,
        HttpRequestMessage request,
        Encoding encoding,
        JsonSerializer serializer,
        DuplicatePropertyNameHandling duplicatePropertyNameHandling,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var response = await httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, ct);

        response.EnsureSuccessStatusCode();

#if NET5_0_OR_GREATER
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
#elif NETSTANDARD2_1 || NETCOREAPP3_1
        await using var stream = await response.Content.ReadAsStreamAsync().WaitAsync(ct);
#else
        using var stream = await response.Content.ReadAsStreamAsync().WaitAsync(ct);
#endif

        var ls = new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Ignore,
            LineInfoHandling = LineInfoHandling.Ignore,
            DuplicatePropertyNameHandling = duplicatePropertyNameHandling,
        };
        var tr = new StreamReader(stream, encoding, true);

        var task = tr.ReadLineAsync().WaitAsync(ct);
        try
        {
            while (true)
            {
                var line = await task;
                task = null;
                if (line == null)
                {
                    break;
                }

                task = tr.ReadLineAsync().WaitAsync(ct);

                var jt = JToken.Parse(line, ls);
                yield return jt.ToObject<T>(serializer)!;
            }
        }
        finally
        {
            if (task != null)
            {
                task.Dispose();
            }
        }
    }

    public static IAsyncEnumerable<T> SendStreamingAsync<T>(
        this HttpClient httpClient,
        HttpRequestMessage request,
        CancellationToken ct = default) =>
        SendStreamingAsync<T>(
            httpClient,
            request,
            Encoding.UTF8,
            defaultSerializer,
            DuplicatePropertyNameHandling.Ignore,
            ct);

    public static IAsyncEnumerable<T> GetStreamingAsync<T>(
        this HttpClient httpClient,
        string url,
        CancellationToken ct = default) =>
        SendStreamingAsync<T>(
            httpClient,
            new(HttpMethod.Get, url),
            Encoding.UTF8,
            defaultSerializer,
            DuplicatePropertyNameHandling.Ignore,
            ct);

    public static IAsyncEnumerable<T> GetStreamingAsync<T>(
        this HttpClient httpClient,
        Uri url,
        CancellationToken ct = default) =>
        SendStreamingAsync<T>(
            httpClient,
            new(HttpMethod.Get, url),
            Encoding.UTF8,
            defaultSerializer,
            DuplicatePropertyNameHandling.Ignore,
            ct);

    public static IAsyncEnumerable<T> PostStreamingAsync<T>(
        this HttpClient httpClient,
        string url,
        HttpContent content,
        CancellationToken ct = default) =>
        SendStreamingAsync<T>(
            httpClient,
            new(HttpMethod.Post, url) { Content = content, },
            Encoding.UTF8,
            defaultSerializer,
            DuplicatePropertyNameHandling.Ignore,
            ct);

    public static IAsyncEnumerable<T> PostStreamingAsync<T>(
        this HttpClient httpClient,
        Uri url,
        HttpContent content,
        CancellationToken ct = default) =>
        SendStreamingAsync<T>(
            httpClient,
            new(HttpMethod.Post, url) { Content = content, },
            Encoding.UTF8,
            defaultSerializer,
            DuplicatePropertyNameHandling.Ignore,
            ct);
}
