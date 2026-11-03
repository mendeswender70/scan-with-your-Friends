# Scan With Your Friends — documento de design do jogo

**Versão:** 0.1, pesquisa e direção de produto — 3 de novembro de 2026.

**Plataformas planejadas:** Android e iOS, em Unity 6.
**Estado:** proposta para orientar a próxima implementação; números, telas e conteúdo ainda precisam de teste com jogadores.

## 1. Premissa e limite da referência

Queremos criar um jogo **original** de comédia e improvisação sobre um expediente em um call center fictício. O jogador entra no escritório, senta-se à sua mesa, passa a usar o computador *dentro do jogo*, recebe chamadas, conversa com personagens e opera aplicativos do desktop enquanto tenta atingir a meta do turno. A graça deve vir das personalidades, das respostas inesperadas e da pressão de trabalhar em várias janelas, não de uma lista fixa de falas corretas.

*Scam With Your Friends*, de JakeHub e Metater, é a principal referência de **estrutura e ritmo**, não um material a ser reproduzido. Personagens, arte, nomes de aplicativos, textos, sons, layout e cenários do nosso jogo serão próprios. Todo dinheiro e toda informação de atendimento serão fictícios. O jogo nunca solicitará dados financeiros reais.

## 2. O que a pesquisa confirma

As fontes oficiais são a [página da Steam][steam], o [site dos desenvolvedores][site] e as capturas da própria página. A tabela separa fatos observáveis de interpretações e escolhas nossas.

| Aspecto | Evidência no jogo de referência | Grau de certeza |
| --- | --- | --- |
| Premissa | Call center com chefe exigente; modo solo e cooperativo online; personagens com conversa de IA em tempo real. | Descrição oficial [Steam][steam] e [site][site]. |
| Turno | Meta diária, avaliação do chefe, avanço por dias e ampliação das atividades disponíveis. | Descrição oficial [Steam][steam] e [site][site]. |
| Escritório | Ambiente 3D com mesas, colegas e espaços de convivência; incidentes interrompem o expediente. | Descrição oficial e [captura do escritório][shot-office]. |
| Computador | Desktop dentro do jogo, com ícones, barra de tarefas e múltiplos aplicativos/janelas simultâneos. | [Captura do desktop][shot-desktop] e [captura com apps abertos][shot-apps]. |
| Chamada recebida | Janela **Incoming Call** sobre o desktop, com ações visíveis para atender ou recusar. | [Captura oficial da chamada][shot-ring]. |
| Conversa | Aplicativo de telefone com retrato/nome, histórico, campo de texto e indicador de confiança; outros apps podem permanecer abertos. | [Captura com apps abertos][shot-apps]. |
| Ferramentas e interrupções | A descrição cita acesso remoto fictício, câmera, editor de imagem, gravação de tela, vírus e outros desastres de escritório. | Descrição oficial [Steam][steam]; [captura com câmera][shot-ring]. |
| Resultado e progressão | Tela de resultado mostra ganho de equipe, ganho individual e dias trabalhados. | [Captura de resultado][shot-results]. |
| Modificação | Conteúdo modificável e personalidades customizadas são anunciados para a versão de PC. | Descrição oficial [Steam][steam]. |
| Percurso físico até a mesa | O jogador relata que entra no computador ao sentar-se; uma captura mostra personagem diante do monitor. A transição exata, controles e duração não são demonstrados em imagens estáticas. | Relato do jogador + [captura da mesa][shot-desk]. |
| Métodos e perguntas | O jogador relata opções como gift cards e cartão, cada uma com exigências próprias. Os ícones aparecem no desktop oficial, mas o catálogo completo e as regras exatas não estão documentados. | Relato do jogador + [captura do desktop][shot-desktop]. |

Não encontramos nas fontes oficiais uma especificação pública de fórmulas de confiança, paciência, valores, probabilidade de chamadas, lista completa de métodos, sequência de perguntas ou comportamento interno da IA. Guias da comunidade podem registrar experiências particulares, mas não são regras oficiais. O [remake web não oficial][fan] usa números e um fluxo próprios; não será usado como prova de balanceamento do jogo da Steam.

## 3. Objetivo da nossa experiência mobile

**Fantasia do jogador:** “Tenho um turno caótico, uma pessoa imprevisível na linha e vários programas abertos; preciso entender o caso e concluir uma tarefa antes da avaliação.”

**Pilares de design:**

