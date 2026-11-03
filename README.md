# Scan With Your Friends

Protótipo original de jogo mobile sobre um call center fictício. Esta primeira versão é local e individual: o jogador inicia o turno, senta à mesa, entra no desktop do computador, recebe uma chamada com opções para atender ou recusar, conversa por texto e usa programas do PC sem perder a ligação de vista. O escritório ainda é feito de formas simples. A conversa é interpretada por regras locais; não há IA conectada, multiplayer, pagamentos reais ou coleta de dados financeiros.

## Abrir e jogar

1. Abra esta pasta no Unity 6.3 LTS.
2. Aguarde a importação e escolha **Tools > Scan With Your Friends > Create Prototype Scene**.
3. Abra `Assets/Scenes/Prototype.unity`, se necessário. Na aba **Game**, selecione **16:9 (Landscape)** ou crie uma resolução 1600 × 900; então pressione **Play**. No Android e no iOS, o protótipo pede orientação paisagem.

Para testar o ciclo da primeira chamada, inicie o dia e toque em **Sentar no PC**. Após a aproximação da câmera, o desktop ocupa a tela e a chamada aparece como notificação. Atenda ou recuse. Se atender, o Telefone permanece aberto à esquerda enquanto você usa as abas **Métodos**, **Tarefa** e **Anotações** à direita. Na conversa local, experimente perguntar "o que aconteceu?", "qual é o protocolo?", "qual é a referência?" e "qual é a cor do selo?". Os três programas agora exigem combinações e ações diferentes com essas pistas fictícias. Entre chamadas, **Sair do PC** leva de volta ao escritório.

A cena é gerada pelo editor a partir dos scripts. Para um build Android, instale o módulo Android Build Support no Unity Hub e selecione Android em **File > Build Profiles**. Para iOS, abra o mesmo projeto no macOS com o módulo iOS Build Support e Xcode instalados. O protótipo ainda não implementa multiplayer nem chamadas com IA.

O [documento de design](docs/GAME_DESIGN_DOCUMENT.md) reúne a pesquisa do jogo de referência, o fluxo de escritório para desktop e a direção da versão mobile. As evidências confirmadas, as observações do jogador e as escolhas próprias deste protótipo também estão separadas em [Referências de jogabilidade](docs/GAMEPLAY_RESEARCH.md).

A estrutura para ligar conversas livres a um servidor está descrita em [Conversa com IA](docs/AI_INTEGRATION.md). Ela permanece desligada até existir um provedor e um servidor configurado.

As regras jogáveis atuais de tempo, chamadas, confiança, perguntas e meta estão em [Regras do protótipo](docs/PROTOTYPE_RULES.md). Esses números são provisórios e podem mudar após os testes no celular.
