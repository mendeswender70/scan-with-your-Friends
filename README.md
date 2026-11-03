# Scan With Your Friends

Protótipo original de jogo mobile sobre um call center fictício. Esta primeira versão é local e individual: o jogador inicia o turno, senta no computador, atende chamadas, conversa por texto, descobre informações fictícias com o cliente e as registra em um programa do PC para alcançar uma meta em dinheiro fictício. O escritório tem várias mesas, colegas e objetos, com uma câmera que muda ao sentar. A conversa ainda é interpretada por regras locais; não há IA conectada, multiplayer, pagamentos reais ou coleta de dados financeiros.

## Abrir e jogar

1. Abra esta pasta no Unity 6.3 LTS.
2. Aguarde a importação e escolha **Tools > Scan With Your Friends > Create Prototype Scene**.
3. Abra `Assets/Scenes/Prototype.unity`, se necessário. Na aba **Game**, selecione a proporção **9:16 (Portrait)** ou crie uma resolução 1080 × 1920; então pressione **Play**.

Para testar o ciclo da primeira chamada, inicie o dia, sente-se e atenda. Na conversa, experimente perguntar "o que aconteceu?", "qual é o protocolo?" e "qual é a referência?". Depois abra um programa do PC, use as anotações para preencher os campos fictícios e valide a tarefa.

A cena é gerada pelo editor a partir dos scripts. Para um build Android, instale o módulo Android Build Support no Unity Hub e selecione Android em **File > Build Profiles**. Para iOS, abra o mesmo projeto no macOS com o módulo iOS Build Support e Xcode instalados. O protótipo ainda não implementa multiplayer nem chamadas com IA.

As mecânicas confirmadas, as observações do jogador e as escolhas próprias deste protótipo estão separadas em [Referências de jogabilidade](docs/GAMEPLAY_RESEARCH.md).

A estrutura para ligar conversas livres a um servidor está descrita em [Conversa com IA](docs/AI_INTEGRATION.md). Ela permanece desligada até existir um provedor e um servidor configurado.

As regras jogáveis atuais de tempo, chamadas, confiança, perguntas e meta estão em [Regras do protótipo](docs/PROTOTYPE_RULES.md). Esses números são provisórios e podem mudar após os testes no celular.
