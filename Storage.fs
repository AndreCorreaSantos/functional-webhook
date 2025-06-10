module Storage

open System.Threading.Tasks
open Types
open System
open System.IO
open Dapper
open Microsoft.Data.Sqlite

let dbFilePath = "payments.db"
let connectionString = $"Data Source={dbFilePath};"

let CreateDB () =
    if File.Exists(dbFilePath) then
        // deletando DB antiga para garantir os testes
        File.Delete(dbFilePath) 

    use conn = new SqliteConnection(connectionString)
    conn.Open()

    let createTableSql = """
        CREATE TABLE payments (
            transaction_id TEXT PRIMARY KEY,
            event TEXT NOT NULL,
            amount TEXT NOT NULL,
            currency TEXT NOT NULL,
            timestamp TEXT NOT NULL
        );
    """
    conn.Execute(createTableSql) |> ignore



let storeSuccessfulPayment (payment: Types.Payment): Task<unit> = task {
    use conn = new SqliteConnection(connectionString)
    do! conn.OpenAsync()
    let sql = """
        INSERT INTO payments (transaction_id, event, amount, currency, timestamp)
        VALUES (@transaction_id, @event, @amount, @currency, @timestamp)
        ON CONFLICT(transaction_id) DO NOTHING;
    """
    let! _ = conn.ExecuteAsync(sql, payment)
    return ()
}

let isTransactionUniqueById (transaction_id: string): Task<bool> = task {
    use conn = new SqliteConnection(connectionString)
    do! conn.OpenAsync()
    let sql = """
        SELECT COUNT(1)
        FROM payments
        WHERE transaction_id = @transaction_id;
    """
    let! count =
        conn.ExecuteScalarAsync<int>(sql, dict [ "transaction_id", box transaction_id ])
    return count = 0
}

