---
title: Hosting models
subtitle: WebAssembly or server-side
---

> Note: Blazor 8 and Bolero 0.24 introduced new hosting models documented here.
> [This page](Hosting-pre-blazor-8) describes the pre-Blazor 8 hosting models, which are still supported but considered legacy.

Bolero applications, like all Blazor applications, can run in a number of different modes.

* In plain WebAssembly, all F# code runs in the browser using [WebAssembly](https://webassembly.org/) and the static content is a simple HTML file.

* In hosted modes, there is an ASP.NET Core server side that serves the static content, and dynamic behavior can run either in WebAssembly, or on the server side using a [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) connection to update the browser.

## Plain WebAssembly mode

To create a plain WebAssembly application, use the [dotnet project template](index#creating-a-project) with the following arguments:

```sh
dotnet new bolero-app --render=WebAssembly
```

In plain WebAssembly mode, the static content of the application is a simple `.html` file, located at `src/<Project>.Client/wwwroot/index.html`.
The dynamic Bolero content is rendered in a tag of this file.

This file must contain the following:

```html
<script src="_framework/blazor.webassembly.js"></script>
```

as well as a container element for the dynamic content, for example `<div id="main"></div>`.
This container is selected in `Startup.fs`:

```fsharp
builder.RootComponents.Add<Main.MyApp>("#main")
```

Note that prerendering is not possible in plain WebAssembly mode.

## Hosted modes

### InteractiveWebAssembly

To create a hosted WebAssembly application, use the [dotnet project template](index#creating-a-project) with the following arguments:

```sh
dotnet new bolero-app --render=InteractiveWebAssembly
```

In InteractiveWebAssembly mode, the static content of the application is served by an ASP.NET Core server.
It is defined in Bolero HTML syntax in the file `<Project>.Server/Index.fs`.

### InteractiveServer

### InteractiveAuto

## Stream rendering

### Stream rendering with static server rendering

### Stream rendering with interactive modes
