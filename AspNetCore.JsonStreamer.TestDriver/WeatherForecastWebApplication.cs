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
using System;
using System.Threading.Tasks;

namespace AspNetCore.JsonStreamer;

public sealed class WeatherForecastWebApplication : IAsyncDisposable
{
    private const string allowSpecificOriginsName = "allowSpecificOrigins";

    private readonly WebApplication app;

    public WeatherForecastWebApplication(int port,
        Action<IMvcBuilder> mvcBuilder,
        int count)
    {
        this.Count = count;

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

        builder.Services.AddSingleton(this);

        var mb = builder.Services.AddControllers();
        mb.AddApplicationPart(typeof(WeatherForecastWebApplication).Assembly);
        mvcBuilder(mb);

        this.app = builder.Build();

        this.app.UseCors(allowSpecificOriginsName);
        this.app.MapControllers();

        this.Url = new($"http://localhost:{port}/WeatherForecast");
    }

    public int Count { get; }
    public Uri Url { get; }

    public async ValueTask StartAsync()
    {
        await this.app.StartAsync();
        await Task.Delay(500);
    }

    public ValueTask DisposeAsync() =>
        this.app.DisposeAsync();
}
