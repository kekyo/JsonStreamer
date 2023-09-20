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
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.JsonStreamer;

public sealed class NewtonsoftJsonTests
{
    private static HttpClient httpClient = new();

    [Test]
    public async Task StreamerTest()
    {
        await using var testApp = new WeatherForecastWebApplication(
            12345,
            mvcBuilder => mvcBuilder.AddNewtonsoftJsonStreamer(),
            1000000);

        await testApp.StartAsync();

        using var response = await httpClient.GetAsync(
            testApp.Url, HttpCompletionOption.ResponseHeadersRead);

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
            var item = jt.ToObject<WeatherForecast>()!;

            if ((item.Index % 1000) == 0)
            {
                Trace.WriteLine($"StreamerTest: {item.Index}");
            }
        }
    }
}