1. **Conversa que reage.** O jogador digita livremente. O personagem responde ao sentido da conversa, guarda informações relevantes e pode mudar de atitude. Repetir três opções de diálogo não basta para vencer.
2. **Computador como espaço de jogo.** A chamada surge **no desktop do PC**, e o telefone continua acessível enquanto o jogador alterna entre aplicativos. A tarefa acontece nesse espaço, não em uma tela abstrata de pontuação.
3. **Turno com consequência legível.** Cada chamada consome tempo, afeta recursos e pode contribuir para uma meta. O fim do dia explica por que o jogador avançou ou falhou.
4. **Caos social em camadas.** O escritório, colegas e incidentes ampliam a experiência depois que atender e usar o PC já forem divertidos em modo solo.

O tom será de sátira ficcional. Casos cotidianos e personagens humanos reconhecíveis vêm primeiro; figuras extravagantes podem aparecer como exceções. A experiência será clara mesmo sem conhecer o jogo de referência.

## 4. Ciclo principal e estados

1. **Entrada no expediente.** O jogador vê a meta do dia e o tempo restante no escritório. Desloca-se até a própria mesa e toca em **Sentar**.
2. **Entrada no computador.** A câmera aproxima-se do monitor e a interface muda para um desktop jogável em tela cheia. O escritório continua existindo como contexto visual/sonoro; **Voltar ao escritório** devolve o controle ao personagem quando permitido.
3. **Chamada recebida.** Após um intervalo curto, surge uma notificação modal de chamada no desktop, com identificação do personagem, toque e ações **Atender** / **Recusar**. A chamada não começa automaticamente.
4. **Atendimento.** Ao atender, abre-se o app Telefone. O jogador lê a situação inicial, digita perguntas e respostas e vê o histórico e o estado da chamada. Pode minimizar o app, mas a ligação continua.
5. **Trabalho no PC.** O jogador abre o app apropriado para o caso, consulta pistas surgidas na conversa e executa uma tarefa fictícia. Pode voltar ao Telefone para esclarecer algo. A tela preserva o contexto: personagem em linha, estado da tarefa, meta e relógio.
6. **Conclusão da ligação.** O sistema avalia a tarefa e a chamada com regras determinísticas; mostra resultado e ajuste no dinheiro fictício. Em seguida vem outra chamada ou uma oportunidade de voltar ao escritório.
7. **Avaliação do dia.** Ao terminar o turno, aparece um resumo inteligível: chamadas atendidas/perdidas, tarefas concluídas, dinheiro da equipe e meta. Um novo dia oferece variação gradual.

Estados propostos: `Escritorio → SentadoNoDesktop → ChamadaTocando → EmConversa ↔ EmAplicativo → Resultado → SentadoNoDesktop → Avaliacao`. Eventos do escritório podem suspender ou alterar esse fluxo, mas entram somente após a base funcionar. Sair do desktop durante uma chamada precisa de uma regra explícita; na primeira versão, a ação fica indisponível até a ligação terminar.

**Exemplo de uma chamada original:** uma cliente relata que um pedido consta como entregue, mas ela não o recebeu. O jogador pergunta o que já verificou, conversa sobre o caso e abre um aplicativo fictício de atendimento. Ela pode fornecer um identificador inventado para a simulação. O jogador combina esse identificador com o registro do aplicativo e escolhe uma resolução dentro do jogo. Isso testa conversar, lembrar informação e alternar janelas sem ensinar ou pedir credenciais reais.

## 5. Interface e controles para Android/iOS

**Hipótese inicial: paisagem (landscape)** para comportar desktop, telefone e outro aplicativo sem texto minúsculo. O protótipo atual usa retrato; a escolha final deve ser validada em um telefone real antes de refazer todas as telas. Priorizar tamanho de fonte legível, áreas de toque amplas, respeito às áreas seguras e teclado nativo.

- **Escritório:** movimentação por toque/controle virtual simples; botão de interação junto à mesa. A câmera aproxima-se do monitor para comunicar a entrada no PC. O tempo de caminhada deve ser curto: o atendimento é a atividade principal.
- **Desktop:** barra inferior com Telefone, caso ativo e outros apps; janelas podem ser alternadas por abas ou cartões amplos. No celular, não tentar reproduzir janelas minúsculas arrastáveis de um PC. Uma faixa fixa e compacta mostra ligação ativa, confiança/estado, tempo e meta.
- **Chamada:** pop-up de entrada chama atenção sem esconder permanentemente o desktop. **Atender** e **Recusar** precisam de alvos separados. Após atender, o app Telefone fica aberto; minimizar não desliga.
- **Texto primeiro:** campo de mensagem acima do teclado do sistema, histórico rolável e indicador de resposta em andamento. Enquanto o teclado estiver aberto, meta e comando de enviar continuam visíveis. Voz e microfone entram só depois do fluxo por texto estar bom.
- **Acessibilidade:** legendas para todo áudio, opção de reduzir efeitos e animações, contraste adequado, tamanhos de texto testáveis e tempo de chamada ajustável nas opções. O toque não pode ser a única pista de uma ligação.

