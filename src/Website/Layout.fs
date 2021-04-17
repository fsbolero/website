module Website.Layout

open System.IO
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Server

type MainTemplate = Templating.Template<"index.html">

let Menu = [
    "Home", "/"
    "Documentation", "/docs"
    "Blog", "/blog"
    "Try F#", "https://tryfsharp.fsbolero.io"
]

let private head =
    __SOURCE_DIRECTORY__ + "/js/Client.head.html"
    |> File.ReadAllText
    |> Doc.Verbatim

let Page (title: option<string>) hasBanner (docs: Docs.Docs) (body: Doc) =
    MainTemplate()
#if !DEBUG
        .ReleaseMin(".min")
#endif
        .NavbarOverlay(if hasBanner then "overlay-bar" else "")
        .Head(head)
        .Title(
            match title with
            | None -> ""
            | Some t -> t + " | "
        )
        .TopMenu(Menu |> List.map (function
            | text, ("/docs" as url) ->
                let items = docs.sidebar |> Array.map (fun item ->
                    MainTemplate.TopMenuDropdownItem()
                        .Text(item.title)
                        .Url(item.url)
                        .Doc())
                MainTemplate.TopMenuItemWithDropdown()
                    .Text(text)
                    .Url(url)
                    .DropdownItems(items)
                    .Doc()
            | text, url ->
                MainTemplate.TopMenuItem()
                    .Text(text)
                    .Url(url)
                    .Doc()
        ))
        .DrawerMenu(Menu |> List.map (fun (text, url) ->
            MainTemplate.DrawerMenuItem()
                .Text(text)
                .Url(url)
                .Children(
                    match url with
                    | "/docs" ->
                        ul [] (docs.sidebar |> Array.map (fun item ->
                            MainTemplate.DrawerMenuItem()
                                .Text(item.title)
                                .Url(item.url)
                                .Doc()
                        ))
                    | _ -> Doc.Empty
                )
                .Doc()
        ))
        .Body(body)
        .FooterDocs(docs.sidebar |> Array.map (fun item ->
            MainTemplate.FooterDoc()
                .Title(item.title)
                .Url(item.url)
                .Doc()
        ))
        .Doc()
    |> Content.Page
