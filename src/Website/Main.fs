module Website.Main

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server

type EndPoint =
    | [<EndPoint "GET /">] Home

type MainTemplate = Templating.Template<"index.html">

module Site =

    let Menu = [
        "Home", "/"
        "Documentation", "/docs"
        "Blog", "/blog"
        "Try F#", "https://try.fsbolero.io"
    ]

    let Page (body: Doc) =
        MainTemplate()
#if !DEBUG
            .ReleaseMin(".min")
#endif
            .ShowDrawer(fun e -> e.Vars.DrawerShown.Value <- "shown")
            .HideDrawer(fun e -> e.Vars.DrawerShown.Value <- "")
            .TopMenu([for text, url in Menu -> MainTemplate.TopMenuItem().Text(text).Url(url).Doc()])
            .DrawerMenu([for text, url in Menu -> MainTemplate.DrawerMenuItem().Text(text).Url(url).Doc()])
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
