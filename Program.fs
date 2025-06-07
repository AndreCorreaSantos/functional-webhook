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
            use reader = new System.IO.StreamReader(request.Body)
            let body = reader.ReadToEnd()

            let paymentOpt =
                JsonSerializer.Deserialize<Payment>(body, JsonSerializerOptions(PropertyNameCaseInsensitive = true))
                |> Option.ofObj

            match paymentOpt with
            | None ->
                Results.BadRequest()
            | Some payment ->
                if not (isPayloadValid payment) then
                    cancelTransaction payment |> ignore
                    Results.BadRequest()
                elif not (isTransactionUnique payment.transaction_id) then
                    Results.StatusCode(409)
                else
                    confirmTransaction payment |> ignore
                    Results.Ok("Pagamento confirmado")
    )) |> ignore

    app.Run()
    0
