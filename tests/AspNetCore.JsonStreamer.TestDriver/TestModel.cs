//////////////////////////////////////////////////////////////////////////////
//
// AspNetCore.JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
// Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
//
// Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0
//
//////////////////////////////////////////////////////////////////////////////

using System;

namespace AspNetCore.JsonStreamer;

public sealed class TestModel
{
    public int Index { get; set; }
    public DateTime Date { get; set; }
    public Guid Guid { get; set; }
}
