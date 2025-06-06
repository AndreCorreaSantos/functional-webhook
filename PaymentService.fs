module PaymentService 

open System.Net.Http
open System.Text
open System.Text.Json

let private client = new HttpClient()

let confirmTransaction (payment: Types.Payment) =
    let json = JsonSerializer.Serialize(payment)
    let content = new StringContent(json, Encoding.UTF8, "application/json")
    client.PostAsync("http://localhost:5001/confirmar", content) |> Async.AwaitTask

let cancelTransaction (payment: Types.Payment) =
    let json = JsonSerializer.Serialize(payment)
    let content = new StringContent(json, Encoding.UTF8, "application/json")
    client.PostAsync("http://localhost:5001/cancelar", content) |> Async.AwaitTask

