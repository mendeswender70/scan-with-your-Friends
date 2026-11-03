# Conversa com IA

O jogo continua com respostas locais enquanto não houver um serviço de IA configurado. A estrutura para conectar esse serviço já está no cliente Unity: no componente `CallCenterPrototype`, o campo `Dialogue Server Url` aceita a URL de um servidor que receba `POST /dialogue`.

Corpo JSON enviado pelo jogo:

```json
{
  "callerName": "Marta, professora",
  "problem": "Problema inicial do personagem",
  "history": "Você: ...\nPessoa: ...\n",
  "playerMessage": "Mensagem digitada pelo jogador"
}
```

Resposta esperada: `{"reply":"Resposta curta do personagem"}`. Se o servidor falhar, a interface informa o erro e usa a resposta local para que a partida continue. Respostas atrasadas de uma chamada anterior são ignoradas.

O serviço precisa manter a chave do provedor **somente no servidor**; ela não deve entrar no Unity, no repositório ou no aplicativo mobile. Para builds Android e iOS, o serviço precisa estar disponível por HTTPS. O servidor também deve limitar tamanho das mensagens, custo, taxa de requisições e conteúdo, e jamais pedir dados bancários reais. A lógica de dinheiro e da meta continua no jogo, fora do controle do modelo.

Ainda falta escolher um provedor, obter a credencial por um canal seguro, implementar o servidor e testar uma conversa real. O campo da URL fica vazio por padrão para não sugerir que a IA já funciona.
