module Website.Docs

open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions
open Markdig
open Website

type [<CLIMutable>] SidebarItem =
    {
        title: string
        url: string
        hideIfNotCurrent: bool
    }

type [<CLIMutable>] Page =
    {
        title: string
        subtitle: string
        url: string
        content: string
        headers: SidebarItem[]
        hideTitle: bool
        hideEditLink: bool
    }

type Docs =
    {
        sidebar: SidebarItem[]
        pages: IDictionary<string, Page>
    }

[<AutoOpen>]
module private Impl =

    let pipeline =
        MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseYamlFrontMatter()
            .Build()

    let (</>) x y = Path.Combine(x, y)

    let baseDir = __SOURCE_DIRECTORY__ </> "docs"

    let headerRE = Regex("^## (.*)", RegexOptions.Compiled ||| RegexOptions.Multiline)

    let parseFile fullPath =
        let header, content =
            fullPath
            |> File.ReadAllText
            |> Yaml.SplitHeader
        let headers : SidebarItem[] =
            [| for c in headerRE.Matches(content) do
                let title = c.Groups.[1].Value
                yield {
                    title = title
                    url = "#" + Markdig.Helpers.LinkHelper.UrilizeAsGfm title
                    hideIfNotCurrent = false
                }
            |]
        let content = Markdown.ToHtml(content, pipeline)
        let meta = Yaml.OfYaml<Page> header
        { meta with
            content = content
            headers = headers
            subtitle = Markdown.ToHtml("{.subtitle}\n" + meta.subtitle, pipeline)
        }

    let parsePages() =
        Directory.GetFiles(baseDir, "*.md", SearchOption.AllDirectories)
        |> Array.map (fun fullPath ->
            let filename = fullPath.[baseDir.Length..].Replace('\\', '/').Trim('/')
            eprintfn "Parsing %s" filename
            let name = Path.GetFileNameWithoutExtension(filename)
            let url = if name = "index" then "/docs" else "/docs/" + name
            name, { parseFile fullPath with url = url })
        |> dict

    let parseSidebar (pages: IDictionary<string, Page>) =
        baseDir </> "Sidebar.yml"
        |> File.ReadAllText
        |> Yaml.OfYaml<SidebarItem[]>
        |> Array.map (fun item ->
            let page = pages.Values |> Seq.find (fun page -> page.url = item.url)
            let title = match item.title with null -> page.title | t -> t
            { item with title = title }
        )

// A function called from Website() rather than a top-level value
// because if there's an error in a top-level value,
// all you get is "Static ctor threw an exception" without the actual exception.
let Compute() =
    let pages = parsePages()
    {
        pages = pages
        sidebar = parseSidebar pages
    }
