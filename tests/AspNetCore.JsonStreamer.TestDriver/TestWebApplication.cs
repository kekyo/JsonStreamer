//////////////////////////////////////////////////////////////////////////////
//
// AspNetCore.JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.JsonStreamer;

public sealed class TestWebApplication : IAsyncDisposable
{
    private const string allowSpecificOriginsName = "allowSpecificOrigins";

    private WebApplication app = null!;

    private TestWebApplication(
        int count,
        Uri url)
    {
        this.Count = count;
        this.Url = url;
    }

    private void SetWebApplication(WebApplication app) =>
        this.app = app;

    public int Count { get; }
    public Uri Url { get; }

    public ValueTask DisposeAsync() =>
        this.app.DisposeAsync();

    public static async Task<TestWebApplication> CreateAsync(
        int basePort,
        Action<IMvcBuilder> mvcBuilder,
        int count,
        CancellationToken ct)
    {
        var port = basePort +
            int.Parse(new string(
                ThisAssembly.AssemblyMetadata.TargetFrameworkMoniker.
                Where(char.IsNumber).
                ToArray()), CultureInfo.InvariantCulture);

        while (true)
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.
                AddCors(options =>
                {
                    options.AddPolicy(allowSpecificOriginsName,
                        policy =>
                        {
                            policy.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
                        });
                });

            builder.WebHost.
                UseKestrel(options =>
                {
                    options.ConfigureEndpointDefaults(endPointOptions =>
                    {
                        endPointOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                }).
                UseUrls($"http://localhost:{port}/");

            var url = new Uri($"http://localhost:{port}/TestResult");
            var testWebAppication = new TestWebApplication(count, url);
            builder.Services.AddSingleton(testWebAppication);

            var mb = builder.Services.AddControllers();
            mb.AddApplicationPart(typeof(TestWebApplication).Assembly);
            mvcBuilder(mb);

            var app = builder.Build();

            try
            {
                app.UseCors(allowSpecificOriginsName);
                app.MapControllers();

                await app.StartAsync(ct);
                await Task.Delay(500, ct);

                testWebAppication.SetWebApplication(app);

                return testWebAppication;
            }
            catch
            {
            }

            port++;
        }
    }
}
