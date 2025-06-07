module PaymentService 

open System.Net.Http
open System.Text
open System.Text.Json
open Types

let private client = new HttpClient()

let confirmTransaction (payment: Payment) =
    let json = JsonSerializer.Serialize(payment)
    let content = new StringContent(json, Encoding.UTF8, "application/json")
    client.PostAsync("http://localhost:5001/confirmar", content).Result |> ignore

let cancelTransaction (payment: Payment) =
    let json = JsonSerializer.Serialize(payment)
    let content = new StringContent(json, Encoding.UTF8, "application/json")
    client.PostAsync("http://localhost:5001/cancelar", content).Result |> ignore
