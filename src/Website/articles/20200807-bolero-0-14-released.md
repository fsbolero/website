---
title: Bolero 0.14 released
subtitle: Switching to System.Text.Json, using Blazor fragments, URL encoding in the router and more.
---

We are happy to announce the release of [Bolero](https://fsbolero.io) version 0.14. Bolero is a library that enables writing full-stack applications in F#, whose client side runs in WebAssembly using [Blazor](https://blazor.net/).

This release requires the .NET Core SDK version 3.1.300 or newer, which you can download [here](https://dotnet.microsoft.com/download/dotnet-core/3.1) for Windows, OSX or Linux.

Install the latest project template with:

```
dotnet new -i Bolero.Templates::0.14.13
```

## Changes


* [#135](https://github.com/fsbolero/bolero/issues/135) Inferred router performs URL encoding/decoding on `string`-typed parameters.

    ```fsharp
    type Page =
        | [<EndPoint "/article/{slug}">]
          Article of slug: string

    let router = Router.infer SetPage (fun model -> model.page)
    
    router.Link (Article "Déjà-vu") = "/article/D%C3%A9j%C3%A0-vu"
    ```

* [#151](https://github.com/fsbolero/bolero/issues/151) Accept either a relative or absolute path in custom router's `getRoute`.

* [#155](https://github.com/fsbolero/bolero/issues/155) Add function `fragment` to create a Bolero `Node` from a Blazor `RenderFragment`.

    This enables creating components that take fragments as parameters:

    ```fsharp
    type Input() =
        inherit Component()
        
        [<Parameter>]
        member val Label = Unchecked.defaultof<RenderFragment> with get, set
        
        override this.Render() =
            label [] [
                fragment this.Label
                input []
            ]
            
    let myInput = comp<Input> [attr.fragment "Label" (div [] [text "My input"])] []
    ```
    
    In particular, a `RenderFragment` parameter named `ChildContent` defines the children of the component, ie. the content passed as second argument to `comp`.
    
    ```fsharp
    type Panel() =
        inherit Component()
        
        [<Parameter>]
        member val ChildContent = Unchecked.defaultof<RenderFragment> with get, set
        
        [<Parameter>]
        member val Title = "" with get, set
        
        override this.Render() =
            div [attr.classes ["panel"]] [
                h1 [attr.classes ["panel-title"]] [
                    text this.Title
                ]
                div [attr.classes ["panel-body"]] [
                    fragment this.ChildContent
                ]
            ]
            
    let myPanel =
        comp<Panel> ["Title" => "Welcome to Bolero"] [
            text "Let's learn about "
            a [attr.href "https://fsbolero.io"] [text "Bolero"]
            text "!"
        ]
    ```

* [#159](https://github.com/fsbolero/bolero/issues/159) **Breaking change**: Remove the module `Bolero.Json`, and use System.Text.Json together with [FSharp.SystemTextJson](https://github.com/Tarmil/FSharp.SystemTextJson) instead for remoting.

    Remoting serialization can be customized by passing an additional argument `configureSerialization: JsonSerializerOptions -> unit` to `services.AddRemoting()` in both the server-side and client-side startup functions.
    
    Client-side:
    ```fsharp
    open System.Text.Json.Serialization

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)

        builder.Services.AddRemoting(builder.HostEnvironment, fun serializerOptions ->
            JsonFSharpConverter(JsonUnionEncoding.ThothLike)
            |> serializerOptions.Converters.Add
        ) |> ignore
        
        // ...
    ```
    
    Server-side:
    ```fsharp
    open System.Text.Json.Serialization

    type Startup() =
    
        member this.ConfigureServices(services: IServiceCollection) =
            services.AddRemoting<MyRemoteService>(fun serializerOptions ->
                JsonFSharpConverter(JsonUnionEncoding.ThothLike)
                |> serializerOptions.Converters.Add
            ) |> ignore
            
            // ...
    ```

Happy coding!
