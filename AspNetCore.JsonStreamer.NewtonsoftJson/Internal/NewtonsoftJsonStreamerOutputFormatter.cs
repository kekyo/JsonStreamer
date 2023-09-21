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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.JsonStreamer.Internal;

internal sealed class NewtonsoftJsonStreamerOutputFormatter : NewtonsoftJsonOutputFormatter
{
    private static readonly MethodInfo writeAsyncEnumerableAsyncMethodT =
        typeof(NewtonsoftJsonStreamerOutputFormatter).GetMethod(
            "WriteAsyncEnumerableAsync",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;

    private readonly ConcurrentDictionary<Type, Func<OutputFormatterWriteContext, Encoding, Task>?> writeAsyncEnumerableAsyncFunctions = new();

    public NewtonsoftJsonStreamerOutputFormatter(
        JsonSerializerSettings serializerSettings,
        ArrayPool<char> charPool,
        MvcOptions mvcOptions,
        MvcNewtonsoftJsonOptions? jsonOptions) :
#if NET6_0_OR_GREATER
        base(serializerSettings, charPool, mvcOptions, jsonOptions)
#else
        base(serializerSettings, charPool, mvcOptions)
#endif
    {
    }

    private async Task WriteAsyncEnumerableAsync<TElement>(
        OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        await using var tw = context.WriterFactory(
            context.HttpContext.Response.Body, selectedEncoding);

        var jw = base.CreateJsonWriter(tw);
        var js = base.CreateJsonSerializer(context);

        var enumerable = (IAsyncEnumerable<TElement>)context.Object!;
        var ct = context.HttpContext.RequestAborted;

        await using var enumerator =
            enumerable.WithCancellation(ct).GetAsyncEnumerator();

        var moveNextTask = enumerator.MoveNextAsync();

        while (true)
        {
            if (!await moveNextTask)
            {
                break;
            }

            var item = enumerator.Current;
            moveNextTask = enumerator.MoveNextAsync();

            var jt = item != null ?
                JToken.FromObject(item, js) :
                JValue.CreateNull();

            await jt.WriteToAsync(jw, ct);
            await jw.FlushAsync(ct);

            await tw.WriteLineAsync().WaitAsync(ct);
        }

        await jw.FlushAsync(ct);
        await tw.FlushAsync().WaitAsync(ct);
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
         this.writeAsyncEnumerableAsyncFunctions.GetOrAdd(type, _ =>
            GetAsyncEnumerableElementType(type) is { } elementType ?
                (Func<OutputFormatterWriteContext, Encoding, Task>)Delegate.CreateDelegate(
                    typeof(Func<OutputFormatterWriteContext, Encoding, Task>),
                    this,
                    writeAsyncEnumerableAsyncMethodT.MakeGenericMethod(elementType)) : null) is { } func) ?
            func(context, selectedEncoding) :
            base.WriteResponseBodyAsync(context, selectedEncoding);
}
