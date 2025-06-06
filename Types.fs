module Types
type Payment = {
    event: string
    transaction_id: string
    amount: float
    currency: string
    timestamp: string
}
