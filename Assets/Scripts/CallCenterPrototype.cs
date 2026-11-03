using System.Collections.Generic;
using UnityEngine;

public sealed class CallCenterPrototype : MonoBehaviour
{
    private const int CallsPerDay = 5;
    private const int FirstQuota = 600;
    private const float ShiftSeconds = 480f;
    private const float RingSeconds = 25f;
    private const float CallerPatienceSeconds = 120f;

    [SerializeField] private string dialogueServerUrl = "";

    private enum Phase { Morning, Standing, EnteringDesktop, DesktopIdle, Ringing, Talking, Result, Review }
    private enum DesktopApp { Home, Methods, Task, Notes }
    private enum LocalIntent { Details, Ticket, Reference, Other }

    private struct Caller
    {
        public string Name;
        public string Problem;
        public string FollowUp;
        public string ReadyLine;
        public string Ticket;
        public string Reference;

        public Caller(string name, string problem, string followUp, string readyLine, string ticket, string reference)
        {
            Name = name;
            Problem = problem;
            FollowUp = followUp;
            ReadyLine = readyLine;
            Ticket = ticket;
            Reference = reference;
        }
    }

    private struct PaymentMethod
    {
        public string Name;
        public int FictionalAmount;
        public int RequiredTrust;
        public string ReferenceLabel;

        public PaymentMethod(string name, int amount, int requiredTrust, string referenceLabel)
        {
            Name = name;
            FictionalAmount = amount;
            RequiredTrust = requiredTrust;
            ReferenceLabel = referenceLabel;
        }
    }

    private static readonly Caller[] Callers =
    {
        new Caller("Marta, professora", "Troquei de celular e agora não consigo entrar no aplicativo da escola.",
            "O aviso diz que preciso confirmar meu acesso, mas não explica como.", "Entendi. O que você recomenda fazer primeiro?", "ESC-104", "AULA"),
        new Caller("Paulo, comerciante", "Meu computador travou quando tentei abrir a planilha de estoque.",
            "Reiniciei uma vez, mas ele continua muito lento.", "Certo. Posso acompanhar as etapas com você.", "EST-208", "LOJA"),
        new Caller("Renata, aposentada", "Recebi uma cobrança de uma assinatura que eu já tinha cancelado.",
            "Tenho o comprovante do cancelamento guardado.", "Gostaria de entender como registrar essa reclamação.", "ASS-315", "CANCELAMENTO"),
        new Caller("Davi, estudante", "Meu pedido aparece como entregue no aplicativo, mas não chegou aqui.",
            "Já conferi na portaria e ninguém recebeu nada.", "Tudo bem. Vamos ver o que consta no atendimento.", "PED-422", "ENTREGA"),
        new Caller("Helena, designer", "Não estou conseguindo abrir meu e-mail de trabalho desde ontem.",
            "Preciso acessar um arquivo que me enviaram hoje cedo.", "Pode me explicar quais são os próximos passos?", "MAIL-536", "ARQUIVO")
    };

    // These are fictional game tasks. They never request real payment details.
    private static readonly PaymentMethod[] Methods =
    {
        new PaymentMethod("Gift card fictício", 180, 55, "Referência do item virtual"),
        new PaymentMethod("Cartão de crédito fictício", 240, 70, "Referência da simulação"),
        new PaymentMethod("Transferência de fichas", 120, 50, "Referência da tarefa")
    };

    private Phase phase = Phase.Morning;
    private int day = 1;
    private int callsCompleted;
    private int earnedToday;
    private int conversationTurns;
    private int selectedMethod = -1;
    private DesktopApp desktopApp = DesktopApp.Home;
    private int lastEarning;
    private string typedMessage = "";
    private string lastPlayerLine = "";
    private string resultText = "";
    private string dialogueHistory = "";
    private string dialogueStatus = "Sem IA: pergunte problema, protocolo ou referência";
    private string ticketInput = "";
    private string referenceInput = "";
    private string desktopStatus = "";
    private bool ticketKnown;
    private bool referenceKnown;
    private bool awaitingReply;
    private int callSerial;
    private int callerTrust;
    private float shiftRemaining;
    private float ringRemaining;
    private float patienceRemaining;
    private float desktopDelay;
    private float transitionRemaining;
    private Vector2 phoneScroll;
    private string previousMessage = "";
    private Caller activeCaller;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle smallStyle;
    private GUIStyle dialogueStyle;
    private GUIStyle buttonStyle;
    private GUIStyle fieldStyle;
    private GUIStyle panelStyle;
    private GUIStyle desktopStyle;
    private GUIStyle windowStyle;
    private GUIStyle modalStyle;
    private GUIStyle headerStyle;
    private Camera officeCamera;
    private readonly List<Texture2D> uiTextures = new List<Texture2D>();

