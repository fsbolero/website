---
title: Bolero 0.25 released
subtitle: With .NET 10 support
---

We are happy to announce the release of [Bolero](https://fsbolero.io) version 0.25.
Bolero is a library that enables writing full-stack appications in F#, whose interactivity runs in WebAssembly or on the server side using Blazor.

Install the latest project template with:

```sh
dotnet new -i Bolero.Templates::0.25.17
```

## .NET 10 support

Bolero 0.25 adds support for .NET 10. The library itself is updated to work on .NET 10, and the project template now creates a .NET 10 project by default.

Bolero 0.25 also drops support for .NET 6 and 7, which are out of support by Microsoft.

## HTML template improvements

* For event handlers and data bindings, there are now overloads for callbacks returning `Task` or `Async<unit>` rather than `unit`.

    ```html
    <button onclick="${Click}">Click me!</button>
    ```

    ```fsharp
    type MyButton = Template<"index.html">

    let view (js: IJSRuntime) model dispatch =
        Template()
            .Click(fun e -> js.InvokeAsync("console.log", e).AsTask() :> Task)
            .Elt()
    ```

* The code generation for HTML templates is now optimized to generate a linear sequence of instructions rather than building lists of attributes and elements. This improves performance and memory allocations, and brings it in line with computation expressions (and razor pages in classic C# Blazor).

Happy coding!
