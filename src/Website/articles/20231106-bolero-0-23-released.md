---
title: Bolero 0.23 released
subtitle: Router query parameters and .NET 8 compatibility
---

We are happy to announce the release of [Bolero](https://fsbolero.io) version 0.23.
Bolero is a library that enables writing full-stack applications in F#, whose client side runs in WebAssembly using Blazor.

The main highlights of this release are:
* Improvements to routing, including query parameters, not-found handling and hash scrolling.
* .NET 8 compatibility.
* Better C# compatibility for server-side.

Install the latest project template with:

```
dotnet new -i Bolero.Templates::0.23.10
```

## Routing improvements

### Query parameters ([#309](https://github.com/fsbolero/Bolero/issues/309))

Inferred routers can now handle query parameter parsing. It is done using the syntax `?paramName={fieldName}`.

```fsharp
type Page =
    | [<EndPoint "/">] Home
    | [<EndPoint "/articles?page={pageNum}&count={countPerPage}"] Articles of pageNum: int * countPerPage: int
```

Using the router above, `/articles?page=3&count=20` corresponds to the page value `Articles (3, 20)`.

Query parameters are optional if the corresponding field has type `option` or `voption`, and mandatory otherwise.

Additionally, the syntax `?{paramName}` is short for `?paramName={paramName}`.

```fsharp
type Page =
    | [<EndPoint "/articles?{tag}&{page}&{count}"] Articles of tag: string option * page: int option * count: int
```

Using the router above, `/articles?tag=fsharp&count=20` corresponds to the page value `Articles (Some "fsharp", None, 20)`.

Of course, path parameters and query parameters can be mixed in the same endpoint.

```fsharp
type Page =
    | [<EndPoint "/articles/{tag}?{page}"] Articles of tag: string * page: int option
```

Using the router above, `/articles/fsharp?page=20` corresponds to the page value `Articles ("fsharp", Some 20)`.

### "Not found" handler ([#308](https://github.com/fsbolero/Bolero/issues/308))

Routers can indicate what endpoint to use when the URL is invalid using the function `Router.withNotFound`.

```
type Page =
    | Home
    | // ...

type Message =
    | SetPage of Page
    | // ...

type Model =
    { page: Page
      // ...
    }

let router =
    Router.infer PageChanged (fun m -> m.page)
    |> Router.withNotFound Home
```

This function applies when the initial URL is invalid, or when the URL is programmatically changed to be invalid.
It does not apply when the user clicks a link to an invalid URL, in order to allow external links.

### Inner links with hashes ([#315](https://github.com/fsbolero/Bolero/issues/315))

Navigating to a Bolero routed URL with a `#hash` will properly scroll to the element named `hash` in the target page.

```fsharp
div {
    h1 {
        attr.id "the-title"
        "The title of this section"
    }
    a {
        attr.href (router.Link Home + "#the-title")
        "Navigate and scroll to the title of this section"
    }
}
```

Additionally, the methods `IRouter.Link` and `IRouter.HRef` take a new optional argument `hash: string` to link to the corresponding `#hash` in the target page.

```fsharp
div {
    h1 {
        attr.id "the-title"
        "The title of this section"
    }
    a {
        router.HRef(Home, hash = "the-title")
        "Navigate and scroll to the title of this section"
    }
}
```

## .NET 8 Compatibility fixes ([#317](https://github.com/fsbolero/Bolero/issues/317))

* Fix the server-side rendering and prerendering for .NET 8.

* Update the MSBuild task generating scoped CSS.

## C# compatibility of server-side APIs ([#313](https://github.com/fsbolero/Bolero/issues/313))

C# compatibility has been improved for server-side hosting APIs, in order to make it easier to integrate Bolero into an existing C# ASP.NET Core application.

More specifically, for the following extension methods:

* `IServiceCollection.AddBoleroHost`
* `IServiceCollection.AddBoleroRouting`
* `IEndpointRouteBuilder.MapFallbackToBolero`

The following changes are applied:

* Optional arguments are changed from F#-style to C#-style. This is a source breaking change for callers who use explicit syntax `?argument = optionValue`.
* Function arguments are changed from F#-style functions to C#-style `Func` or `Action`.

These changes should be source-compatible for most existing F# host applications.

## Event handler fixes

* Fix `on.stopPropagation` and `on.preventDefault` to take event names without the `on` prefix, for consistency with `on.event`. ([#316](https://github.com/fsbolero/Bolero/issues/316))

* Use `WheelEventArgs` for wheel events and `ErrorEventArgs` for `on.error`. ([#323](https://github.com/fsbolero/Bolero/issues/323))

Happy coding!