    private int DailyQuota { get { return FirstQuota * day; } }

    private int PayoutFor(int methodIndex) { return Methods[methodIndex].FictionalAmount * day; }

    private void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif
        BuildOffice();
    }

    private void Update()
    {
        if (phase == Phase.Morning || phase == Phase.Review) return;
        if (phase == Phase.EnteringDesktop)
        {
            transitionRemaining -= Time.deltaTime;
            if (transitionRemaining <= 0) OpenDesktop();
        }
        shiftRemaining = Mathf.Max(0, shiftRemaining - Time.deltaTime);
        if (shiftRemaining <= 0)
        {
            EndShift();
            return;
        }

        if (phase == Phase.DesktopIdle)
        {
            desktopDelay -= Time.deltaTime;
            if (desktopDelay <= 0) StartRinging();
        }
        else if (phase == Phase.Ringing)
        {
            ringRemaining = Mathf.Max(0, ringRemaining - Time.deltaTime);
            if (ringRemaining <= 0) EndCall("A chamada não foi atendida a tempo.");
        }
        else if (phase == Phase.Talking)
        {
            patienceRemaining = Mathf.Max(0, patienceRemaining - Time.deltaTime);
            if (patienceRemaining <= 0) EndCall("A pessoa perdeu a paciência e desligou.");
        }
    }

    private void LateUpdate()
    {
        if (officeCamera == null) return;
        bool seated = phase == Phase.EnteringDesktop || phase == Phase.DesktopIdle ||
            phase == Phase.Ringing || phase == Phase.Talking || phase == Phase.Result;
        Vector3 position = seated ? new Vector3(0, 1.7f, -1.8f) : new Vector3(5.8f, 6.2f, -9.5f);
        Vector3 focus = seated ? new Vector3(0, 1.55f, 0.05f) : new Vector3(0, 0.8f, 1.2f);
        officeCamera.transform.position = Vector3.Lerp(officeCamera.transform.position, position, Time.deltaTime * 2.5f);
        Quaternion rotation = Quaternion.LookRotation(focus - officeCamera.transform.position);
        officeCamera.transform.rotation = Quaternion.Slerp(officeCamera.transform.rotation, rotation, Time.deltaTime * 2.5f);
    }

    private void BeginCall()
    {
        callSerial++;
        conversationTurns = 0;
        callerTrust = 45;
        patienceRemaining = CallerPatienceSeconds;
        previousMessage = "";
        selectedMethod = -1;
        ticketInput = "";
        referenceInput = "";
        ticketKnown = false;
        referenceKnown = false;
        awaitingReply = false;
        typedMessage = "";
        lastPlayerLine = "";
        dialogueHistory = "Pessoa: " + activeCaller.Problem + "\n";
        phoneScroll = Vector2.zero;
        desktopApp = DesktopApp.Home;
        desktopStatus = "Escolha um programa quando estiver pronto para a tarefa.";
        dialogueStatus = string.IsNullOrWhiteSpace(dialogueServerUrl)
            ? "Sem IA: pergunte problema, protocolo ou referência" : "Conversa livre com IA configurada";
        phase = Phase.Talking;
    }

    private void SendMessage()
    {
        string message = typedMessage.Trim();
        if (phase != Phase.Talking || awaitingReply || message.Length < 3) return;

        lastPlayerLine = message;
        typedMessage = "";
        LocalIntent intent = ClassifyIntent(message);
        if (string.IsNullOrWhiteSpace(dialogueServerUrl) || intent == LocalIntent.Ticket || intent == LocalIntent.Reference)
        {
            ApplyReply(LocalReply(intent), "Sem IA: pergunte problema, protocolo ou referência", intent);
            return;
        }

        awaitingReply = true;
        dialogueStatus = "Aguardando resposta da IA...";
        int serial = callSerial;
        StartCoroutine(DialogueGateway.Ask(dialogueServerUrl, activeCaller.Name, activeCaller.Problem,
            dialogueHistory, message, delegate(string reply, string error)
            {
                if (serial != callSerial || phase != Phase.Talking) return;
                awaitingReply = false;
                ApplyReply(string.IsNullOrWhiteSpace(reply) ? LocalReply(intent) : reply,
                    error ?? "Conversa livre com IA configurada", intent);
            }));
    }

    private static LocalIntent ClassifyIntent(string message)
    {
        string text = message.ToLowerInvariant();
        if (text.Contains("protocolo") || text.Contains("chamado") || text.Contains("ticket")) return LocalIntent.Ticket;
        if (text.Contains("refer") || text.Contains("palavra") || text.Contains("categoria")) return LocalIntent.Reference;
        if (text.Contains("problema") || text.Contains("aconteceu") || text.Contains("aparece") || text.Contains("ajuda"))
            return LocalIntent.Details;
        return LocalIntent.Other;
    }

    private string LocalReply(LocalIntent intent)
    {
        if (intent == LocalIntent.Ticket) return "O protocolo fictício é " + activeCaller.Ticket + ".";
        if (intent == LocalIntent.Reference) return "A referência da tarefa é " + activeCaller.Reference + ".";
        if (intent == LocalIntent.Details)
            return conversationTurns == 0 ? activeCaller.FollowUp : activeCaller.ReadyLine;
        return "Não entendi bem. O que você precisa saber sobre meu atendimento?";
    }

    private void ApplyReply(string reply, string status, LocalIntent intent)
    {
        dialogueHistory += "Você: " + lastPlayerLine + "\nPessoa: " + reply + "\n";
        phoneScroll.y = 100000f;
        if (intent == LocalIntent.Ticket) ticketKnown = true;
        if (intent == LocalIntent.Reference) referenceKnown = true;
        conversationTurns++;
        bool repeated = string.Equals(lastPlayerLine, previousMessage, System.StringComparison.OrdinalIgnoreCase);
        int trustChange = repeated ? -15 : intent == LocalIntent.Other ? 0 : intent == LocalIntent.Details ? 8 : 12;
        callerTrust = Mathf.Clamp(callerTrust + trustChange, 0, 100);
        previousMessage = lastPlayerLine;
        dialogueStatus = status;
        if (callerTrust == 0) EndCall("A pessoa perdeu a confiança e desligou.");
    }

    private void ChooseMethod(int index)
    {
        if (phase != Phase.Talking || index < 0 || index >= Methods.Length) return;
        PaymentMethod method = Methods[index];
        if (callerTrust < method.RequiredTrust)
        {
            desktopStatus = "Confiança insuficiente para este programa. Volte à conversa.";
            return;
        }
        if (selectedMethod != index)
        {
            ticketInput = "";
            referenceInput = "";
        }
        selectedMethod = index;
        desktopStatus = "Preencha os campos com informações desta chamada. Se precisar, volte ao telefone.";
        desktopApp = DesktopApp.Task;
    }

    private void CompleteTask()
    {
        if (phase != Phase.Talking || selectedMethod < 0 || patienceRemaining <= 0) return;
        PaymentMethod method = Methods[selectedMethod];
        if (callerTrust < method.RequiredTrust)
        {
            desktopStatus = "A confiança caiu. Volte ao telefone antes de concluir a tarefa.";
            return;
        }
        if (!ticketKnown || !referenceKnown)
        {
            desktopStatus = "Faltam informações da conversa. Pergunte ao cliente antes de validar.";
            return;
        }

        bool validTicket = string.Equals(ticketInput.Trim(), activeCaller.Ticket, System.StringComparison.OrdinalIgnoreCase);
        bool validReference = string.Equals(referenceInput.Trim(), activeCaller.Reference, System.StringComparison.OrdinalIgnoreCase);
        if (!validTicket || !validReference)
        {
            callerTrust = Mathf.Max(0, callerTrust - 5);
            patienceRemaining = Mathf.Max(0, patienceRemaining - 8f);
            desktopStatus = "Os dados fictícios não conferem. Pergunte ao cliente e tente novamente.";
            if (callerTrust <= 0) EndCall("A pessoa perdeu a confiança e desligou.");
            else if (patienceRemaining <= 0) EndCall("A pessoa perdeu a paciência durante a tarefa.");
            return;
        }

        lastEarning = PayoutFor(selectedMethod);
        earnedToday += lastEarning;
        EndCall("Tarefa fictícia concluída no computador. Nenhum dado de pagamento real foi usado.", false);
    }

    private void BeginShift()
    {
        shiftRemaining = ShiftSeconds;
        phase = Phase.Standing;
    }

    private void EnterDesktop()
    {
        if (phase != Phase.Standing) return;
        transitionRemaining = 1.25f;
        phase = Phase.EnteringDesktop;
    }

    private void OpenDesktop()
    {
        desktopApp = DesktopApp.Home;
        desktopDelay = 3f;
        phase = Phase.DesktopIdle;
    }

    private void LeaveDesktop()
    {
        if (phase == Phase.DesktopIdle) phase = Phase.Standing;
    }

    private void StartRinging()
    {
        activeCaller = Callers[(callsCompleted + day - 1) % Callers.Length];
        ringRemaining = RingSeconds;
        phase = Phase.Ringing;
    }

    private void EndCall(string reason, bool zeroEarning = true)
    {
        if (phase != Phase.Ringing && phase != Phase.Talking) return;
        callSerial++;
        awaitingReply = false;
        if (zeroEarning) lastEarning = 0;
        callsCompleted++;
        resultText = reason;
        desktopApp = DesktopApp.Home;
        phase = Phase.Result;
    }

    private void EndShift()
    {
        callSerial++;
        awaitingReply = false;
        phase = Phase.Review;
    }

    private void ResetDay(bool advance)
    {
        if (advance) day++;
        callsCompleted = 0;
        earnedToday = 0;
        shiftRemaining = 0;
        phase = Phase.Morning;
    }

    private void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
        float left = (Screen.width - 1600f * scale) * 0.5f;
        float top = (Screen.height - 900f * scale) * 0.5f;
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(left, top, 0), Quaternion.identity, new Vector3(scale, scale, 1));
        EnsureStyles();

        if (phase == Phase.Morning || phase == Phase.Standing ||
            phase == Phase.EnteringDesktop || phase == Phase.Review)
            DrawOfficeInterface();
        else
            DrawDesktopInterface();

        GUI.matrix = previous;
    }

    private void DrawOfficeInterface()
    {
        GUI.Box(new Rect(40, 35, 950, 145), "", panelStyle);
        GUI.Label(new Rect(70, 55, 880, 65), "SCAN WITH YOUR FRIENDS", titleStyle);
        GUI.Label(new Rect(70, 120, 870, 45),
            "Dia " + day + "  •  Meta fictícia $" + DailyQuota + "  •  Chamadas " + callsCompleted + "/" + CallsPerDay, smallStyle);
        GUI.Box(new Rect(35, 635, 1530, 220), "", panelStyle);
        if (phase == Phase.Morning)
        {
            GUI.Label(new Rect(70, 665, 1050, 65), "O expediente vai começar. Sua mesa está à frente.", bodyStyle);
            UiButton(new Rect(1165, 680, 350, 95), "INICIAR DIA", BeginShift);
        }
        else if (phase == Phase.Standing)
        {
            GUI.Label(new Rect(70, 665, 1050, 85), "Sente-se à mesa para entrar no computador e começar a receber chamadas.", bodyStyle);
            UiButton(new Rect(1165, 680, 350, 95), "SENTAR NO PC", EnterDesktop);
        }
        else if (phase == Phase.EnteringDesktop)
        {
            GUI.Label(new Rect(70, 685, 1200, 80), "Entrando no computador...", bodyStyle);
        }
        else
        {
            bool passed = earnedToday >= DailyQuota;
            GUI.Label(new Rect(70, 655, 1040, 65), passed ? "Meta cumprida!" : "Meta não atingida", titleStyle);
            GUI.Label(new Rect(70, 725, 1050, 90),
                "Avaliação: $" + earnedToday + " arrecadados em " + callsCompleted + " chamadas. Meta: $" + DailyQuota + ".", smallStyle);
            UiButton(new Rect(1165, 680, 350, 95), passed ? "PRÓXIMO DIA" : "REINICIAR DIA",
                delegate { ResetDay(passed); });
        }
    }

    private void DrawDesktopInterface()
    {
        GUI.Box(new Rect(0, 0, 1600, 900), "", desktopStyle);
        GUI.Box(new Rect(0, 0, 1600, 83), "", headerStyle);
        GUI.Label(new Rect(40, 15, 580, 55), "CENTRAL / COMPUTADOR 01", titleStyle);
        GUI.Label(new Rect(640, 25, 920, 45),
            "Dia " + day + "  •  " + callsCompleted + "/" + CallsPerDay + " chamadas  •  " + FormatTime(shiftRemaining) +
            "  •  $" + earnedToday + " / $" + DailyQuota, smallStyle);

        if (phase == Phase.Talking)
        {
            DrawPhoneWindow();
            DrawWorkWindow();
        }
        else
        {
            DrawDesktopHome();
            if (phase == Phase.Ringing) DrawIncomingCall();
            if (phase == Phase.Result) DrawCallResult();
        }

        GUI.Box(new Rect(0, 827, 1600, 73), "", headerStyle);
        GUI.Label(new Rect(40, 845, 470, 45), "●  CENTRAL OS    |    TELEFONE    |    ARQUIVOS", smallStyle);
        string footer = phase == Phase.Ringing ? "CHAMADA RECEBIDA • " + Mathf.CeilToInt(ringRemaining) + "s" :
            phase == Phase.Talking ? "EM LIGAÇÃO • Confiança " + callerTrust + "% • Paciência " + Mathf.CeilToInt(patienceRemaining) + "s" :
            "EXPEDIENTE EM ANDAMENTO";
        GUI.Label(new Rect(800, 845, 750, 45), footer, smallStyle);
    }

    private void DrawDesktopHome()
    {
        GUI.Label(new Rect(75, 150, 900, 75), "Área de trabalho", titleStyle);
        GUI.Label(new Rect(75, 230, 1130, 100),
            phase == Phase.DesktopIdle ? "Computador iniciado. Aguarde a próxima chamada..." :
            "Telefone, programas e anotações ficam abertos neste computador.", bodyStyle);
        GUI.Box(new Rect(75, 390, 370, 250), "", windowStyle);
        GUI.Label(new Rect(105, 425, 310, 65), "☎  TELEFONE", titleStyle);
        GUI.Label(new Rect(105, 510, 300, 90), "As chamadas chegam aqui durante o expediente.", smallStyle);
        GUI.Box(new Rect(470, 390, 370, 250), "", windowStyle);
        GUI.Label(new Rect(500, 425, 310, 65), "▣  PROGRAMAS", titleStyle);
        GUI.Label(new Rect(500, 510, 300, 90), "Use aplicativos para concluir cada caso fictício.", smallStyle);
        GUI.Box(new Rect(865, 390, 370, 250), "", windowStyle);
        GUI.Label(new Rect(895, 425, 310, 65), "▤  ANOTAÇÕES", titleStyle);
        GUI.Label(new Rect(895, 510, 300, 90), "As pistas da conversa aparecem aqui.", smallStyle);
        if (phase == Phase.DesktopIdle)
            UiButton(new Rect(1260, 690, 280, 90), "SAIR DO PC", LeaveDesktop);
    }

    private void DrawIncomingCall()
    {
        GUI.Box(new Rect(450, 205, 700, 510), "", modalStyle);
        GUI.Label(new Rect(505, 250, 600, 65), "CHAMADA RECEBIDA", titleStyle);
        GUI.Label(new Rect(505, 345, 600, 120), activeCaller.Name, bodyStyle);
        GUI.Label(new Rect(505, 475, 600, 70), "Atender ou recusar? " + Mathf.CeilToInt(ringRemaining) + "s", smallStyle);
        UiButton(new Rect(505, 580, 285, 92), "RECUSAR", delegate { EndCall("Você recusou a chamada."); });
        UiButton(new Rect(815, 580, 285, 92), "ATENDER", BeginCall);
    }

    private void DrawCallResult()
    {
        GUI.Box(new Rect(370, 195, 860, 530), "", modalStyle);
        GUI.Label(new Rect(425, 240, 750, 65), "RESULTADO DA CHAMADA", titleStyle);
        GUI.Label(new Rect(425, 345, 750, 145), resultText, bodyStyle);
        GUI.Label(new Rect(425, 505, 750, 65), "+$" + lastEarning + " em dinheiro fictício", titleStyle);
        UiButton(new Rect(425, 600, 750, 88), "CONTINUAR NO COMPUTADOR", delegate
        {
            if (callsCompleted >= CallsPerDay || shiftRemaining <= 0) EndShift();
            else OpenDesktop();
        });
    }

    private void DrawPhoneWindow()
    {
        GUI.Box(new Rect(35, 105, 725, 690), "", windowStyle);
        GUI.Box(new Rect(35, 105, 725, 58), "", headerStyle);
        GUI.Label(new Rect(62, 113, 650, 48), "☎  TELEFONE • CHAMADA ATIVA", smallStyle);
        GUI.Label(new Rect(65, 178, 650, 65), activeCaller.Name, titleStyle);
        GUI.Label(new Rect(65, 252, 650, 40), "Confiança " + callerTrust + "%  •  Paciência " +
            Mathf.CeilToInt(patienceRemaining) + "s", smallStyle);

        float historyHeight = Mathf.Max(250, dialogueStyle.CalcHeight(new GUIContent(dialogueHistory), 625) + 25);
        phoneScroll = GUI.BeginScrollView(new Rect(65, 305, 665, 245), phoneScroll,
            new Rect(0, 0, 625, historyHeight));
        GUI.Label(new Rect(0, 0, 615, historyHeight), dialogueHistory, dialogueStyle);
        GUI.EndScrollView();

        GUI.Label(new Rect(65, 560, 650, 40), dialogueStatus, smallStyle);
        typedMessage = GUI.TextField(new Rect(65, 610, 665, 65), typedMessage, 160, fieldStyle);
        GUI.enabled = !awaitingReply;
        UiButton(new Rect(65, 695, 435, 72), awaitingReply ? "AGUARDANDO..." : "ENVIAR MENSAGEM", SendMessage);
        GUI.enabled = true;
        UiButton(new Rect(520, 695, 210, 72), "DESLIGAR", delegate { EndCall("Você encerrou a chamada."); });
    }

    private void DrawWorkWindow()
    {
        GUI.Box(new Rect(785, 105, 780, 690), "", windowStyle);
        GUI.Box(new Rect(785, 105, 780, 58), "", headerStyle);
        GUI.Label(new Rect(810, 113, 740, 48), "▣  PROGRAMAS DO COMPUTADOR", smallStyle);

        UiButton(new Rect(810, 178, 165, 62), "INÍCIO", delegate { desktopApp = DesktopApp.Home; });
        UiButton(new Rect(988, 178, 165, 62), "MÉTODOS", delegate { desktopApp = DesktopApp.Methods; });
        UiButton(new Rect(1166, 178, 165, 62), "TAREFA", delegate { desktopApp = DesktopApp.Task; });
        UiButton(new Rect(1344, 178, 190, 62), "ANOTAÇÕES", delegate { desktopApp = DesktopApp.Notes; });

        if (desktopApp == DesktopApp.Methods) DrawMethodsApp();
        else if (desktopApp == DesktopApp.Task) DrawTaskApp();
        else if (desktopApp == DesktopApp.Notes) DrawNotesApp();
        else
        {
            GUI.Label(new Rect(825, 285, 690, 70), "Caso em andamento", titleStyle);
            GUI.Label(new Rect(825, 380, 685, 190),
                "Converse com a pessoa pelo telefone. Você pode abrir programas e anotações sem encerrar a ligação.", bodyStyle);
            UiButton(new Rect(825, 655, 690, 90), "ABRIR PROGRAMAS", delegate { desktopApp = DesktopApp.Methods; });
        }
    }

    private void DrawMethodsApp()
    {
        GUI.Label(new Rect(825, 265, 690, 60), "Escolha um programa", titleStyle);
        GUI.Label(new Rect(825, 330, 690, 65), "As informações para a tarefa vêm da conversa.", smallStyle);
        for (int i = 0; i < Methods.Length; i++)
        {
            int methodIndex = i;
            UiButton(new Rect(825, 410 + i * 88, 690, 72),
                Methods[i].Name + "  •  $" + PayoutFor(i) + "  •  Confiança " + Methods[i].RequiredTrust + "%",
                delegate { ChooseMethod(methodIndex); });
        }
        GUI.Label(new Rect(825, 690, 690, 75), desktopStatus, smallStyle);
    }

    private void DrawTaskApp()
    {
        if (selectedMethod < 0)
        {
            GUI.Label(new Rect(825, 295, 690, 130), "Escolha primeiro um programa na aba Métodos.", bodyStyle);
            return;
        }
        PaymentMethod method = Methods[selectedMethod];
        GUI.Label(new Rect(825, 265, 690, 60), method.Name, titleStyle);
        GUI.Label(new Rect(825, 340, 690, 42), "Protocolo fictício do atendimento", smallStyle);
        ticketInput = GUI.TextField(new Rect(825, 385, 690, 60), ticketInput, 30, fieldStyle);
        GUI.Label(new Rect(825, 455, 690, 42), method.ReferenceLabel, smallStyle);
        referenceInput = GUI.TextField(new Rect(825, 500, 690, 60), referenceInput, 30, fieldStyle);
        GUI.Label(new Rect(825, 570, 690, 70), desktopStatus, smallStyle);
        UiButton(new Rect(825, 665, 690, 90), "VALIDAR TAREFA", CompleteTask);
    }

    private void DrawNotesApp()
    {
        GUI.Label(new Rect(825, 285, 690, 70), "Anotações do atendimento", titleStyle);
        GUI.Label(new Rect(825, 380, 690, 240),
            "Protocolo: " + (ticketKnown ? activeCaller.Ticket : "ainda não descoberto") + "\n\n" +
            "Referência: " + (referenceKnown ? activeCaller.Reference : "ainda não descoberta"), bodyStyle);
        GUI.Label(new Rect(825, 665, 690, 85),
            "Se faltar algo, pergunte à pessoa; o telefone continua aberto ao lado.", smallStyle);
    }

    private void UiButton(Rect rect, string text, System.Action action)
    {
        if (GUI.Button(rect, text, buttonStyle)) action();
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.CeilToInt(seconds);
        return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
    }

    private void EnsureStyles()
    {
        if (titleStyle != null) return;
        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = SolidTexture(new Color(0.04f, 0.09f, 0.14f, 0.92f));
        desktopStyle = new GUIStyle(GUI.skin.box);
        desktopStyle.normal.background = SolidTexture(new Color(0.10f, 0.18f, 0.28f));
        windowStyle = new GUIStyle(GUI.skin.box);
        windowStyle.normal.background = SolidTexture(new Color(0.05f, 0.10f, 0.17f));
        modalStyle = new GUIStyle(GUI.skin.box);
        modalStyle.normal.background = SolidTexture(new Color(0.07f, 0.15f, 0.23f));
        headerStyle = new GUIStyle(GUI.skin.box);
        headerStyle.normal.background = SolidTexture(new Color(0.03f, 0.07f, 0.12f));
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 46, fontStyle = FontStyle.Bold, wordWrap = true };
        titleStyle.normal.textColor = new Color(0.52f, 0.94f, 0.93f);
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 38, wordWrap = true };
        bodyStyle.normal.textColor = Color.white;
        smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 29, wordWrap = true };
        smallStyle.normal.textColor = new Color(0.76f, 0.85f, 0.9f);
        dialogueStyle = new GUIStyle(GUI.skin.label) { fontSize = 29, wordWrap = true };
        dialogueStyle.normal.textColor = Color.white;
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 31, fontStyle = FontStyle.Bold, wordWrap = true };
        buttonStyle.normal.background = SolidTexture(new Color(0.08f, 0.57f, 0.61f));
        buttonStyle.hover.background = SolidTexture(new Color(0.1f, 0.7f, 0.7f));
        buttonStyle.active.background = SolidTexture(new Color(0.04f, 0.43f, 0.48f));
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.active.textColor = Color.white;
        fieldStyle = new GUIStyle(GUI.skin.textField) { fontSize = 34 };
        fieldStyle.normal.background = SolidTexture(new Color(0.9f, 0.95f, 0.96f));
        fieldStyle.normal.textColor = new Color(0.04f, 0.09f, 0.14f);
    }

    private Texture2D SolidTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        uiTextures.Add(texture);
        return texture;
    }

    private void OnDestroy()
    {
        foreach (Texture2D texture in uiTextures)
            Destroy(texture);
    }

    private void BuildOffice()
    {
        officeCamera = new GameObject("Office Camera").AddComponent<Camera>();
        officeCamera.transform.position = new Vector3(5.8f, 6.2f, -9.5f);
        officeCamera.transform.LookAt(new Vector3(0, 0.8f, 1.2f));
        officeCamera.fieldOfView = 72;
        officeCamera.clearFlags = CameraClearFlags.SolidColor;
        officeCamera.backgroundColor = new Color(0.12f, 0.19f, 0.25f);
        RenderSettings.ambientLight = new Color(0.7f, 0.74f, 0.8f);

        Light light = new GameObject("Office Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.3f;
        light.transform.rotation = Quaternion.Euler(50, -35, 0);

        Color wall = new Color(0.33f, 0.42f, 0.49f);
        Color floor = new Color(0.2f, 0.26f, 0.3f);
        Cube("Carpet", new Vector3(0, -0.18f, 0), new Vector3(13, 0.25f, 13), floor);
        Cube("Back wall", new Vector3(0, 2.4f, 6.5f), new Vector3(13, 5, 0.2f), wall);
        Cube("Left wall", new Vector3(-6.5f, 2.4f, 0), new Vector3(0.2f, 5, 13), wall);
        Cube("Wall trim", new Vector3(0, 0.25f, 6.34f), new Vector3(13, 0.25f, 0.18f), new Color(0.19f, 0.28f, 0.33f));
        Cube("Window frame", new Vector3(-3.4f, 2.9f, 6.32f), new Vector3(3.7f, 2.2f, 0.11f), new Color(0.1f, 0.16f, 0.22f));
        Cube("Window glass", new Vector3(-3.4f, 2.9f, 6.23f), new Vector3(3.4f, 1.9f, 0.08f), new Color(0.25f, 0.58f, 0.65f));
        Cube("Window divider", new Vector3(-3.4f, 2.9f, 6.16f), new Vector3(0.08f, 1.9f, 0.09f), new Color(0.1f, 0.16f, 0.22f));
        Cube("Office notice board", new Vector3(2.6f, 3.0f, 6.31f), new Vector3(2.6f, 1.7f, 0.13f), new Color(0.65f, 0.45f, 0.3f));
        Cube("Notice", new Vector3(2.2f, 3.1f, 6.2f), new Vector3(0.8f, 0.9f, 0.03f), new Color(0.95f, 0.92f, 0.72f));
        Cube("Notice 2", new Vector3(3.25f, 2.8f, 6.2f), new Vector3(0.7f, 0.7f, 0.03f), new Color(0.67f, 0.87f, 0.92f));

        for (int i = 0; i < 9; i++)
            Cube("Carpet seam " + i, new Vector3(-5.8f + i * 1.45f, -0.04f, 0),
                new Vector3(0.015f, 0.01f, 12.5f), new Color(0.23f, 0.3f, 0.35f));

        BuildDesk("Your desk", 0, -0.2f, new Color(0.08f, 0.66f, 0.72f));
        BuildDesk("Coworker desk A", -3.7f, 3.4f, new Color(0.35f, 0.7f, 0.52f));
        BuildDesk("Coworker desk B", 3.7f, 3.4f, new Color(0.74f, 0.48f, 0.28f));
        BuildCoworker("Coworker A", -3.7f, 2.2f, new Color(0.45f, 0.7f, 0.58f));
        BuildCoworker("Coworker B", 3.7f, 2.2f, new Color(0.85f, 0.58f, 0.38f));
        BuildPlant("Office plant", -5.4f, 4.9f);

        Cube("Reception cabinet", new Vector3(5.3f, 0.7f, -2.5f), new Vector3(1.1f, 1.4f, 2.2f), new Color(0.5f, 0.58f, 0.61f));
        Cube("Cabinet drawer", new Vector3(4.72f, 0.95f, -2.5f), new Vector3(0.04f, 0.45f, 1.8f), new Color(0.3f, 0.39f, 0.44f));
        Cube("Ceiling light", new Vector3(0, 4.5f, 2.2f), new Vector3(2.8f, 0.12f, 0.7f), new Color(0.95f, 0.92f, 0.72f));
    }

    private static void BuildDesk(string label, float x, float z, Color screenColor)
    {
        Color wood = new Color(0.57f, 0.39f, 0.26f);
        Color metal = new Color(0.21f, 0.27f, 0.31f);
        Cube(label + " top", new Vector3(x, 0.82f, z), new Vector3(2.8f, 0.15f, 1.4f), wood);
        Cube(label + " left leg", new Vector3(x - 1.15f, 0.37f, z), new Vector3(0.13f, 0.76f, 1.1f), metal);
        Cube(label + " right leg", new Vector3(x + 1.15f, 0.37f, z), new Vector3(0.13f, 0.76f, 1.1f), metal);
        Cube(label + " monitor frame", new Vector3(x, 1.55f, z + 0.32f), new Vector3(1.25f, 0.9f, 0.12f), metal);
        Cube(label + " monitor screen", new Vector3(x, 1.55f, z + 0.25f), new Vector3(1.1f, 0.74f, 0.03f), screenColor);
        Cube(label + " monitor stand", new Vector3(x, 1.05f, z + 0.32f), new Vector3(0.12f, 0.22f, 0.12f), metal);
        Cube(label + " keyboard", new Vector3(x, 0.94f, z - 0.32f), new Vector3(0.95f, 0.06f, 0.25f), metal);
        Shape(label + " mouse", PrimitiveType.Sphere, new Vector3(x + 0.82f, 0.95f, z - 0.25f),
            new Vector3(0.16f, 0.07f, 0.22f), metal);
        Shape(label + " phone", PrimitiveType.Cylinder, new Vector3(x - 1.0f, 0.97f, z + 0.1f),
            new Vector3(0.2f, 0.08f, 0.2f), new Color(0.12f, 0.62f, 0.39f));
        Shape(label + " chair base", PrimitiveType.Cylinder, new Vector3(x, 0.25f, z - 1.3f),
            new Vector3(0.55f, 0.12f, 0.55f), metal);
        Cube(label + " chair seat", new Vector3(x, 0.55f, z - 1.3f), new Vector3(0.9f, 0.18f, 0.7f), metal);
        Cube(label + " chair back", new Vector3(x, 1.06f, z - 1.65f), new Vector3(0.9f, 0.95f, 0.18f), metal);
    }

    private static void BuildCoworker(string label, float x, float z, Color shirt)
    {
        Shape(label + " body", PrimitiveType.Capsule, new Vector3(x, 1.1f, z),
            new Vector3(0.55f, 0.85f, 0.45f), shirt);
        Shape(label + " head", PrimitiveType.Sphere, new Vector3(x, 1.8f, z),
            new Vector3(0.38f, 0.43f, 0.38f), new Color(0.79f, 0.54f, 0.38f));
        Shape(label + " headset", PrimitiveType.Cylinder, new Vector3(x - 0.25f, 1.85f, z),
            new Vector3(0.12f, 0.15f, 0.12f), new Color(0.1f, 0.13f, 0.17f));
    }

    private static void BuildPlant(string label, float x, float z)
    {
        Shape(label + " pot", PrimitiveType.Cylinder, new Vector3(x, 0.34f, z),
            new Vector3(0.45f, 0.36f, 0.45f), new Color(0.67f, 0.39f, 0.27f));
        Shape(label + " leaves", PrimitiveType.Sphere, new Vector3(x, 1.25f, z),
            new Vector3(0.8f, 1.0f, 0.8f), new Color(0.2f, 0.56f, 0.34f));
        Shape(label + " leaves high", PrimitiveType.Sphere, new Vector3(x + 0.25f, 1.65f, z),
            new Vector3(0.55f, 0.65f, 0.55f), new Color(0.29f, 0.67f, 0.39f));
    }

    private static void Cube(string label, Vector3 position, Vector3 size, Color color)
    {
        Shape(label, PrimitiveType.Cube, position, size, color);
    }

    private static void Shape(string label, PrimitiveType type, Vector3 position, Vector3 size, Color color)
    {
        GameObject shape = GameObject.CreatePrimitive(type);
        shape.name = label;
        shape.transform.position = position;
        shape.transform.localScale = size;
        shape.GetComponent<Renderer>().material.color = color;
    }
}
