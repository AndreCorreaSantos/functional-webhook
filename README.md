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

  - O servidor verifica se todos os campos obrigatórios estão presentes no payload recebido, caso contrário, retorna um erro 400 Bad Request com mensagem "Payload inválido".
- [X] O serviço deve implementar algum mecanismo de veracidade da transação

  - Comparo o token enviado com o token armazenado --> caso iguais, confirmo a transação
- [X] O serviço deve cancelar a transação em caso de divergência

  - função `cancelTransaction` é chamada quando os tokens divergem. A função manda post para o endpoint `/cancelar` com o payload da transação.
- [X] O serviço deve retornar um erro em caso de divergência

  - Caso o token enviado não seja igual ao token armazenado, o serviço retorna um erro unauthorized (401) com a mensagem "Token inválido" e cancela a transação.     Caso algum campo obrigatório esteja faltando, o se viço retorna um erro Bad Request (400) com a mensagem "Payload inválido".
- [X] O serviço deve confirmar a transação em caso de sucesso

  - função `confirmTransaction` é chamada quando os tokens são iguais. A função manda post para o endpoint `/confirmar` com o payload da transação.
- [ ] O serviço deve persistir a transação em um BD

- [X] Implementar um serviço HTTPS

  - O servidor escuta HTTP na porta 5101 e HTTPS na porta 5102
