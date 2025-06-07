module PaymentValidation

open System
open Types

let isValidToken (token: string) =
    token = "meu-token-secreto"

let seen = System.Collections.Concurrent.ConcurrentDictionary<string, bool>()
let isTransactionUnique id = seen.TryAdd(id, true)

let private tryParseAmount (s: string) =
    match Decimal.TryParse(s) with
    | true, v when v > 0.0M -> Some v
    | _ -> None

let isPayloadValid (payment: Payment) =
    not (String.IsNullOrWhiteSpace payment.transaction_id)
    && tryParseAmount payment.amount |> Option.isSome
    && payment.currency = "BRL"
    && payment.event = "payment_success"
    && not (String.IsNullOrWhiteSpace payment.timestamp)
