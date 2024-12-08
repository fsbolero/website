---
title: Bolero 0.24 released
subtitle: Interactive render modes
---

We are happy to announce the release of [Bolero](https://fsbolero.io) version 0.24.
Bolero is a library that enables writing full-stack appications in F#, whose interactivity runs in WebAssembly or on the server side using Blazor.

The main highlights of this release are:
* Support for Interactive render modes
* Support for Streaming rendering.

Install the latest project template with:

```sh
dotnet new -i Bolero.Templates::0.24.38
```

## Interactive render modes

Bolero now supports the [Interactive render modes](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes) which were introduced in Blazor 8.
This is a new way of deciding whether dynamic content should be rendered on the server side or in WebAssembly, incompatible with the existing `IBoleroHostConfig`-based method.

The Bolero project template now uses InteractiveWebAssembly mode by default. You can toggle it with the `--render` argument:

```sh
dotnet new bolero-app --render=InteractiveAuto
dotnet new bolero-app --render=InteractiveServer
dotnet new bolero-app --render=InteractiveWebAssembly
dotnet new bolero-app --render=LegacyServer      # "classic" server-side mode
dotnet new bolero-app --render=LegacyWebAssembly # "classic" WebAssembly mode
```

To switch an existing project to the new mode:

* Make sure you are using Bolero HTML for your static content.

* Wrap your `index` page definition in a component:

    ```fsharp
    [<Route "/{*path}">]
    type Page() =
        inherit Bolero.Component()
        override _.Render() = index
    ```

* In the server-side dependency injection: replace `AddBoleroHost()` with `AddBoleroComponents()`.

* In the server-side endpoints configuration, replace this:

    ```fsharp
    endpoints.MapDefaultToBolero(index) |> ignore
    ```

    with:

    ```fsharp
    endpoints.MapRazorComponents<Page>()
        // If you use Server or Auto render mode:
        .AddInteractiveServerRenderMode()
        // If you use WebAssembly or Auto render mode:
        .AddInteractiveWebAssemblyRenderMode()
        // List here your client assemblies:
        .AddAdditionalAssemblies(typeof<MyApp>.Assembly)
    |> ignore
    ```

* On your root dynamic component(s), use one of the following to decide its render mode:

    * `BoleroRenderMode` on the component itself:

        ```fsharp
        // Note: prerender is true if not specified.
        [<BoleroRenderMode(BoleroRenderMode.Auto, prerender = false)>]
        type MyApp() =
            // ...
        ```

        OR

    * `attr.renderMode` when creating an instance of the component:

        ```fsharp
        open Microsoft.AspNetCore.Components.Web

        let index = doctypeHtml {
            head { (* ... *) }
            body {
                // Default (ie with prerender=true):
                comp<MyApp> { attr.renderMode RenderMode.InteractiveAuto }

                // Custom (here with prerender=false):
                comp<MyApp> { attr.renderMode (InteractiveAutoRenderMode(prerender = false)) }

                boleroScript
            }
        }
        ```

    * If neither of these are used, then the render mode will be Static Server (ie no interactive content).

## Streaming rendering

Bolero now supports [streaming rendering](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/rendering?view=aspnetcore-9.0#streaming-rendering) in two different ways:

* Streaming rendering in the Elmish program, using `Program.mkStreamRendering` or `Program.mkSimpleStreamRendering`:

    ```fsharp
    type Model = { text: string }

    [<StreamRendering true>]
    type MyApp() =
        inherit ProgramComponent<Model, Message>()

        override this.Program =
            let initModel = { text = "Loading..." }

            let load initialModel = task {
                let! loadedText = myRemoteApi.getText ()
                return { initialModel with text = $"Loaded {loadedText}!" }, Cmd.none
            }

            Program.mkStreamRendering initModel load update view
    ```

* Streaming rendering on a (non-Elmish) component by inheriting from `StreamRenderingComponent<'model>`:

    ```fsharp
    type Model = { text: string }

    type MyStreamedComponent() =
        inherit StreamRenderingComponent<Model>()

        override _.InitialModel = { text = "Loading..." }

        override _.LoadModel(initialModel) = task {
            let! loadedText = myRemoteApi.getText ()
            return { initialModel with text = $"Loaded {loadedText}!" }, Cmd.none
        }
    ```

## .NET 9 and F# 9 improvements

This is not part of Bolero 0.24, but since [.NET 9 and F# 9 were recently released](https://devblogs.microsoft.com/dotnet/announcing-dotnet-9/), we would like to highlight some improvements that are relevant to Bolero:

* Tooling performance has been greatly improved around large and nested computation expressions. This should make the experience of editing HTML-heavy source files much more pleasant.

* It is now possible to create empty computation expressions. For example, to create an empty `div` element, instead of:

    ```fsharp
    div { attr.empty() }
    ```

    You can now simply write:

    ```fsharp
    div {}
    ```

* (On Blazor 9 only) Dependency injection can now be done using constructor arguments, rather than properties with the `Inject` attribute.

    ```fsharp
    // Blazor 8:
    type MyOldComponent() =
        inherit Component()

        [<Inject>]
        member val JS = Unchecked.defaultof<IJSRuntime> with get, set

        override this.Render() =
            this.JS.InvokeAsync("console.log", "Hello world!") |> ignore
            div { attr.empty() }

    // Blazor 9:
    type MyNewComponent(js: IJSRuntime) =
        inherit Component()

        override this.Render() =
            js.InvokeAsync("console.log", "Hello world!") |> ignore
            div {}
    ```

## .NET 6 and .NET 7 notice

Bolero 0.24 will be the last version of Bolero supporting .NET 6 and .NET 7 (which are already out of support by Microsoft). Starting with Bolero 0.25, .NET 8 will be the minimum supported version.

Happy coding!
