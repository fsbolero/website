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

To create a server-side application, use the [dotnet project template](index#creating-a-project) with the following arguments:

```sh
dotnet new bolero-app --render=InteractiveServer
```

In InteractiveServer mode, both the static and dynamic content of the application run in an ASP.NET Core server.

The static content is defined in Bolero HTML syntax in the file `<Project>.Server/Index.fs`.

The dynamic content runs on the server side using a [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) connection to update the browser.

### InteractiveAuto

To create an auto-hosted application, use the [dotnet project template](index#creating-a-project) with the following arguments:

```sh
dotnet new bolero-app --render=InteractiveAuto
```

In InteractiveAuto mode, the application determines automatically whether it should run in InteractiveServer or InteractiveWebAssembly mode.
Essentially, the first time a user connects to the site, it runs in InteractiveServer mode, but also downloads the WebAssembly assets in the background.
Then, on subsequent connections, it runs in InteractiveWebAssembly mode using the assets that are already loaded.

## Stream rendering

Stream rendering is a Blazor feature that allows serving an initial version of a page while launching an asynchronous task that will dynamically update the page with updated content.

It can be used when the page's proper content is slow to load, to display temporary content: in general, either a loader or a cached but potentially outdated version of the content.

Stream rendering is supported in all hosted modes.

### Stream rendering static content

To stream render static content, add a [component](blazor#components) in the server project inheriting from `StreamRenderingComponent<'T>`, where `'T` is the type of the component's data model.
This type has three members to override:

* `InitialModel: 'T` is the data model to display initially.
* `LoadModel : 'T -> Task<'T>` loads the data model to stream dynamically. It takes `InitialModel` as argument.
* `Render : 'T -> Node` indicates how to render the data model.

For example, loading the data from a database repository:

```fsharp
type Item = { name: string; price: decimal }

type PriceTable() =
    inherit StreamRenderingComponent<Item array>()
    
    [<Inject>]
    member val IPriceRepository PriceRepository = null with get, set

    override _.InitialModel = [| { name = "Loading prices..."; price = 0m } |]
    
    override this.LoadModel(_initialModel) = task {
        let! data = this.PriceRepository.GetPricesAsync()
        return data
    }
    
    override _.Render(model) =
        table {
            thead {
                tr { th { "Name" }; th { "Price" } }
            }
            tbody {
                for item in model do
                    tr { td { item.name }; td { $"{item.price}" } }
            }
        }
```

And then to include the content in the page:

```fsharp
let index = doctypeHtml {
    head { (* ... *) }
    body {
        h1 { "Price table" }
        comp<PriceTable>
        boleroScript
    }
}
```

### Stream rendering an Elmish program

For content that will continue to be dynamic after the stream rendering, you can use a stream rendered [Elmish program](elmish).

* Add the attribute `[<StreamRendering true>]` to your `ElmishComponent<'model, 'msg>`.

* Instead of creating the Elmish program with `Program.mkSimple` or `Program.mkProgram`, use `Program.mkSimpleStreamRendering` or `Program.mkStreamRendering`, respectively.

    * `mkSimpleStreamRendering`, in addition to an `initialModel: 'model`, also takes a function `load: 'model -> Task<'model>` which loads the model to stream dynamically.
    
    * `mkStreamRendering`, instead of a function `init` which returns an initial model and commands, takes:
    
        * an `initialModel: 'model` which is rendered initially;
        
        * a function `loadModel: 'model -> Task<'model> * Cmd<'msg>` which loads the model to stream dynamically as well as initial commands, if any.

The following example loads the model by calling a remote function:

```fsharp
type Remote = { getCounter: unit -> Async<int> }

type Model = { counter: int }

type Msg = Increment | Decrement

let initialModel = { counter = 0 }

let loadModel (remote: Remote) _ = task {
    let! counter = remote.getCounter()
    return { counter = counter }
}

let update msg model =
    match msg with
    | Decrement -> { counter = model.counter - 1 }
    | Increment -> { counter = model.counter + 1 }

let view model dispatch =
    concat {
        button { "-"; on.click (fun _ -> dispatch Decrement) }
        $" {model.counter} "
        button { "+"; on.click (fun _ -> dispatch Increment) }
    }

[<StreamRendering true>]
type Counter() =
    inherit ElmishComponent<Model, Msg>()
    
    override _.Program 
        Program.mkSimpleStreamRendering initialModel loadModel update view
```
