---
title: Bolero 0.21 released
subtitle: Revamped server-side rendering
---

We are happy to announce the release of [Bolero](https://fsbolero.io) version 0.21.
Bolero is a library that enables writing full-stack applications in F#, whose client side runs in WebAssembly using Blazor.

The main highlight of this release is a revamp of server-side rendering that fixes prerendering and provides new functions to convert Bolero HTML directly to string.

Install the latest project template with:

```
dotnet new -i Bolero.Templates::0.21.24
```

## Changes

* [#261](https://github.com/fsbolero/Bolero/issues/261) Fix prerendering of components inside server-side Bolero.Html.

* To instantiate a client-side component inside server-side Bolero.Html, the standard `comp<T>` can now be used instead of the dedicated `rootComp<T>`.

* [#275](https://github.com/fsbolero/Bolero/issues/275) Add new module `Bolero.Server.Components.Rendering` with functions:

    * `renderPlain : Node -> string` renders a node to raw HTML. Blazor components are ignored.

    * `renderPage : Node -> HttpContext -> IHtmlHelper -> IBoleroHostConfig -> string` also renders a node to raw HTML. Blazor components are rendered according to the given host config.


* [#279](https://github.com/fsbolero/Bolero/issues/279) Bolero.Build: Disable the production of a reference assembly.
    This fixes occurrences of error FS2030 that happen on Bolero client projects in .NET 7.

* [#285](https://github.com/fsbolero/Bolero/issues/285) Fix typed component builders so that nested components aren't forced to have the same type.

Happy coding!
