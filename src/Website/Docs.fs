module Website.Docs

open System.IO
open Markdig
open Markdig.Extensions.AutoIdentifiers

let private pipeline =
    MarkdownPipelineBuilder()
        .UseAutoIdentifiers(AutoIdentifierOptions.GitHub)
        .Build()

let Pages =
    let baseDir = Path.Combine(__SOURCE_DIRECTORY__, "docs")
    Directory.GetFiles(baseDir, "*.md", SearchOption.AllDirectories)
    |> Array.filter (fun fullPath ->
        let filename = Path.GetFileNameWithoutExtension(fullPath)
        not (filename.StartsWith("_"))
    )
    |> Array.map (fun fullPath ->
        let path = fullPath.[baseDir.Length..].Replace('\\', '/').Trim('/')
        let path = path.[..path.Length - 4] // Trim .md
        let path = if path = "Home" then "index" else path + "/index"
        let content = Markdown.ToHtml(File.ReadAllText(fullPath), pipeline)
        path, content)
    |> dict
