//////////////////////////////////////////////////////////////////////////////
//
// JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
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

namespace JsonStreamer.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class TestResultController : ControllerBase
{
    private readonly TestWebApplication parent;

    public TestResultController(TestWebApplication parent) =>
        this.parent = parent;

    [HttpGet]
    public async IAsyncEnumerable<TestModel> Get()
    {
        await foreach (var item in AsyncEnumerable.
            Range(0, this.parent.Count).
            Select(index =>
            {
                return new TestModel
                {
                    Index = index,
                    Date = DateTime.Now.AddDays(index),
                    Guid = Guid.NewGuid(),
                };
            }))
        {
            yield return item;

            if ((item.Index % 1000) == 0)
            {
                Trace.WriteLine($"TestResultController: {item.Index}");
            }
        }

        Trace.WriteLine("TestResultController: Done.");
    }
}
