open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open System.Text.Json
open Types
open Storage
open PaymentValidation
open PaymentService

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    let app = builder.Build()

    CreateDB()
    
    app.MapPost("/webhook", Func<HttpRequest, Task<IResult>>(fun request ->
        task {
            let token =
                if request.Headers.ContainsKey("X-Webhook-Token") then
                    request.Headers["X-Webhook-Token"].ToString()
                else ""
            
            // se token invalido ignora a transacao
            if not (isValidToken token) then
                // ignora
                return Results.StatusCode(204)
            else
                use reader = new System.IO.StreamReader(request.Body)
                let! body = reader.ReadToEndAsync()
                let paymentOpt =
                    JsonSerializer.Deserialize<Payment>(body, JsonSerializerOptions(PropertyNameCaseInsensitive = true))
                    |> Option.ofObj
                
                match paymentOpt with
                | None ->
                    // payload invalido cancela transacao
                    do! cancelTransaction { event = ""; transaction_id = ""; amount = ""; currency = ""; timestamp = "" }
                    return Results.StatusCode(204)
                | Some payment ->
                    if not (isPayloadValid payment) then
                        // informacoes faltantes ou erradas cancela transacao
                        do! cancelTransaction payment
                        return Results.StatusCode(204)
                    elif not (isTransactionUnique payment.transaction_id) then
                        // transacao duplicada cancela
                        do! cancelTransaction payment
                        return Results.StatusCode(204)
                    else
                        // transacao ok confirma e retorna 200
                        do! confirmTransaction payment
                        do! storeSuccessfulPayment payment
                        return Results.Ok("Pagamento confirmado")
        })
    )   |> ignore
    
    app.Run()
    0