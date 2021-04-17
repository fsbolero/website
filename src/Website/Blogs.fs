module Website.Blogs

open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions
open FSharp.Text.RegexProvider
open FSharp.Text.RegexExtensions
open Markdig
open Website

type [<CLIMutable>] Page =
    {
        date: DateTime
        title: string
        subtitle: string
        url: string
        content: string
    }

type Articles =
    {
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

    let baseDir = __SOURCE_DIRECTORY__ </> "articles"

    let titleRE = new Regex<"""(?<year>\d\d\d\d)(?<month>\d\d)(?<day>\d\d)-(?<slug>.*)""">(RegexOptions.Compiled)

    let parseArticleDate filename =
        let m = titleRE.TypedMatch(filename)
        if m.Success then
            DateTime(m.year.AsInt, m.month.AsInt, m.day.AsInt)
        else
            failwithf "Invalid article filename: %s (should be YYYYMMDD-slug.md)" filename

    let parseFile fullPath =
        let header, content =
            fullPath
            |> File.ReadAllText
            |> Yaml.SplitHeader
        let content = Markdown.ToHtml(content, pipeline)
        let meta = Yaml.OfYaml<Page> header
        { meta with
            content = content
            subtitle = Markdown.ToHtml("{.subtitle}\n" + meta.subtitle, pipeline)
        }

    let parsePages() =
        Directory.GetFiles(baseDir, "*.md", SearchOption.AllDirectories)
        |> Array.map (fun fullPath ->
            let filename = fullPath.[baseDir.Length..].Replace('\\', '/').Trim('/')
            eprintfn "Parsing %s" filename
            let name = Path.GetFileNameWithoutExtension(filename)
            let url = "/blog/" + name
            name,
            { parseFile fullPath with
                date = parseArticleDate name
                url = url })
        |> dict

let Compute() =
    { pages = parsePages() }
