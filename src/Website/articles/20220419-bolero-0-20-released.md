---
title: Bolero 0.20 released
subtitle: A new HTML syntax on .NET 6
---

We are happy to announce the release of [Bolero](https://fsbolero.io) version 0.20.
Bolero is a library that enables writing full-stack applications in F#, whose client side runs in WebAssembly using Blazor.

The main highlight of this release is a new syntax for writing HTML in F# that is both visually lighter and more efficient at runtime.

Install the latest project template with:

```
dotnet new -i Bolero.Templates::0.20.4
```

## Changes

* Bolero now requires .NET 6.

* [#249](https://github.com/fsbolero/Bolero/issues/249) Use computation expressions for the HTML syntax.
    Instead of functions taking lists of attributes and children, HTML now looks like this:

    ```fsharp
    concat {
        "Welcome to "
        a {
            attr.href "https://fsbolero.io"
            attr.id "link"
            "Bolero"
        }
        "!"
    }
    ```

    This new syntax also applies to Blazor components.
    For example, to use a [MatBlazor](https://www.matblazor.com/) button:

    ```fsharp
    comp<MatButton> {
        attr.callback "OnClick" (fun _ -> dispatch ButtonClicked)
        "Link" => "https://fsbolero.io"
        "Target" => "_blank"

        "Click me!"
    }
    ```

    The [`virtualize`](20210417-bolero-0-17-released) component uses `let!` to bind the current item:

    ```fsharp
    let numbers = [ 1 .. 100 ]

    ul {
        virtualize.comp {
            virtualize.itemSize 20f

            let! item = virtualize.items numbers
            li {
                $"Item #{item}"
            }
        }
    }
    ```

    In addition to being slightly more concise, this syntax is more efficient performance-wise; see the linked issue for more details.

* The values `empty` and `attr.empty` are now functions: `empty()` / `attr.empty()`.

* CSS classes from multiple calls to `attr.classes` and/or ``` attr.``class`` ``` are no longer combined together into a single `class` attribute.
    This feature was incompatible with the code inlining that makes the new syntax performant.

    To combine CSS classes, combine them with `String.concat " "` and pass the result to ``` attr.``class`` ```.

* Bolero's assemblies are now trimmed during publish in WebAssembly mode, for a smaller download size.

Happy coding!
