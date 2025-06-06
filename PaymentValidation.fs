module PaymentValidation

open Types
open System

let isValidToken (token: string) =
    token = "meu-token-secreto"  // Token usado nos testes Python

/// Armazena as transações únicas já vistas
let seen = System.Collections.Concurrent.ConcurrentDictionary<string, bool>()

/// Verifica se a transação ainda não foi processada
let isTransactionUnique (transactionId: string) =
    seen.TryAdd(transactionId, true)

/// Valida os campos do pagamento
let isPayloadValid (payment: Payment) =
    not (String.IsNullOrWhiteSpace payment.transaction_id) &&
    payment.amount > 0.0 &&
    payment.currency = "BRL" &&
    payment.event = "payment_success" &&
    not (String.IsNullOrWhiteSpace payment.timestamp)
