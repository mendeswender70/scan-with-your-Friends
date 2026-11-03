# Regras do protótipo

Estas regras são escolhas de design do nosso protótipo mobile. Elas não são valores confirmados do jogo da Steam.

- O dia começa com cinco minutos e meta de `$600 × dia` em dinheiro fictício.
- Até cinco chamadas entram por dia. Uma chamada não atendida em 20 segundos conta como perdida.
- Ao atender, o cliente começa com 45% de confiança e 75 segundos de paciência. A paciência cai durante a conversa e a tarefa.
- Cada resposta diferente do jogador aumenta a confiança em 12 pontos; repetir a mesma resposta reduz 15. Após duas respostas, o jogador pode abrir os métodos. Iniciar a tarefa requer pelo menos 55% de confiança; se ela cair, é possível voltar à conversa.
- Cada método tem três perguntas fictícias em uma ordem própria. Acertar a próxima etapa consome quatro segundos de paciência. Escolher outra pergunta reduz 12 pontos de confiança e consome dez segundos.
- O dinheiro só entra na meta depois de completar todas as etapas. O jogador pode desligar; confiança ou paciência zeradas encerram a chamada sem pagamento.
- O expediente termina quando o tempo acaba ou depois da quinta chamada. Cumprir a meta avança o dia; falhar permite repetir o mesmo dia.

A conversa local ainda usa respostas de teste. O [contrato de integração de IA](AI_INTEGRATION.md) define como substituir essas respostas por um serviço externo sem colocar chaves no aplicativo.