## 6. Sistemas de jogo propostos

### Chamadas e personagens

Cada personagem possui nome, retrato original, situação inicial, objetivos, fatos que pode revelar e estilo de fala. O sistema acompanha **estado da chamada** (tocando, ativa, encerrada), **estado da tarefa** e **confiança**. A confiança é confirmada visualmente no original; sua fórmula no nosso jogo será construída e testada, não copiada. A paciência pode entrar como dificuldade ajustável, mas números do protótipo atual são provisórios. Perder uma chamada deve ter consequência clara, sem punir demais quem ainda aprende a interface.

### Conversa por IA

Para a versão desejada, a IA gera a fala do personagem e interpreta a intenção do jogador, mas **não decide vitória, pagamento, meta ou fatos secretos**. O jogo mantém uma ficha estruturada do caso e valida a tarefa com regras próprias. A resposta deve permanecer coerente com a personalidade e com o que já foi dito; o jogador pode pedir explicações ou mudar de abordagem. Respostas locais permitem testar o fluxo sem rede, mas não substituem a conversa aberta. O serviço de IA ficará fora do aplicativo, com chave somente no servidor, limite de custo e mensagens e uma resposta de contingência quando a rede falhar. A integração atual está descrita em [Conversa com IA](AI_INTEGRATION.md); ainda não existe provedor ativo.

### Aplicativos e tarefas

O desktop terá poucos apps úteis no início: **Telefone**, **Registro do caso** e **Ferramenta da tarefa**. O Registro mostra apenas fatos aprendidos ou documentos fictícios disponibilizados na partida. A Ferramenta exige uma ação curta que se relaciona com a conversa: identificar o pedido correto, comparar dois registros ou selecionar uma resolução. Erros devem dar feedback específico e permitir voltar à ligação. Novos apps podem adicionar tipos de tarefa, mas não devem repetir o mesmo formulário com outro nome.

Métodos de ganho no jogo original, citados pelo jogador, serão tratados como **inspiração temática**, sem procedimentos reais. No nosso jogo, recursos e valores usarão fichas e identificadores inventados; não haverá campos para número de cartão, código de gift card válido, senha, documento, acesso remoto real ou contato com pessoas reais. O design final de cada tarefa precisa ser divertido mesmo com símbolos inteiramente fictícios.

### Meta, turnos e progressão

Cada turno tem duração, meta fictícia e conjunto de chamadas. O progresso soma resultados individuais/equipe e termina com avaliação. Novos dias podem adicionar variações de casos, apps e incidentes, mantendo uma curva compreensível. Falhar gera um resumo de causa e chance de repetir; o jogador deve aprender uma estratégia de jogo, não decorar uma fala. Valores como “$600”, cinco chamadas e oito minutos são apenas os [números atuais do protótipo](PROTOTYPE_RULES.md), não fatos confirmados do original nem metas finais.

### Cooperativo e escritório

O jogo de referência confirma cooperativo online e escritório 3D. Para a nossa versão, a primeira entrega será **solo**. Depois podemos testar colegas na mesma sala, meta compartilhada, divisão de chamadas, ajuda contextual e eventos de escritório. Toda ação que altera meta, tarefa ou estado de chamada precisará ter um único estado confiável no multiplayer. Incidentes cômicos devem criar decisões interessantes sem apagar uma conversa por falha de interface.

## 7. Recorte de produção

**Primeira vertical slice (jogável e avaliável):** um escritório pequeno com uma mesa, transição nítida para o desktop, notificação de chamada, botão para atender, uma cliente original com conversa por texto, um app de tarefa ligado ao que foi dito, resultado da chamada e avaliação de um turno curto. Deve funcionar em Android e iOS com o mesmo layout lógico. A IA real pode ser habilitada durante essa etapa se o servidor estiver pronto; o fluxo completo precisa ser testável com resposta local enquanto isso.

Critérios de aceitação da vertical slice:

1. O jogador chega à mesa, senta e **vê o desktop em tela cheia**; entende que saiu da visão do escritório e entrou no PC.
2. A chamada toca **dentro do desktop**. Atender abre o Telefone; recusar ou deixar tocar produz estados distintos e legíveis.
3. O jogador envia texto livre, lê a resposta e pode alternar para o aplicativo de tarefa sem perder o histórico da ligação.
4. A tarefa exige pelo menos uma informação obtida na conversa; um erro explica o que falta sem revelar a solução inteira.
5. Ao concluir, o resultado afeta a meta e a avaliação do dia. A sessão pode ser reiniciada sem estado quebrado.
6. Texto, botões e teclado são usáveis em aparelhos reais, sem precisar ampliar manualmente a interface.

