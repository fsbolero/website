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
        title: option<string>
        content: string
    }

/// Markdig can ignore the frontmatter, but it doesn't actually parse its contents
/// so we have to do it ourselves :(
let readMetadata (rawText: string) doc =
    let parseLine doc (line: string) =
        let line = line.Trim()
        match line.IndexOf(':') with
        | -1 -> doc
        | i ->
            let value = line.[i + 1..].Trim()
            match line.[..i - 1].Trim() with
            | "title" -> { doc with title = Some value }
            | _ -> doc
    if not (rawText.StartsWith("---")) then doc else
    match rawText.IndexOf("---", 4) with
    | -1 -> doc
    | i ->
        let lines = rawText.[4..i-1].Split([|'\n'|], StringSplitOptions.RemoveEmptyEntries)
        Array.fold parseLine doc lines

let private parseFile fullPath =
    let rawText = File.ReadAllText(fullPath)
    {
        title = None
        content = Markdown.ToHtml(rawText, pipeline)
    }
    |> readMetadata rawText

let Pages =
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

let Sidebar =
    let doc = parseFile (baseDir </> "_Sidebar.md")
    doc.content
