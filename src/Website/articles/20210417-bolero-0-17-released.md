---
title: Bolero 0.17 released
subtitle: Define static content using Bolero.Html
---

We are happy to announce the release of [Bolero](https://fsbolero.io) version 0.17.
Bolero is a library that enables writing full-stack applications in F#, whose client side runs in WebAssembly using Blazor.

The main highlight of this release is the capability to use Bolero.Html functions to generate the static server-side HTML.
This comes as a replacement for the dynamically-compiled Razor page that was previously necessary for ASP.NET Core hosted applications.

Install the latest project template with:

```
dotnet new -i Bolero.Templates::0.17.10
```

## Changes

* [#202](https://github.com/fsbolero/Bolero/issues/202) Add the ability to generate static HTML content using Bolero.Html functions.

    For example, a simple page may look like this:

    ```fsharp
    let index = doctypeHtml [] [
        head [] [
            title [] [text "Hello, world!"]
        ]
        body [] [
            div [attr.id "main"] [
                rootComp<Client.MyApp>
            ]
            boleroScript
        ]
    ]
    ```

    In this sample, the call to `rootComp` inserts the Bolero (or Blazor) component `Client.MyApp` in the page.
    The call to `boleroScript` inserts the script tag required by Blazor.

    These tags use the configuration passed to `AddBoleroHost` to determine whether the component is server-side or WebAssembly, and whether it is prerendered or not.

    This new feature is now used by default by the dotnet template instead of a dynamically-compiled Razor page.
    A new argument to `dotnet new` determines how the static HTML is generated:

    * `--hostpage=bolero` is the default and uses Bolero.Html functions.

    * `--hostpage=razor` uses a dynamically-compiled Razor page.

    * `--hostpage=html` uses a plain index.html file.

    Learn more in [the documentation](https://fsbolero.io/docs/Hosting#static-content-generation).

* [#216](https://github.com/fsbolero/Bolero/issues/216) Add helpers to create [virtualized components](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/virtualization?view=aspnetcore-5.0).
    This is a Blazor feature that allows rendering only the visible items in a collection.

    ```fsharp
    // Display a virtualized list of 100 items.
    let items = [1..100]

    virtualize.comp [] items <| fun item ->
        div [] [textf "%i" item]
    ```

    The helpers provide all the features of Blazor's `Virtualize` component, such as loading from a function rather than a collection, and placeholders while items are loading:

    ```fsharp
    // Display a virtualized list of items retrieved from a remote function.

    let getItems (r: ItemsProviderRequest) : ValueTask<ItemProviderResult> =
        async {
            let! items, totalCount = remote.GetItems(r.StartIndex, r.Count)
            return ItemsProviderResult(items, totalCount)
        }
        |> Async.StartAsTask
        |> ValueTask

    // Displayed while an item is being loaded.
    let placeholder = div [attr.classes ["my-placeholder"]] []

    virtualize.compProvider
        [ virtualize.placeholder (fun _ -> placeholder) ]
        getItems
        <| fun item ->
            div [] [textf "%i" item]
    ```

* [#205](https://github.com/fsbolero/Bolero/issues/205) **Breaking change**: the `Value` property of of `Ref<'T>` and `HtmlRef` now has type `'T option` rather than `'T`.
    It is `None` if the reference hasn't been set using `attr.ref`.

* [#214](https://github.com/fsbolero/Bolero/issues/214) Fix stripping F# metadata from assemblies when building in non-trimmed mode.

Happy coding!
