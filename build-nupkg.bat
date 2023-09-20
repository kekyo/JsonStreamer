@echo off

rem AspNetCore.JsonStreamer - JSON Lines streaming serializer on ASP.NET Core.
rem Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
rem
rem Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0

echo.
echo "==========================================================="
echo "Build AspNetCore.JsonStreamer"
echo.

dotnet build -p:Configuration=Release AspNetCore.JsonStreamer.sln
dotnet pack -p:Configuration=Release -o artifacts AspNetCore.JsonStreamer.sln
