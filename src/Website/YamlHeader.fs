module Website.Yaml

open System
open System.IO
open System.Text.RegularExpressions
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open Newtonsoft.Json

let OfYaml<'T> (yaml: string) =
    eprintfn "DEBUG/YAMLsrc=%s" yaml
    let json =
        if String.IsNullOrWhiteSpace yaml then "{}" else
        let meta = new StringReader(yaml)
        let deserializer =
            (new DeserializerBuilder())
                .WithNamingConvention(PascalCaseNamingConvention())
                .Build()
        let serializer = (new SerializerBuilder()).JsonCompatible().Build()
        let yaml = deserializer.Deserialize(meta)
        eprintfn "DEBUG/YAML=%A" yaml
        serializer.Serialize(yaml)
    JsonConvert.DeserializeObject<'T>(json)

let private delimRE = Regex("^---\\w*\r?$", RegexOptions.Compiled ||| RegexOptions.Multiline)

/// Split file contents into a YAML header and the rest of the file.
let SplitHeader (source: string) =
    let searchFrom = if source.StartsWith("---") then 3 else 0
    let m = delimRE.Match(source, searchFrom)
    if m.Success then
        source.[searchFrom..m.Index-1], source.[m.Index + m.Length..]
    else
        "", source
