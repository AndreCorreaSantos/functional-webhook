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
    
    app.MapPost("/webhook", Func<HttpRequest, IResult>(fun request ->
        let token =
            if request.Headers.ContainsKey("X-Webhook-Token") then
                request.Headers["X-Webhook-Token"].ToString()
            else ""
        
        // se token invalido ignora a transacao
        if not (isValidToken token) then
            () 
        else
            use reader = new System.IO.StreamReader(request.Body)
            let body = reader.ReadToEndAsync().Result
            let paymentOpt =
                JsonSerializer.Deserialize<Payment>(body, JsonSerializerOptions(PropertyNameCaseInsensitive = true))
                |> Option.ofObj
            
            match paymentOpt with
            | None ->
                // payload invalido cancela transacao
                cancelTransaction { event = ""; transaction_id = ""; amount = ""; currency = ""; timestamp = "" } |> ignore
                Results.StatusCode(204)
            | Some payment ->
                if not (isPayloadValid payment) then
                    // informacoes faltantes ou erradas cancela transacao
                    cancelTransaction payment |> ignore
                    Results.StatusCode(204)
                elif not (isTransactionUnique payment.transaction_id) then
                    // transacao duplicada cancela
                    cancelTransaction payment |> ignore
                    Results.StatusCode(204)
                else
                    // transacao ok confirma e retorna 200
                    confirmTransaction payment |> ignore
                    Results.Ok("Pagamento confirmado")
    )) |> ignore
    
    app.Run()
    0