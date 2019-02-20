module Website.Docs

open System.IO
open Markdig

let private pipeline =
    MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build()

let private (</>) x y = Path.Combine(x, y)

let private baseDir = __SOURCE_DIRECTORY__ </> "docs"

let private parseFile fullPath =
    Markdown.ToHtml(File.ReadAllText(fullPath), pipeline)

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
