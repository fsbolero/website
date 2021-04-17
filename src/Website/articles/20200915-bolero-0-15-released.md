---
title: Bolero 0.15 released
subtitle: Update to Elmish 3.0, improvements to element binders and various fixes.
---

We are happy to announce the release of [Bolero](https://fsbolero.io) version 0.15. Bolero is a library that enables writing full-stack applications in F#, whose client side runs in WebAssembly using [Blazor](https://blazor.net/).

This release requires the .NET Core SDK version 3.1.402, which you can download [here](https://dotnet.microsoft.com/download/dotnet-core/3.1) for Windows, OSX or Linux.

Install the latest project template with:

```shell
dotnet new -i Bolero.Templates::0.15.4
```


## Features

* [#56](https://github.com/fsbolero/bolero/issues/56): Update to Elmish 3.0. Also update the `Cmd` module to match Elmish 3's API, adding submodules `Cmd.OfAuthorized` and `Cmd.OfJS`.

* [#163](https://github.com/fsbolero/bolero/issues/163) Rework the HTML element reference API and add Blazor component reference:
    * `ElementRefBinder` renamed to `HtmlRef` (old name still available but obsolete)
    * `attr.bindRef` renamed to `attr.ref` (old name still available but obsolete)
    * `attr.ref` taking a function removed
    * `ref.Ref` renamed to `ref.Value`

    ```fsharp
    let theDiv = HtmlRef()    // Used to be: let theDiv = ElementRefBinder()

    div [
        attr.ref theDiv    // Used to be: attr.bindRef theDiv
        on.click (fun _ -> doSomethingWith theDiv.Value)    // Used to be: doSomethingWith theDiv.Ref
    ] []
    ```

    * Added `Ref<'Component>` which provides the same capability for Blazor components, using the same `attr.ref`:

    ```fsharp
    let theComp = Ref<MyComponent>()

    comp<MyComponent> [
        attr.ref theComp
        on.click (fun _ -> doSomethingWith theComp.Value)
    ] []
    ```

* [#168](https://github.com/fsbolero/bolero/issues/168): Move the module `Bolero.Html` to a separate assembly and make all of its functions inline, in order to reduce the downloaded binary size.

## Fixes

* [#144](https://github.com/fsbolero/bolero/issues/144): When a router is enabled and the user clicks a link that points to a URI not handled by the router, do navigate to this URI.

* [#166](https://github.com/fsbolero/bolero/issues/166): Ensure that Elmish subscriptions and init commands are not run during server-side prerender.

* [#174](https://github.com/fsbolero/bolero/issues/174): Change ShouldRender to invoke override instead of base implementation (thanks @dougquidd!)

* [#175](https://github.com/fsbolero/bolero/issues/175): Do not render Ref until after Child Content (thanks @dougquidd!)

Happy coding!
