### To install dependencies:

```bash
dotnet build
```

### To run webhook:

```bash
./run.sh
```

### How the Webhook Works

When started, the service deletes any previous `payments.db` file and creates a new SQLite database with a payments table. It exposes a POST endpoint at `/webhook` and works as follows:

#### Token Validation
The token is extracted from the `X-Webhook-Token` header.

- If the token is missing or incorrect, the transaction is ignored and the server responds with **204 No Content**.
- No confirmation or cancellation occurs for invalid tokens.

#### Payload Deserialization and Validation
The request body is read and deserialized into a `Payment` object.

- If deserialization fails (e.g., invalid JSON), the transaction is canceled by sending a POST to `/cancelar` with an empty payload.
- If required fields are missing or incorrect (except for `transaction_id`, which is optional), the transaction is also canceled via POST to `/cancelar`.

#### Transaction Uniqueness Check
The service checks if a payment with the same `transaction_id` already exists in the database.

- If found, the transaction is considered a duplicate and is canceled via POST to `/cancelar`.

#### Transaction Confirmation
If the token is valid, the payload is complete and correct, and the transaction is unique:

- The service sends a POST request to `/confirmar` with the payment data.
- The transaction is saved to the SQLite database.
- The service responds with **200 OK** and the message `"Pagamento confirmado"`.

#### Database
All successful transactions are persisted to the `payments.db` file using Dapper.
The database is reset at every service startup to ensure a clean environment.



### Checklist:

- [X] O serviço deve verificar a integridade do payload
  - O servidor verifica se todos os campos obrigatórios estão presentes no payload recebido (exceto transaction_id que é opcional).
- [X] O serviço deve implementar algum mecanismo de veracidade da transação
  - Comparo o token enviado com o token armazenado para verificar a autenticidade da transação.
- [X] O serviço deve cancelar a transação em caso de divergência
  - Função `cancelTransaction` é chamada quando alguma informação estiver errada (ex: valor incorreto) ou alguma informação obrigatória estiver faltante (exceto transaction_id). A função manda POST para o endpoint `/cancelar` com o payload da transação.
- [X] O serviço deve tratar divergências conforme especificado
  - Token inválido/errado: Ignora a transação (não responde, não cancela) - transação considerada falsa
  - Informações erradas: Cancela a transação via POST `/cancelar`
  - Informações faltantes (exceto transaction_id): Cancela a transação via POST `/cancelar`
  - Nenhum código de erro HTTP é retornado para transações problemáticas
- [X] O serviço deve confirmar a transação em caso de sucesso
  - Função `confirmTransaction` é chamada quando todos os campos obrigatórios estão presentes, token é válido (igual ao token armazenado) e todas as informações estão corretas. A função retorna 200 OK e manda POST para o endpoint `/confirmar` com o payload da transação.
- [X] O serviço deve persistir a transação em um BD
  - As transações de sucesso são persistidas em um banco de dados SQLite usando DAPPER.
- [X] Implementar um serviço HTTPS
  - O servidor escuta HTTP na porta 5101 e HTTPS na porta 5102.
