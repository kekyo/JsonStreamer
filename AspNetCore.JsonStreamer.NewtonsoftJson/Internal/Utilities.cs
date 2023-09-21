//////////////////////////////////////////////////////////////////////////////
//
// AspNetCore.JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.JsonStreamer.Internal;

internal static class Utilities
{
#if !NET6_0_OR_GREATER
    private static Task InternalWaitAsync(
        Task task, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<bool>();
        var ctr = ct.Register(() => tcs.TrySetCanceled());

        task.ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully)
            {
                tcs.TrySetResult(true);
            }
            else if (t.IsCanceled)
            {
                tcs.TrySetCanceled();
            }
            else
            {
                tcs.TrySetException(t.Exception!);
            }
        });

        return tcs.Task;
    }

    public static Task WaitAsync(
        this Task task, CancellationToken ct) =>
        task.IsCompleted ? task : InternalWaitAsync(task, ct);
#endif
}
