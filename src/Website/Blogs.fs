namespace Website.Blogs

open System
open System.IO
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open Website

module Helpers =
    open System.Text.RegularExpressions

    // Return (fullpath, filename, (year, month, day), slug, extension)
    let (|ArticleFile|_|) (fullpath: string) =
        let s = Path.GetFileName(fullpath)
        let r = new Regex("([0-9]+)-([0-9]+)-([0-9]+)-(.+)\.(md|html)")
        if r.IsMatch(s) then
            let a = r.Match(s)
            let V (i: int) = a.Groups.[i].Value
            Some (fullpath, V 0, (V 1, V 2, V 3), V 4, V 5)
        else
            None

type BlogConfig =
    {
        /// The folder that contains the post sources, relative to the webroot.
        PostsFolder: string
        /// The folder that contains the layout templates, relative to the webroot.
        LayoutsFolder: string
    }

type [<CLIMutable>] Site =
    {
        url: string
        baseurl: string
        theme_settings: ThemeSettingsModel
        paginate: int
        paginate_path: string
        excerpt_separator: string
        markdown: string
        highlighter: string
    }

and [<CLIMutable>] ThemeSettingsModel =
    {
        title: string
        avatar: string
        favicon: string
        gravatar: string
        description: string
        header_text: string
        header_text_feature_image: string
        footer_text: string
        rss: bool
        email_address: string
        behance: string
        bitbucket: string
        dribbble: string
        facebook: string
        flickr: string
        gitlab: string
        github: string
        google_plus: string
        instagram: string
        linkedin: string
        pinterest: string
        reddit: string
        soundcloud: string
        stack_exchange: string
        steam: string
        tumblr: string
        twitter: string
        wordpress: string
        youtube: string

        google_analytics: string
        disqus_shortname: string
        katex: bool
        search: bool

        str_follow_on: string
        str_rss_follow: string
        str_email: string
        str_next_post: string
        str_previous_post: string
        str_next_page: string
        str_previous_page: string
        str_continue_reading: string
        str_javascript_required_disqus: string
        str_search_no_results: string

        google_fonts: string
        post_navigation: bool
    }

module Runtime =
    type [<CLIMutable>] TemplatePage =
        {
            layout: string
            excerpt_separator: string
        }

    type Post =
        {
            url: string
            title: string
            date: string
            excerpt: string
            content: string
        }

    type [<CLIMutable>] Page =
        {
            title: string
            headline: string
            layout: string
            next: Page
            previous: Page
        }

    // A stateful paginator, updated via `Update`.
    type Paginator =
        {
            mutable page: int
            per_page: int
            posts: Post list
            total_posts: int
            total_pages: int
            mutable previous_page: string // Number or ""
            mutable previous_page_path: string // Path or ""
            mutable next_page: string // Number or ""
            mutable next_page_path: string // Path or ""
        }

        member this.FindIndex(page, f: string -> string) =
            this.posts
            |> List.mapi (fun i post -> i+1, post)
            |> List.tryFind (fun (i, post) -> f post.url = page)
            |> Option.defaultWith (fun () -> failwithf "Can not paginate unknown page (%s)" page)

        member this.Update(page: string, f: string -> string) =
            let index, _ = this.FindIndex(page, f)
            let prevPage, prevPagePath =
                match int(Math.Ceiling(float index / float this.per_page)) with
                | pn when pn <= 1 ->
                    "", ""
                | pn ->
                    string pn, string pn // TODO: fix URL
            let nextPage, nextPagePath =
                if index < this.total_posts - this.per_page then
                    let next = int (float index / float this.total_posts * float this.per_page) + 1
                    string next, string next // TODO: fix URL
                else
                    "", ""
            this.page <- index
            this.previous_page <- prevPage
            this.previous_page_path <- prevPagePath
            this.next_page <- nextPage
            this.next_page_path <- nextPagePath

        // Updates the given Page with next/previous based on the given slug.
        member this.UpdatePage(slug: string, page: Page) =
            //let index, post = this.FindIndex(page, f)
            //{ page with
            //    next =
            //    previous: Page
            //}
            page

        static member BuildPostList (config: BlogConfig) =
            let folder = Path.Combine (__SOURCE_DIRECTORY__, config.PostsFolder)
            eprintfn "folder=%s" folder
            if Directory.Exists folder then
                // .md files are preferred and take precedence over .html ones
                let htmls = Directory.EnumerateFiles(folder, "*.html", SearchOption.AllDirectories)
                let mds = Directory.EnumerateFiles(folder, "*.md", SearchOption.AllDirectories)
                Seq.append htmls mds
                |> Seq.toList
                |> List.map (fun fname -> eprintfn "Found file: %s" fname; fname)
                // Filter out ill-formed filenames
                |> List.choose (Helpers.(|ArticleFile|_|))
            else
                eprintfn "warning: the posts folder (%s) does not exist." folder
                []

        static member Build (config: BlogConfig, site: Site) =
            // Mapping filenames (without extension) to (Post, <header>) pairs
            let posts =
                Paginator.BuildPostList config
                |> List.map (fun (path, filename, (y, m, d), slug, ext) ->
                    let filename = Path.GetFileNameWithoutExtension filename
                    eprintfn "---> Added %s to the posts collection" path
                    let header, content =
                        path
                        |> File.ReadAllText
                        |> Yaml.SplitHeader
                    filename,
                    ({
                        url = sprintf "/blog/%s" filename // ctx.Link (EndPoint.BlogPage filename)
                        title = slug
                        date = sprintf "%s-%s-%s" y m d // TODO: enable date customization
                        excerpt = "short excerpt here..."
                        content = content
                    }, header)
                )
                |> Map.ofList
            let totalPosts = Map.count posts
            {
                page = 1
                per_page = site.paginate
                posts = posts |> Map.toList |> List.map (fun (_, (post, _)) -> post)
                total_posts = totalPosts
                total_pages = float totalPosts / float site.paginate |> Math.Ceiling |> int
                previous_page = "" // TODO
                previous_page_path = "" // TODO
                next_page = "" // TODO
                next_page_path = "" // TODO
            }

