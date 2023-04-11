---
title: Bolero 0.22 released
subtitle: Endpoint routing and CSS isolation
---

We are happy to announce the release of [Bolero](https://fsbolero.io) version 0.22.
Bolero is a library that enables writing full-stack applications in F#, whose client side runs in WebAssembly using Blazor.

The main highlights of this release are the switch of remote services to endpoint routing, and the ability to provide CSS isolation for components.

Install the latest project template with:

```
dotnet new -i Bolero.Templates::0.22.7
```

## Endpoint routing for remote services

In ASP.NET Core, there are two ways that a handler can register itself with the application:

* As a middleware component, inserting itself in the request handling pipeline;

* As an endpoint, registering itself with the built-in routing table.

So far, Bolero remote services have used the former method.
However, endpoint routing provides better performance and more configurability.
Therefore, in Bolero 0.22, we implemented ([#289](https://github.com/fsbolero/Bolero/issues/289)) endpoint routing for remote services and obsoleted the remoting middleware component.

In your code, this manifests as the replacement of `app.UseRemoting()` with `endpoints.MapBoleroRemoting()` in the server-side application code.
For a more detailed migration guide, see [the Upgrade page](https://fsbolero.io/docs/Upgrade#from-v0.21-to-v0.22).

### Endpoint configuration

The method `endpoints.MapBoleroRemoting()` returns and `IEndpointConventionBuilder`, similar to other endpoint routing methods.
This allows further configuration of the endpoint, such as [enabling cross-origin requests](https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-7.0#enable-cors-with-endpoint-routing), or [filtering by domain name](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-7.0#host-matching-in-routes-with-requirehost):

```fsharp
endpoints.MapBoleroRemoting()
    .RequireCors(fun policy ->
        policy.WithMethods("POST")
        |> ignore)
    .RequireHost("fsbolero.io")
|> ignore
```

### Per remote service configuration

A Bolero application may contain multiple remote services.
It may be useful to provide different configuration (for example, different CORS policies) for different services.
For this purpose, `MapBoleroRemoting` can take the remote service type as type parameter.

```fsharp
endpoints.MapBoleroRemoting<MyApi>()
    .RequireCors(fun policy ->
        policy.WithOrigins("fsbolero.io")
        |> ignore)
|> ignore
```

Note: this is the remote service type, not the remote handler type.
In other words, if you use a class `MyApiHandler` that inherits from `RemoteHandler<MyApi>`, then you need to use `MapBoleroRemoting<MyApi>`, not `MapBoleroRemoting<MyApiHandler>`.

A call to `MapBoleroRemoting()` without type parameter will configure all remote services that don't have a corresponding typed `MapBoleroRemoting<T>()` call.

### Per method configuration

Endpoints can be configured in an even more fine-grained way.
The method `MapBoleroRemoting`, both typed and untyped, can take a function that is called separately for each remote method.

```fsharp
endpoints.MapBoleroRemoting(fun method endpoint ->
    if method.Service.Type = typeof<MyApi> && method.Name = "mySensitiveMethod" then
        endpoint.RequireHost("private.fsbolero.io") |> ignore)
|> ignore
```

It is however recommended to use separate services for methods that require significantly different configurations.

## Per remote service client-side configuration

Similarly, remote services can now be configured per service on the client side. [#280](https://github.com/fsbolero/Bolero/issues/280)

The method `services.AddBoleroRemoting` (which replaces the now-obsolete `services.AddRemoting`) has a variant that takes a remote service as parameter, and allows configuring the HTTP client and serialization for just this service.

```fsharp
// MyApi is served from fsbolero.io.
builder.Services.AddBoleroRemoting<MyApi>(configureHttpClient = fun http ->
    http.BaseAddress <- System.Uri "https://fsbolero.io")
|> ignore

// Other services are served from the current page's domain.
builder.Services.AddBoleroRemoting(builder.HostEnvironment) |> ignore
```

## CSS isolation for components

Blazor has a feature called [CSS isolation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/css-isolation), which allows creating a dedicated style sheet for a given component.
In Bolero 0.22, it is now possible to create CSS-isolated components too. [#296](https://github.com/fsbolero/Bolero/issues/296)

To add CSS isolation to a Bolero component:

* Create a file with extension `.bolero.css`. For example, `MyComponent.bolero.css`:

    ```css
    a.active {
        background: lightblue;
    }
    ```

* Compile the project once. This will generate a source file containing a module `CssScopes`.

* Add the generated scope to your component:

    ```fsharp
    type MyComponent() =
        inherit Component()

        override _.CssScope = CssScopes.MyComponent

        override _.Render() =
            a {
                attr.href "https://fsbolero.io"
                attr.``class`` "active"
                "Go to Bolero!"
            }
    ```

* Make sure that the generated CSS file is included in your HTML.
    It is done by default in the `bolero-app` project template since version 0.21.11, but for projects created earlier, it needs to be added.

    ```fsharp
    let index = doctypeHtml {
        head {
            // The file name is based on the client project name:
            link { attr.rel "stylesheet"; attr.href "MyApp.Client.styles.css" }
        }
        body {
            // ...
        }
    }
    ```

And that's it, the link in `MyComponent` will appear with a light blue background!

The property `CssScope` is available on all Bolero component base types, including `Component`, `ElmishComponent<'model, 'msg>` and `ProgramComponent<'model, 'msg>`.

The name of the stylesheet in the module `CssScopes` is based on the file name; it can be customized in `Client.fsproj`:

```xml
<ItemGroup>
  <BoleroScopedCss Update="MyStylesheet.bolero.css" ScopeName="MyScope" />
</ItemGroup>
```

## Elmish 4.0

The dependency on Elmish has been updated to version 4. [#288](https://github.com/fsbolero/Bolero/issues/288)

This release significantly revamps subscriptions.
See [the Elmish documentation](https://elmish.github.io/elmish/docs/subscription.html#migrating-from-v3) for a guide to convert Elmish subscriptions from v3 to v4.

## HTML Element References in Templates

It is now possible to bind an `HtmlRef` to an element from an HTML template. [#290](https://github.com/fsbolero/Bolero/issues/290)

The attribute `ref="${MyRef}"` will generate a method `.MyRef()` on the provided type, which takes the `HtmlRef` as argument.

```html
<button onclick="FocusBtn">Focus the input box</button>
<input ref="${InputRef}" type="text" />
```

```fsharp
type MyComponentTemplate = Template<"MyComponent.html">

type MyComponent() =
    inherit Component()

    let inputRef = HtmlRef()

    override _.Render() =
        MyComponentTemplate()
            .InputRef(inputRef)
            .FocusBtn(fun _ ->
                match inputRef.Value with
                | Some inputRef -> inputRef.FocusAsync() |> ignore
                | None -> failwith "Input is not bound")
            .Elt()
```

Happy coding!
