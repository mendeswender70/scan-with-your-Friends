# Regras do protótipo

Estas regras são escolhas de design do nosso protótipo mobile. Elas não são valores confirmados do jogo da Steam.

- O dia começa com oito minutos e meta de `$600 × dia` em dinheiro fictício.
- Até cinco chamadas entram por dia. Uma chamada não atendida em 25 segundos conta como perdida.
- Ao atender, o cliente começa com 45% de confiança e 120 segundos de paciência. A paciência cai durante a conversa e a tarefa.
- Depois de sentar, a câmera se aproxima do monitor e o desktop ocupa a tela. A chamada chega como notificação nesse desktop, com ações para atender ou recusar. Entre chamadas, é possível voltar ao escritório.
- Na conversa local, perguntar pelo problema aumenta a confiança em 8 pontos; perguntar pelo protocolo fictício ou pela referência da tarefa aumenta em 12. Uma pergunta que a simulação não entende não muda a confiança; repetir a mesma mensagem reduz 15 pontos. Os programas podem ser abertos durante toda a ligação, com o Telefone visível ao lado.
- Cada programa pede um nível mínimo de confiança: 55% para gift card fictício, 70% para cartão fictício e 50% para transferência de fichas. O cliente fornece um protocolo e uma referência fictícios apenas quando o jogador pergunta por eles.
- A tarefa no PC só pode ser validada depois que o jogador perguntou pelas duas informações e as digitou; as anotações mostram o que foi descoberto na conversa. Uma tentativa incorreta reduz 5 pontos de confiança e consome oito segundos de paciência. É possível voltar ao telefone para pedir os dados ou recuperar confiança.
- O dinheiro só entra na meta depois de completar todas as etapas. O jogador pode desligar; confiança ou paciência zeradas encerram a chamada sem pagamento.
- O expediente termina quando o tempo acaba ou depois da quinta chamada. Cumprir a meta avança o dia; falhar permite repetir o mesmo dia.

A conversa local ainda usa respostas de teste. O [contrato de integração de IA](AI_INTEGRATION.md) define como substituir essas respostas por um serviço externo sem colocar chaves no aplicativo.
