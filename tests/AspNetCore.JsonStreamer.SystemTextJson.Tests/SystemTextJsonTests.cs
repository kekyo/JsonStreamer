//////////////////////////////////////////////////////////////////////////////
//
// AspNetCore.JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AspNetCore.JsonStreamer;

public sealed class SystemTextJsonTests
{
    [Test]
    public async Task StreamerTest()
    {
        var totalCount = 1000000;

        await using var testWebApplication = await TestWebApplication.CreateAsync(
            12445,
            mvcBuilder => mvcBuilder.AddJsonStreamer(),
            totalCount,
            default);

        var count = 0;
        await foreach (var item in
            StreamingFetcher.StreamingFetchAsync<TestModel>(testWebApplication.Url, default))
        {
            Assert.AreEqual(count, item.Index);

            if ((item.Index % 1000) == 0)
            {
                Trace.WriteLine($"SystemTextJsonTests: {item.Index}");
            }

            count++;
        }

        Trace.WriteLine($"SystemTextJsonTests: {count}");

        Assert.AreEqual(totalCount, count);
    }
}
