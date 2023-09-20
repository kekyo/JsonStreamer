//////////////////////////////////////////////////////////////////////////////
//
// AspNetCore.JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.JsonStreamer.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> logger;
    private readonly WeatherForecastWebApplication parent;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        WeatherForecastWebApplication parent)
    {
        this.logger = logger;
        this.parent = parent;
    }

    [HttpGet]
    public async IAsyncEnumerable<WeatherForecast> Get()
    {
        await foreach (var item in AsyncEnumerable.
            Range(1, this.parent.Count).
            Select(index =>
            {
                return new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)],
                    Index = index,
                };
            }))
        {
            yield return item;

            if ((item.Index % 1000) == 0)
            {
                Trace.WriteLine($"WeatherForecastController: {item.Index}");
            }
        }

        Trace.WriteLine("WeatherForecastController: Done.");
    }
}
