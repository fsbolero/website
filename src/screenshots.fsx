// This script is used to create the screenshots on the home page.
// Screenshots made with VS Code.
// * theme: Light+
// * font: Consolas 20px
// * Editor options:
//   * Code lens: disabled
// * F# options:
//   * Line Lens: never
//   * Linter: disabled
//   * Inlay Hints: disabled
#load "../.paket/load/net6.0/refdoc/Bolero.fsx"
#r "netstandard"
#r "Facades/netstandard"
#r "../packages/refdoc/Bolero/lib/net6.0/Bolero.dll"

open Bolero
open Bolero.Html
open Elmish


module FSharpCode =
    //---------------

    let loginForm =
        form {
            attr.id "login-form"
            input { attr.placeholder "First name" }
            input { attr.placeholder "Last name" }
            button {
                on.click (fun _ -> printfn "Welcome!")
                "Log in"
            }
        }

    //---------------

module Elmish =
    //---------------

    type Model = { name: string; age: int }

    type Message =
        | SetName of string

    let update model message =
        match message with
        | SetName n -> { model with name = n }

    //---------------

module Templating =
    type Model2 = { name: string }
    type Message2 = LogOut

    //---------------

    // F#:
    type Form = Template<"Form.html">
    let form model dispatch =
        Form().name(model.name)
            .logOut(fun _ -> dispatch LogOut)
            .Elt()

    //---------------
        model, dispatch

module Remoting =
    type Model = { text: string }
    type Message =
        | Greet of string
        | GotGreeting of string
        | Error of exn
    type MyRemoteFunc = { getGreeting : string -> Async<string> }

    //---------------

    // Server
    let service =
        { getGreeting = fun who ->
            async { return "Hello, " + who + "!" } }

    // Client
    let update service model message =
        match message with
        | Greet who ->
            model,
            Cmd.OfAsync.either service.getGreeting who GotGreeting Error
        | GotGreeting msg ->
            { model with text = msg }, Cmd.none
        | Error _ -> model, Cmd.none

    //---------------

module Routing =
    //---------------

    type Route =
        | [<EndPoint "/">] Home
        | [<EndPoint "/article/{id}">] Article of id: int

    type Model = { route: Route }

    type Message =
        | SetRoute of Route

    let router = Router.infer SetRoute (fun m -> m.route)

    //---------------


// Reference values from the above modules
// so that they're not colored as unused
module DontIgnoreStuff =

    let x = FSharpCode.loginForm
    let y = Routing.router
    let z = Remoting.service
