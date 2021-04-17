module Website.Main

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI

type EndPoint =
    | [<EndPoint "GET /">] Home
    | [<EndPoint "GET /docs"; Wildcard>] Docs of string
    | [<EndPoint "/blog">] BlogPage of slug: string

module Site =
    open WebSharper.UI.Html
    open Website.Blogs

    let HomePage docs =
        Layout.MainTemplate.HomeBody()
            .Doc()
        |> Layout.Page None true docs

    let PlainHtml html =
        div [Attr.Create "ws-preserve" ""] [Doc.Verbatim html]


    let DocSidebar (docs: Docs.Docs) (doc: Docs.Page) =
        let mutable foundCurrent = false
        let res =
            docs.sidebar
            |> Array.map (fun item ->
                let tpl =
                    Layout.MainTemplate.SidebarItem()
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
                                Layout.MainTemplate.SidebarSubItem()
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
        if not foundCurrent then
            failwithf "Doc missing from the sidebar: %s" doc.url
        res

    let DocPage (docs: Docs.Docs) (pageName: string) (doc: Docs.Page) =
        Layout.MainTemplate.DocsBody()
            .Title(
                if doc.hideTitle then
                    Doc.Empty
                else
                    Layout.MainTemplate.DocsTitle()
                        .Title(doc.title)
                        .Subtitle(Doc.Verbatim doc.subtitle)
                        .Doc()
            )
            .EditPageAttr(if doc.hideEditLink then attr.style "display: none" else Attr.Empty)
            .GitHubUrl("https://github.com/fsbolero/website/tree/master/src/Website/docs/" + pageName + ".md")
            .Sidebar(DocSidebar docs doc)
            .Content(PlainHtml doc.content)
            .Doc()
        |> Layout.Page (Some doc.title) false docs

    let BlogSidebar (blogs: Blogs.Articles) (page: Blogs.Page) =
        blogs.pages.Values
        |> Seq.sortByDescending (fun a -> a.date)
        |> Seq.map (fun a ->
            let isCurrent = a.url = page.url
            Layout.MainTemplate.SidebarItem()
                .Title(a.title)
                .Url(a.url)
                .LinkAttr(if isCurrent then attr.``class`` "is-active" else Attr.Empty)
                .SubItemsAttr(attr.``class`` "is-hidden")
                .Doc())

    let BlogPage (blogs: Blogs.Articles) docs (page: Blogs.Page) =
        Layout.MainTemplate.BlogsBody()
            .Title(page.title)
            .Subtitle(Doc.Verbatim page.subtitle)
            .Sidebar(BlogSidebar blogs page)
            .Content(PlainHtml page.content)
            .Doc()
        |> Layout.Page (Some page.title) false docs

    let Main docs blogs =
        let (KeyValue(blogIndexSlug, _)) =
            blogs.pages
            |> Seq.maxBy (fun (KeyValue(_, v)) -> v.date)
        let rec page ctx = function
            | Home ->
                HomePage docs
            | BlogPage "index" ->
                blogIndexSlug
                |> EndPoint.BlogPage
                |> page ctx
            | BlogPage p ->
                BlogPage blogs docs blogs.pages.[p]
            | Docs p ->
                DocPage docs p docs.pages.[p]
        Application.MultiPage page

[<Sealed>]
type Website() =
    let docs = Docs.Compute()
    let blogs = Blogs.Compute()

    interface IWebsite<EndPoint> with
        member this.Sitelet = Site.Main docs blogs
        member this.Actions = [
            yield Home
            for p in docs.pages.Keys do
                yield Docs p
            yield BlogPage "index"
            for p in blogs.pages.Keys do
                yield BlogPage p
        ]

[<assembly: Website(typeof<Website>)>]
do ()
