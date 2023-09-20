#!/bin/bash

# AspNetCore.JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
# Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
#
# Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0

echo ""
echo "==========================================================="
echo "Build AspNetCore.JsonStreamer"
echo ""

dotnet build -p:Configuration=Release AspNetCore.JsonStreamer.sln
dotnet pack -p:Configuration=Release -o artifacts AspNetCore.JsonStreamer.sln
