# zhw_MaxPlayers
Author: zhw1nq

Dit me MatchZy


## Getting Started
If you need help getting started with making a CounterStrikeSharp plugin, head over to the docs:\
https://docs.cssharp.dev/

## Building during development
Build your plugin using `dotnet` CLI:
```shell
dotnet build
```
or
```shell
dotnet watch build --project src/zhw_MaxPlayers.csproj
```
and find your plugin build at `./build/zhw_MaxPlayers`

Optionally, [setup automatic live hot-reloading on your remote server](/docs/auto-live-hot-reloading.md)

## Building for release
Build your plugin using `dotnet` CLI:
```shell
dotnet publish
```
and find your plugin published at `./publish/zhw_MaxPlayers`

## Template info
By default, **create-cssharp-plugin** initializes your project with a reference to 
the latest `1.*` version of `CounterStrikeSharp.API` as managed by NuGet.

`CounterStrikeSharp.API` and its dependancies **will not** be bundled with your plugin
build to reduce bundle size and avoid redundancy. This is **recommended**; however, 
if you wish to undo this default, you can change the following lines in `zhw_MaxPlayers.csproj`
```xml
<PackageReference Include="CounterStrikeSharp.API" version="1.*">
    <PrivateAssets>all</PrivateAssets>
</PackageReference>
```
to:
```xml
<PackageReference Include="CounterStrikeSharp.API" version="1.*" />
```

Generated using **create-cssharp-plugin** version 0.4.0
