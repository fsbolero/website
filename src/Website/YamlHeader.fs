module Website.Yaml

open System
open System.Text.RegularExpressions
open YamlDotNet.Serialization

let OfYaml<'T> (yaml: string) =
    let deserializer =
        (new DeserializerBuilder())
            .Build()
    if String.IsNullOrWhiteSpace yaml then
        deserializer.Deserialize<'T>("{}")
    else
    let yaml = deserializer.Deserialize<'T>(yaml)
    eprintfn "DEBUG/YAML=%A" yaml
    yaml

let private delimRE = Regex("^---\\w*\r?$", RegexOptions.Compiled ||| RegexOptions.Multiline)

/// Split file contents into a YAML header and the rest of the file.
let SplitHeader (source: string) =
    let searchFrom = if source.StartsWith("---") then 3 else 0
    let m = delimRE.Match(source, searchFrom)
    if m.Success then
        source.[searchFrom..m.Index-1], source.[m.Index + m.Length..]
    else
        "", source
