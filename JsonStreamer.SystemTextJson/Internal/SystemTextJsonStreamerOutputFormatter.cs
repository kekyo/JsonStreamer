//////////////////////////////////////////////////////////////////////////////
//
// JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JsonStreamer.Internal;

internal sealed class SystemTextJsonStreamerOutputFormatter :
    TextOutputFormatter
{
    private static readonly MethodInfo writeAsyncEnumerableAsyncMethodT =
        typeof(SystemTextJsonStreamerOutputFormatter).GetMethod(
            "WriteAsyncEnumerableAsync",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
    private static readonly byte[] newline = new[] { (byte)'\n' };
    private static readonly ConcurrentDictionary<Type, Func<OutputFormatterWriteContext, Encoding, Task>?> writeAsyncEnumerableAsyncFunctions = new();

    private readonly SystemTextJsonOutputFormatter inner;

    internal SystemTextJsonStreamerOutputFormatter(
        JsonSerializerOptions serializerOptions)
    {
        this.inner = new(serializerOptions);
        foreach (var encoding in this.inner.SupportedEncodings)
        {
            this.SupportedEncodings.Add(encoding);
        }
        foreach (var mediaTypes in this.inner.SupportedMediaTypes)
        {
            this.SupportedMediaTypes.Add(mediaTypes);
        }
    }

    private async Task WriteAsyncEnumerableAsync<TElement>(
        OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        Debug.Assert(context.Object != null);

        var enumerable = (IAsyncEnumerable<TElement>)context.Object!;
        var ct = context.HttpContext.RequestAborted;

        await using var enumerator =
            enumerable.WithCancellation(ct).GetAsyncEnumerator();

        var moveNextTask = enumerator.MoveNextAsync();

        var responseStream = selectedEncoding.CodePage == Encoding.UTF8.CodePage ?
            context.HttpContext.Response.Body :
            Encoding.CreateTranscodingStream(
                context.HttpContext.Response.Body,
                selectedEncoding,
                Encoding.UTF8,
                true);
        var elementType = typeof(TElement);

        while (true)
        {
            if (!await moveNextTask)
            {
                break;
            }

            var item = enumerator.Current;
            moveNextTask = enumerator.MoveNextAsync();

            await JsonSerializer.SerializeAsync(
                responseStream, item, elementType, this.inner.SerializerOptions, ct);
            await responseStream.WriteAsync(newline, ct);
            await responseStream.FlushAsync(ct);
        }
    }

    private static Type? GetAsyncEnumerableElementType(Type type) =>
        type.GetInterfaces().
        Select(it =>
            (it.IsGenericType &&
             it.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>) &&
             it.GetGenericArguments() is [{ } elementType]) ?
                 elementType : null).
        FirstOrDefault();

    public override Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context, Encoding selectedEncoding) =>
        (context.Object?.GetType() is { } type &&
         writeAsyncEnumerableAsyncFunctions.GetOrAdd(type, _ =>
            GetAsyncEnumerableElementType(type) is { } elementType ?
                (Func<OutputFormatterWriteContext, Encoding, Task>)Delegate.CreateDelegate(
                    typeof(Func<OutputFormatterWriteContext, Encoding, Task>),
                    this,
                    writeAsyncEnumerableAsyncMethodT.MakeGenericMethod(elementType)) : null) is { } func) ?
            func(context, selectedEncoding) :
            this.inner.WriteResponseBodyAsync(context, selectedEncoding);
}
