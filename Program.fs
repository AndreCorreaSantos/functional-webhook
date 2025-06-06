open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open System.Text.Json
open Types
open PaymentValidation
open PaymentService
open System.Threading.Tasks

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()
    app.Urls.Add("http://localhost:5102")

    app.MapPost("/webhook", Func<HttpRequest, Task<IResult>>(fun request ->
        task {
            try
                // read the token from the header
                let token =
                    if request.Headers.ContainsKey("X-Webhook-Token") then
                        request.Headers["X-Webhook-Token"].ToString()
                    else ""

                if not (isValidToken token) then
                    printfn "Token inválido"
                    return Results.Unauthorized()

                // read request body
                use reader = new System.IO.StreamReader(request.Body)
                let! body = reader.ReadToEndAsync()

                // json to payment
                let paymentOpt =
                    JsonSerializer.Deserialize<Payment>(body, JsonSerializerOptions(PropertyNameCaseInsensitive = true))
                    |> Option.ofObj

                match paymentOpt with
                | None ->
                    return Results.BadRequest()
                | Some payment ->
                    // validate payload
                    if not (isPayloadValid payment) then
                        let! _ = cancelTransaction payment
                        return Results.BadRequest()

                    // check if transaction is unique
                    if not (isTransactionUnique payment.transaction_id) then
                        return Results.StatusCode(409)

                    // confirm transaction
                    let! _ = confirmTransaction payment
                    return Results.Ok("Pagamento confirmado")
            with ex ->
                printfn "Erro: %s" ex.Message
                // handle error
                return Results.StatusCode(500)
        }
    )) |> ignore

    app.Run()
    0