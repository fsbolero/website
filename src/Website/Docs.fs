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
    }

type [<CLIMutable>] Page =
    {
        title: string option
        url: string
        content: string
        headers: SidebarItem[]
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

    let headerRE = Regex("^# (.*)", RegexOptions.Compiled ||| RegexOptions.Multiline)

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
                }
            |]
        let content = Markdown.ToHtml(content, pipeline)
        { Yaml.OfYaml<Page> header with
            content = content
            headers = headers
        }

    let parsePages() =
        Directory.GetFiles(baseDir, "*.md", SearchOption.AllDirectories)
        |> Array.map (fun fullPath ->
            let path = fullPath.[baseDir.Length..].Replace('\\', '/').Trim('/')
            eprintfn "Parsing %s" path
            let path = path.[..path.Length - 4] // Trim .md
            let url = if path = "index" then "/docs" else "/docs/" + path
            path, { parseFile fullPath with url = url })
        |> dict

    let parseSidebar (pages: IDictionary<string, Page>) =
        baseDir </> "Sidebar.yml"
        |> File.ReadAllText
        |> Yaml.OfYaml<SidebarItem[]>
        |> Array.map (fun item ->
            let page = pages.Values |> Seq.find (fun page -> page.url = item.url)
            let title =
                match item.title, page.title with
                | null, None -> failwithf "Sidebar item has no title: %s" item.url
                | null, Some t | t, _ -> t
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
