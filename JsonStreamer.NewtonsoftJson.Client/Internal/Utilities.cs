//////////////////////////////////////////////////////////////////////////////
//
// JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JsonStreamer.Internal;

internal static class Utilities
{
    public static JsonSerializer CreateDefaultSerializer()
    {
        var defaultSerializer = new JsonSerializer();
        defaultSerializer.ObjectCreationHandling = ObjectCreationHandling.Replace;
        defaultSerializer.Formatting = Formatting.None;
        defaultSerializer.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        defaultSerializer.DateParseHandling = DateParseHandling.DateTimeOffset;
        defaultSerializer.DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
        defaultSerializer.NullValueHandling = NullValueHandling.Ignore;
        defaultSerializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
        defaultSerializer.Converters.Add(
            new StringEnumConverter(new CamelCaseNamingStrategy()));

        return defaultSerializer;
    }


#if !NET6_0_OR_GREATER
    public static Task<T> WaitAsync<T>(
        this Task<T> task, CancellationToken ct)
    {
        if (task.IsCompleted)
        {
            return task;
        }

        var tcs = new TaskCompletionSource<T>();
        var ctr = ct.Register(() => tcs.TrySetCanceled());

        task.ContinueWith(task =>
        {
            ctr.Dispose();

            var c = task.IsCanceled;
            var f = task.IsFaulted;
            if (!c && !f)
            {
                tcs.TrySetResult(task.Result);
            }
            else if (c)
            {
                tcs.TrySetCanceled();
            }
            else if (f)
            {
                tcs.TrySetException(task.Exception!);
            }
        });

        return tcs.Task;
    }
#endif
}
