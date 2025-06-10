module PaymentService 
open System.Net.Http
open System.Text
open System.Text.Json
open System.Threading.Tasks
open Types

let private client = new HttpClient()

let confirmTransaction (payment: Payment) : Task<unit> =
    task {
        let json = JsonSerializer.Serialize(payment)
        let content = new StringContent(json, Encoding.UTF8, "application/json")
        let! _ = client.PostAsync("http://localhost:5001/confirmar", content)
        return ()
    }

let cancelTransaction (payment: Payment) : Task<unit> =
    task {
        let json = JsonSerializer.Serialize(payment)
        let content = new StringContent(json, Encoding.UTF8, "application/json")
        let! _ = client.PostAsync("http://localhost:5001/cancelar", content)
        return ()
    }