**Ordem sugerida após a slice:** mais variedade de clientes e tarefas → conversa de IA confiável e observável → balanceamento e retenção por dias → arte/áudio final → incidentes do escritório → cooperativo → conteúdo adicional. A ordem depende dos testes; voz, modding e dezenas de apps ficam fora do primeiro marco.

## 8. Diferença para o protótipo atual

O protótipo agora faz a passagem **mesa → desktop → chamada recebida → telefone e aplicativo simultâneos**. A câmera se aproxima do monitor, a interface do PC ocupa a tela e uma notificação oferece atender ou recusar. Durante a ligação, Telefone e aplicativos permanecem lado a lado. A implementação ainda usa `OnGUI`, sala feita de formas simples, casos e valores predefinidos e respostas locais por regras. Não há navegação física livre, IA ativa, áudio de ligação ou cooperativo. Os números atuais estão em [Regras do protótipo](PROTOTYPE_RULES.md).

O próximo teste com o jogador deve verificar se essa transição e o uso simultâneo dos aplicativos realmente transmitem a experiência de trabalhar no PC. Depois disso, a prioridade é aprofundar a conversa e as tarefas, antes de polir os gráficos da sala.

## 9. Decisões e perguntas ainda abertas

- Testar paisagem versus retrato em aparelhos reais; paisagem é a hipótese de design, ainda não uma decisão irreversível.
- Definir se o tema final permanece um call center satírico de golpes fictícios ou muda para atendimento problemático de outra natureza. Isso afeta classificação etária, tom e conteúdo, não o fluxo central.
- Escolher provedor de IA, custo máximo por sessão, limites de uso e política de armazenamento antes de ligar respostas reais.
- Definir duração ideal do turno e dificuldade a partir de testes; não importar números do jogo de PC ou de remakes de fãs.
- Decidir quando multiplayer entra no cronograma, depois que a experiência solo de chamada e PC provar valor.

## Fontes consultadas

- [Página oficial do jogo na Steam][steam] — descrição, recursos, categorias e capturas; consultada em 03/11/2026.
- [Página dos desenvolvedores][site] — descrição do conceito e trailer; consultada em 03/11/2026.
- [Captura oficial: desktop e aplicativos][shot-desktop], [chamada recebida][shot-ring], [apps durante a conversa][shot-apps], [escritório][shot-office], [mesa e monitor][shot-desk] e [resultado][shot-results] — observação visual, não documentação de regras internas.
- [Guias públicos da comunidade Steam](https://steamcommunity.com/app/4954910/guides/) — usados para localizar temas de interesse; relatos de jogadores não são tratados como especificação oficial.
- [Remake web não oficial][fan] — consultado apenas para evitar atribuir regras inventadas ao jogo original.
- Relato do próprio jogador nesta conversa sobre sentar, entrar no PC, atender chamadas e existência de métodos; identificado como relato quando não há confirmação oficial.

[steam]: https://store.steampowered.com/app/4954910/Scam_With_Your_Friends/
[site]: https://jataterworldwide.com/scam-with-your-friends/
[fan]: https://github.com/mrzh7/scam-with-your-friends-web/blob/main/docs/GAMEPLAY-EN.md
[shot-desktop]: https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/4954910/e9b297c16108c1702873d59df1eedf5a39851be3/ss_e9b297c16108c1702873d59df1eedf5a39851be3.1920x1080.jpg
[shot-ring]: https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/4954910/7f2cbf7e73844e1d500d124009753fb6720541c2/ss_7f2cbf7e73844e1d500d124009753fb6720541c2.1920x1080.jpg
[shot-apps]: https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/4954910/bcff5f79257b09d3477d82fbfc880c4d084bfee2/ss_bcff5f79257b09d3477d82fbfc880c4d084bfee2.1920x1080.jpg
[shot-office]: https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/4954910/0de88d51b58a77eb96a848df89af9d3f7adf3f16/ss_0de88d51b58a77eb96a848df89af9d3f7adf3f16.1920x1080.jpg
[shot-desk]: https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/4954910/e3920cbb1c07287096a38574119788763d29967d/ss_e3920cbb1c07287096a38574119788763d29967d.1920x1080.jpg
[shot-results]: https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/4954910/0bad5dd0fa92399a999726f4947095952ad5f963/ss_0bad5dd0fa92399a999726f4947095952ad5f963.1920x1080.jpg
