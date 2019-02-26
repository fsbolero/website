module Website.Docs

open System
open System.IO
open Markdig
open Website

type [<CLIMutable>] Document =
    {
        title: string option
        content: string
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

    let parseFile fullPath =
        let header, content =
            fullPath
            |> File.ReadAllText
            |> Yaml.SplitHeader
        let content = Markdown.ToHtml(content, pipeline)
        { Yaml.OfYaml header with content = content }

let Pages =
    try
    Directory.GetFiles(baseDir, "*.md", SearchOption.AllDirectories)
    |> Array.filter (fun fullPath ->
        let filename = Path.GetFileNameWithoutExtension(fullPath)
        not (filename.StartsWith("_"))
    )
    |> Array.map (fun fullPath ->
        let path = fullPath.[baseDir.Length..].Replace('\\', '/').Trim('/')
        eprintfn "Parsing %s" path
        let path = path.[..path.Length - 4] // Trim .md
        path, parseFile fullPath)
    |> dict
    with exn ->
    eprintfn "### %A" exn
    reraise()

let Sidebar =
    let doc = parseFile (baseDir </> "_Sidebar.md")
    doc.content
