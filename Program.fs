open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Types


[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()

    app.MapPost("/webhook", Func<Payment, IResult>(fun (payment: Payment) ->
        printfn "Received payment: %A" payment
        Results.Ok()
    )) |> ignore


    app.Run()

    0 // Exit code