type TemplateModel =
    {
        page: Runtime.Page
        site: Site
        content: string
        paginator: Runtime.Paginator
    }

module Jekyll =
    open System.Collections.Generic
    open Microsoft.FSharp.Reflection
    open DotLiquid
    open WebSharper.UI.Templating

    exception NotFound of path: string

    type Template = Templating.Template< @"_layouts\default.html", ClientLoad.FromDocument>

    type SlugType =
        | BlogPost of slug: string
        | Index of slug: string

    module Markdown =
        open Markdig

        let ToHtml (s: string) =
            // For a list of pipeline extensions, see
            //    https://github.com/lunet-io/markdig
            let pipeline =
                (new MarkdownPipelineBuilder())
                    .UsePipeTables()
                    .UseGridTables()
                    .UseListExtras()
                    .UseEmphasisExtras()
                    .UseGenericAttributes()
                    .UseAutoLinks()
                    .UseTaskLists()
                    .UseMediaLinks()
                    .UseCustomContainers()
                    .UseMathematics()
                    .UseEmojiAndSmiley()
                    .Build()
            Markdown.ToHtml(s, pipeline)

    module Liquid =
        /// Given a type which is an F# record containing seq<_>, list<_>, array<_>, option and
        /// other records, register the type with DotLiquid so that its fields are accessible
        /// This is a copy of https://github.com/SuaveIO/suave/blob/master/src/Suave.DotLiquid/Library.fs
        let safe =
            let o = obj()
            fun f -> lock o f

        // Register types with DotLiquid by unfolding them.
        //
        let RegisterTypeTree ty =
            let registered = Dictionary<_, _>()
            let rec loop ty =
                if not (registered.ContainsKey ty) then
                    if FSharpType.IsRecord ty then
                        let fields = FSharpType.GetRecordFields ty
                        Template.RegisterSafeType(
                            ty,
                            fields |> Array.map (fun f -> f.Name)
                        )
                        registered.[ty] <- true
                        for f in fields do loop f.PropertyType
                    elif ty.IsGenericType then
                        let t = ty.GetGenericTypeDefinition()
                        registered.[ty] <- true
                        eprintfn "Checking generic type: %A" t
                        if t = typedefof<seq<_>> || t = typedefof<list<_>>  then
                            loop (ty.GetGenericArguments().[0])
                        elif t = typedefof<option<_>> then
                            Template.RegisterSafeType(ty, [|"Value"|])
                            loop (ty.GetGenericArguments().[0])
                    elif ty.IsArray then
                        registered.[ty] <- true
                        loop (ty.GetElementType())
                    else
                        eprintfn "--- type fell through: %A" ty
            safe (fun () -> loop ty)

        let WithLiquidTemplate<'T> (model: 'T) template =
            Template.NamingConvention <- DotLiquid.NamingConventions.RubyNamingConvention()
            let root = Path.Combine(__SOURCE_DIRECTORY__, "_layouts")
            eprintfn "ROOT is %s" root
            Template.FileSystem <- new FileSystems.LocalFileSystem(root)
            RegisterTypeTree typeof<'T>
            let view = Template.Parse template
            let res =
                model
                |> Hash.FromAnonymousObject
                |> view.Render
            res

    let BlogPage ctx (config: BlogConfig) (site: Site, paginator: Runtime.Paginator) (slugTy: SlugType) =
        eprintfn "debug: %A" slugTy
        try
            let slug, sourceFolder =
                match slugTy with
                | SlugType.BlogPost slug -> slug, config.PostsFolder
                | SlugType.Index slug -> slug, ""
            let header, content =
                let FILE ext = Path.Combine(__SOURCE_DIRECTORY__, sourceFolder, slug)
                // .md files are preferred and take precedence over .html ones
                if FILE ".md" |> File.Exists then
                    FILE ".md"
                    |> File.ReadAllText
                    |> Yaml.SplitHeader
                elif FILE ".html" |> File.Exists then
                    FILE ".html"
                    |> File.ReadAllText
                    |> Yaml.SplitHeader
                else
                    raise <| NotFound (FILE ".md")
            // Get the JSON representation of the YAML header
            let page = Yaml.OfYaml<Runtime.Page> header
            //let page = paginator.UpdatePage page
            eprintfn "page=%A" page
            // Update the paginator
            match slugTy with
            | SlugType.BlogPost slug ->
                //paginator.Update(slug, fun url -> url.Replace("/blog/", ""))
                ()
            | SlugType.Index _ ->
                ()
            let model =
                {
                    page = page
                    content = "[CONTENT BEING INITIALIZED]"
                    site = site
                    paginator = paginator
                }
            // Find the template to instantiate.
            let templateText, content =
                let template =
                    // If no layout is specified, we assume "default"
                    if String.IsNullOrEmpty page.layout then
                        "default"
                    else
                        page.layout
                let masterHeader, masterContent =
                    Path.Combine(__SOURCE_DIRECTORY__, config.LayoutsFolder, template + ".html")
                    |> File.ReadAllText
                    |> Yaml.SplitHeader
                // If the template doesn't specify a master layout, use it as source
                if String.IsNullOrEmpty header then
                    masterContent, content |> Liquid.WithLiquidTemplate model |> Markdown.ToHtml
                // Otherwise, we plug post content into the layout specified
                else
                    eprintfn "DEBUG: Master template found, processing..."
                    let master = Yaml.OfYaml<Runtime.TemplatePage> masterHeader
                    let templateText =
                        Path.Combine(__SOURCE_DIRECTORY__, config.LayoutsFolder, master.layout + ".html")
                        |> File.ReadAllText
                    let model = { model with content = content |> Liquid.WithLiquidTemplate model |> Markdown.ToHtml }
                    templateText, masterContent |> Liquid.WithLiquidTemplate model |> Markdown.ToHtml
            // Assemble our blog page model.
            let model = { model with content = content }
            // Run the Liquid engine on the template and the page model.
            let res = Liquid.WithLiquidTemplate model templateText
            // Return the computed page bound to the default template.
            // This is where you can plug in any further WebSharper content.
            Content.Page(
                Template(res)
                    .Doc()
            )
        with
        | NotFound file ->
            eprintfn "Not found: %s" file
            Content.NotFound
        | exn ->
            eprintfn "Exception: %A" exn
            Content.ServerError
