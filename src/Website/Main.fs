module Website.Main

open System
open System.Collections.Generic
open System.IO
open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Notation

type EndPoint =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "GET /docs"; Wildcard>] Docs of string
    | [<EndPoint "/blog">] BlogPage of slug: string

type MainTemplate = Templating.Template<"index.html">

module Site =
    open WebSharper.UI.Html
    open WebSharper.UI.Server
    open Website.Blogs
    open Website.Blogs.Jekyll

    let Menu = [
        "Home", "/"
        "Documentation", "/docs"
        //"Blog", "/blog"
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

    let HomePage docs =
        MainTemplate.HomeBody()
            .Doc()
        |> Page None true docs

    let PlainHtml html =
        div [Attr.Create "ws-preserve" ""] [Doc.Verbatim html]

    let DocSidebar (docs: Docs.Docs) (doc: Docs.Page) =
        let mutable foundCurrent = false
        let res =
            docs.sidebar
            |> Array.map (fun item ->
                let tpl =
                    MainTemplate.DocsSidebarItem()
                        .Title(item.title)
                        .Url(item.url)
                let tpl =
                    if item.url = doc.url then
                        if foundCurrent then
                            failwithf "Doc present twice in the sidebar: %s" doc.url
                        foundCurrent <- true
                        let children =
                            doc.headers
                            |> Array.map (fun header ->
                                MainTemplate.DocsSidebarSubItem()
                                    .Title(header.title)
                                    .Url(header.url)
                                    .Doc()
                            )
                        tpl.Children(children)
                            .LinkAttr(attr.``class`` "is-active")
                    else
                        tpl.SubItemsAttr(attr.``class`` "is-hidden")
                tpl.Doc()
            )
            |> Doc.Concat
        if not foundCurrent then
            failwithf "Doc missing from the sidebar: %s" doc.url
        res

    let DocPage (docs: Docs.Docs) (pageName: string) (doc: Docs.Page) =
        MainTemplate.DocsBody()
            .Title(
                if doc.hideTitle then
                    Doc.Empty
                else
                    MainTemplate.DocsTitle()
                        .Title(doc.title)
                        .Subtitle(Doc.Verbatim doc.subtitle)
                        .Doc()
            )
            .EditPageAttr(if doc.hideEditLink then attr.style "display: none" else Attr.Empty)
            .GitHubUrl("https://github.com/fsbolero/website/tree/master/src/Website/docs/" + pageName + ".md")
            .Sidebar(DocSidebar docs doc)
            .Content(PlainHtml doc.content)
            .Doc()
        |> Page (Some doc.title) false docs

    let blogConfig =
        {
            PostsFolder = "_posts"
            LayoutsFolder = "_layouts"
        }

    let BlogPages() =
        Runtime.Paginator.BuildPostList blogConfig

    let Main docs =
        Application.MultiPage (fun ctx action ->
            let site =
                Path.Combine(__SOURCE_DIRECTORY__, "_config.yml")
                |> File.ReadAllText
                |> Yaml.OfYaml<Site>
            printfn "site=%A" site
            let paginator = Runtime.Paginator.Build(blogConfig, site)
            printfn "paginator=%A" paginator
            match action with
            | Home ->
                HomePage docs
            | BlogPage p ->
                Jekyll.BlogPage ctx blogConfig (site, paginator) (SlugType.BlogPost p)
            | Docs p ->
                DocPage docs p docs.pages.[p]
        )

[<Sealed>]
type Website() =
    let docs = Docs.Compute()

    interface IWebsite<EndPoint> with
        member this.Sitelet = Site.Main docs
        member this.Actions = [
            yield Home
            for p in docs.pages.Keys do
                yield Docs p
            for (path, filename, (y, m, d), slug, ext) in Site.BlogPages() do
                yield BlogPage filename
        ]

[<assembly: Website(typeof<Website>)>]
do ()
