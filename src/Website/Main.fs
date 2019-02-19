module Website.Main

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server

type EndPoint =
    | [<EndPoint "GET /">] Home

type MainTemplate = Templating.Template<"index.html">

module Site =

    let Page (body: Doc) =
        MainTemplate()
            .ShowDrawer(fun e -> e.Vars.DrawerShown.Value <- "shown")
            .HideDrawer(fun e -> e.Vars.DrawerShown.Value <- "")
            .Body(body)
            .Doc()
        |> Content.Page

    let HomePage () =
        MainTemplate.HomeBody()
            .Doc()
        |> Page

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx action ->
            match action with
            | Home -> HomePage ()
        )

[<Sealed>]
type Website() =
    interface IWebsite<EndPoint> with
        member this.Sitelet = Site.Main
        member this.Actions = [Home]

[<assembly: Website(typeof<Website>)>]
do ()
