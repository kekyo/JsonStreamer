# AspNetCore.JsonStreamer

JSON Lines streaming serializer on ASP.NET Core.

![AspNetCore.JsonStreamer](Images/AspNetCore.JsonStreamer.100.png)

# Status

[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip)

|Target serializer|Pakcage|
|:----|:----|
|System.Text.Json|TODO:|
|Newtonsoft.Json|[![NuGet AspNetCore.JsonStreamer.NewtonsoftJson](https://img.shields.io/nuget/v/AspNetCore.JsonStreamer.NewtonsoftJson.svg?style=flat)](https://www.nuget.org/packages/AspNetCore.JsonStreamer.NewtonsoftJson)|

----

## What is this?

Has anyone else noticed that ASP.NET Core can send streaming data using asynchronous iterators `IAsyncEnumerable<T>` ?
It is code like this:

```csharp
[HttpGet]
public IAsyncEnumerable<WeatherForecast> Get() =>
    AsyncEnumerable.Range(1, 1000000).
    Select(index => new WeatherForecast
    {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    });
```

In fact, this works as expected on the server side of ASP.NET Core and does not consume memory for buffering.
Were you aware that this code returns a JSON array?

```json
[
    {
        "date": "2023-09-20T20:23:49.5736146+09:00",
        "temperatureC": 14,
        "temperatureF": 57,
        "summary": "Mild"
    },
    {
        "date": "2023-09-21T20:23:49.5768618+09:00",
        "temperatureC": 34,
        "temperatureF": 93,
        "summary": "Freezing"
    },
    {
        "date": "2023-09-22T20:23:49.5768924+09:00",
        "temperatureC": 1,
        "temperatureF": 33,
        "summary": "Mild"
    },

    // (continues a lot of JObject)
]
```

What about on the browser side receiving this?

There is no standard asynchronous iterator implementation in the JavaScript world (maybe there is, I just don't know).
Moreover, there is definitely no inherent language syntax for handling asynchronous iterators.
In other words, there is no syntactic sugar such as `await foreach` that C# can handle.

Streaming send is easy, but streaming receive is not.

So we want to return the data in [JSON Lines (or NDJSON) format](https://jsonlines.org/):

```json
{"date":"2023-09-20T20:23:49.5736146+09:00","temperatureC":14,"temperatureF":57,"summary":"Mild"}
{"date":"2023-09-21T20:23:49.5768618+09:00","temperatureC":34,"temperatureF":93,"summary":"Freezing"}
{"date":"2023-09-22T20:23:49.5768924+09:00","temperatureC":1,"temperatureF":33,"summary":"Mild"}

// (continues a lot of JObject)
```

That is, instead of an array of JSON, JObjects are sent separated by LF (Newline delimitation).
This means that deserializer iteration is easier.

If this is the case, there exists the deserializer implementation on the JavaScript side ([can-ndjson-stream](https://github.com/canjs/can-ndjson-stream)).

The library overrides the serializer so that when returning asynchronous iterators, they are automatically sent in JSON Lines.
(Other types use the default serializer)

### Target .NET platforms

* .NET 7.0 to 5.0
* .NET Core 3.1
* ASP.NET Core 7 to 3.

## How to use

It is very easy to use, just install this package and set it up in the builder as follows.

* You already use with Newtonsoft.Json serializer, use of [AspNetCore.JsonStreamer.NewtonsoftJson](https://www.nuget.org/packages/AspNetCore.JsonStreamer.NewtonsoftJson)
  and call `AddNewtonsoftJsonStreamer()` instead of `AddNewtonsoftJson()`.
* TODO:

```csharp
public static void Main(string[] args)
{
    // ...

    // Add services to the container.
    builder.Services.AddControllers().
        AddNewtonsoftJsonStreamer();    // Enable streamer.

    // ...

    var app = builder.Build();

    app.MapControllers();
    app.Run();
}
```

----

## License

Apache-v2

## History

