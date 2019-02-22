module Website.Main

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Notation

type EndPoint =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "GET /docs"; Wildcard>] Docs of string

type MainTemplate = Templating.Template<"index.html">

[<JavaScript>]
module Client =
    open WebSharper.HighlightJS
    open WebSharper.JavaScript

    [<Require(typeof<Resources.Languages.Fsharp>)>]
    [<Require(typeof<Resources.Styles.Vs>)>]
    let HighlightCode() =
        JS.Document.QuerySelectorAll("code[class^=language-]").ForEach(
            (fun (node, _, _, _) -> Hljs.HighlightBlock(node)),
            JS.Undefined
        )

module Site =
    open WebSharper.UI.Server

    let Menu = [
        "Home", "/"
        "Documentation", "/docs"
        "Blog", "/blog"
        "Try F#", "https://try.fsbolero.io"
    ]

    let Page (title: option<string>) (body: Doc) =
        MainTemplate()
#if !DEBUG
            .ReleaseMin(".min")
#endif
            .Title(
                match title with
                | None -> ""
                | Some t -> t + " | "
            )
            .ShowDrawer(fun e -> e.Vars.DrawerShown := "shown")
            .HideDrawer(fun e -> e.Vars.DrawerShown := "")
            .TopMenu([for text, url in Menu -> MainTemplate.TopMenuItem().Text(text).Url(url).Doc()])
            .DrawerMenu([for text, url in Menu -> MainTemplate.DrawerMenuItem().Text(text).Url(url).Doc()])
            .Body(body)
            .Elt()
            .OnAfterRender(fun _ -> Client.HighlightCode())
        |> Content.Page

    let HomePage () =
        MainTemplate.HomeBody()
            .Doc()
        |> Page None

    let DocPage (doc: Docs.Document) =
        MainTemplate.DocsBody()
            .Sidebar(Doc.Verbatim Docs.Sidebar)
            .Content(Doc.Verbatim doc.content)
            .Doc()
        |> Page doc.title

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx action ->
            match action with
            | Home -> HomePage ()
            | Docs p -> DocPage Docs.Pages.[p]
        )

[<Sealed>]
type Website() =
    interface IWebsite<EndPoint> with
        member this.Sitelet = Site.Main
        member this.Actions = [
            yield Home
            for p in Docs.Pages.Keys do
                yield Docs p
        ]

[<assembly: Website(typeof<Website>)>]
do ()
