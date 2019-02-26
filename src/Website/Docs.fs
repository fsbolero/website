module Website.Docs

open System
open System.IO
open Markdig

let private pipeline =
    MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseYamlFrontMatter()
        .Build()

let private (</>) x y = Path.Combine(x, y)

let private baseDir = __SOURCE_DIRECTORY__ </> "docs"

type Document =
    {
        title: string
        content: string
    }

let private parseFile fullPath =
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
        let path = path.[..path.Length - 4] // Trim .md
        path, parseFile fullPath)
    |> dict
  with exn ->
    eprintfn "%A" exn
    reraise()

let Sidebar =
    let doc = parseFile (baseDir </> "_Sidebar.md")
    doc.content
