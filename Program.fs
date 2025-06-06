open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open System.Text.Json
open Types
open PaymentValidation
open PaymentService

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()
    app.Urls.Add("http://localhost:5102")

    app.MapPost("/webhook", Func<HttpRequest, IResult>(fun request ->
        let token =
            if request.Headers.ContainsKey("X-Webhook-Token") then
                request.Headers["X-Webhook-Token"].ToString()
            else ""

        if not (isValidToken token) then
            Results.Unauthorized()
        else
            let result =
                async {
                    try
                        use reader = new System.IO.StreamReader(request.Body)
                        let! body = reader.ReadToEndAsync() |> Async.AwaitTask

                        let paymentOpt =
                            JsonSerializer.Deserialize<Payment>(body, JsonSerializerOptions(PropertyNameCaseInsensitive = true))
                            |> Option.ofObj

                        match paymentOpt with
                        | None ->
                            return Results.BadRequest()
                        | Some payment ->
                            if not (isPayloadValid payment) then
                                let! _ = cancelTransaction payment
                                return Results.BadRequest()
                            elif not (isTransactionUnique payment.transaction_id) then
                                return Results.StatusCode(409)
                            else
                                let! _ = confirmTransaction payment
                                return Results.Ok("Pagamento confirmado")
                    with ex ->
                        printfn "Erro interno: %s" ex.Message
                        return Results.StatusCode(500)
                }
                |> Async.RunSynchronously

            result
    )) |> ignore

    app.Run()
    0
