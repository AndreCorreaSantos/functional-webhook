module Types
type Payment = {
    event : string
    transaction_id : string
    amount : decimal
    currency : string
    timestamp : string
}