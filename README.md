### To install dependencies:

```bash
dotnet build
```

### To run webhook:

```bash
./run.sh
```

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
