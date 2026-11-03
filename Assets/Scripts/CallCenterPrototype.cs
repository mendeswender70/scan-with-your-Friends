using System.Collections.Generic;
using UnityEngine;

public sealed class CallCenterPrototype : MonoBehaviour
{
    private const int CallsPerDay = 5;
    private const int FirstQuota = 600;
    private const float ShiftSeconds = 300f;
    private const float RingSeconds = 20f;
    private const float CallerPatienceSeconds = 75f;

    [SerializeField] private string dialogueServerUrl = "";

    private enum Phase { Morning, Standing, Ringing, Talking, MethodSelection, MethodSteps, Result, Review }

    private struct Caller
    {
        public string Name;
        public string Problem;
        public string FollowUp;
        public string ReadyLine;

        public Caller(string name, string problem, string followUp, string readyLine)
        {
            Name = name;
            Problem = problem;
            FollowUp = followUp;
            ReadyLine = readyLine;
        }
    }

    private struct PaymentMethod
    {
        public string Name;
        public int FictionalAmount;
        public string[] Questions;
        public string[] Replies;

        public PaymentMethod(string name, int amount, string[] questions, string[] replies)
        {
            Name = name;
            FictionalAmount = amount;
            Questions = questions;
            Replies = replies;
        }
    }

    private static readonly Caller[] Callers =
    {
        new Caller("Marta, professora", "Troquei de celular e agora não consigo entrar no aplicativo da escola.",
            "O aviso diz que preciso confirmar meu acesso, mas não explica como.", "Entendi. O que você recomenda fazer primeiro?"),
        new Caller("Paulo, comerciante", "Meu computador travou quando tentei abrir a planilha de estoque.",
            "Reiniciei uma vez, mas ele continua muito lento.", "Certo. Posso acompanhar as etapas com você."),
        new Caller("Renata, aposentada", "Recebi uma cobrança de uma assinatura que eu já tinha cancelado.",
            "Tenho o comprovante do cancelamento guardado.", "Gostaria de entender como registrar essa reclamação."),
        new Caller("Davi, estudante", "Meu pedido aparece como entregue no aplicativo, mas não chegou aqui.",
            "Já conferi na portaria e ninguém recebeu nada.", "Tudo bem. Vamos ver o que consta no atendimento."),
        new Caller("Helena, designer", "Não estou conseguindo abrir meu e-mail de trabalho desde ontem.",
            "Preciso acessar um arquivo que me enviaram hoje cedo.", "Pode me explicar quais são os próximos passos?")
    };

    // These are fictional game tasks. They never request real payment details.
    private static readonly PaymentMethod[] Methods =
    {
        new PaymentMethod("Gift card fictício", 180,
            new[] { "Perguntar qual item virtual combina com a situação", "Confirmar o nome da missão no jogo", "Registrar a entrega imaginária no PC" },
            new[] { "Escolhi um item virtual.", "Essa é a missão certa.", "Entrega registrada na simulação." }),
        new PaymentMethod("Cartão de crédito fictício", 240,
            new[] { "Confirmar a proposta fictícia escolhida", "Conferir o identificador da tarefa do jogo", "Concluir a simulação no PC" },
            new[] { "Essa proposta funciona para mim.", "A tarefa do jogo está correta.", "Simulação concluída." }),
        new PaymentMethod("Transferência de fichas", 120,
            new[] { "Explicar o valor em fichas do jogo", "Confirmar a escolha do personagem", "Registrar as fichas no PC" },
            new[] { "Entendi o valor em fichas.", "É essa minha escolha.", "Fichas registradas." })
    };

    private Phase phase = Phase.Morning;
    private int day = 1;
    private int callsCompleted;
    private int earnedToday;
    private int conversationTurns;
    private int selectedMethod;
    private int methodStep;
    private int lastEarning;
    private string typedMessage = "";
    private string lastPlayerLine = "";
    private string lastCallerLine = "";
    private string resultText = "";
    private string dialogueHistory = "";
    private string dialogueStatus = "Simulação local, ainda sem IA";
    private bool awaitingReply;
    private int callSerial;
    private int callerTrust;
    private float shiftRemaining;
    private float ringRemaining;
    private float patienceRemaining;
    private string previousMessage = "";
    private Caller activeCaller;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle smallStyle;
    private GUIStyle buttonStyle;
    private GUIStyle fieldStyle;
    private GUIStyle panelStyle;
    private Camera officeCamera;
    private readonly List<Texture2D> uiTextures = new List<Texture2D>();

    private int DailyQuota { get { return FirstQuota * day; } }

    private int PayoutFor(int methodIndex) { return Methods[methodIndex].FictionalAmount * day; }

    private void Awake()
    {
        BuildOffice();
    }

    private void Update()
    {
        if (phase == Phase.Morning || phase == Phase.Review) return;
        shiftRemaining = Mathf.Max(0, shiftRemaining - Time.deltaTime);
        if (shiftRemaining <= 0)
        {
            EndShift();
            return;
        }

        if (phase == Phase.Ringing)
        {
            ringRemaining = Mathf.Max(0, ringRemaining - Time.deltaTime);
            if (ringRemaining <= 0) EndCall("A chamada não foi atendida a tempo.");
        }
        else if (phase == Phase.Talking || phase == Phase.MethodSelection || phase == Phase.MethodSteps)
        {
            patienceRemaining = Mathf.Max(0, patienceRemaining - Time.deltaTime);
            if (patienceRemaining <= 0) EndCall("A pessoa perdeu a paciência e desligou.");
        }
    }

    private void LateUpdate()
    {
        if (officeCamera == null) return;
        bool seated = phase == Phase.Ringing || phase == Phase.Talking ||
            phase == Phase.MethodSelection || phase == Phase.MethodSteps || phase == Phase.Result;
        Vector3 position = seated ? new Vector3(0, 2.8f, -2.7f) : new Vector3(5.8f, 6.2f, -9.5f);
        Vector3 focus = seated ? new Vector3(0, 1.5f, 0.5f) : new Vector3(0, 0.8f, 1.2f);
        officeCamera.transform.position = Vector3.Lerp(officeCamera.transform.position, position, Time.deltaTime * 2.5f);
        Quaternion rotation = Quaternion.LookRotation(focus - officeCamera.transform.position);
        officeCamera.transform.rotation = Quaternion.Slerp(officeCamera.transform.rotation, rotation, Time.deltaTime * 2.5f);
    }

    private void BeginCall()
    {
        callSerial++;
        activeCaller = Callers[(callsCompleted + day - 1) % Callers.Length];
        conversationTurns = 0;
        callerTrust = 45;
        patienceRemaining = CallerPatienceSeconds;
        previousMessage = "";
        methodStep = 0;
        awaitingReply = false;
        typedMessage = "";
        lastPlayerLine = "";
        lastCallerLine = activeCaller.Problem;
        dialogueHistory = "";
        dialogueStatus = string.IsNullOrWhiteSpace(dialogueServerUrl)
            ? "Simulação local, ainda sem IA" : "Conversa livre com IA configurada";
        phase = Phase.Talking;
    }

    private void SendMessage()
    {
        string message = typedMessage.Trim();
        if (phase != Phase.Talking || awaitingReply || message.Length < 3) return;

        lastPlayerLine = message;
        typedMessage = "";
        if (string.IsNullOrWhiteSpace(dialogueServerUrl))
        {
            ApplyReply(LocalReply(), "Simulação local, ainda sem IA");
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
                ApplyReply(string.IsNullOrWhiteSpace(reply) ? LocalReply() : reply,
                    error ?? "Conversa livre com IA configurada");
            }));
    }

    private string LocalReply()
    {
        return conversationTurns == 0 ? activeCaller.FollowUp : activeCaller.ReadyLine;
    }

    private void ApplyReply(string reply, string status)
    {
        lastCallerLine = reply;
        dialogueHistory += "Você: " + lastPlayerLine + "\nPessoa: " + reply + "\n";
        conversationTurns++;
        bool repeated = string.Equals(lastPlayerLine, previousMessage, System.StringComparison.OrdinalIgnoreCase);
        callerTrust = Mathf.Clamp(callerTrust + (repeated ? -15 : 12), 0, 100);
        previousMessage = lastPlayerLine;
        dialogueStatus = status;
        if (callerTrust == 0) EndCall("A pessoa perdeu a confiança e desligou.");
    }

    private void ChooseMethod(int index)
    {
        if (phase != Phase.MethodSelection) return;
        if (callerTrust < 55)
        {
            lastCallerLine = "Ainda não me sinto à vontade para continuar.";
            callerTrust = Mathf.Max(0, callerTrust - 10);
            if (callerTrust == 0) EndCall("A pessoa perdeu a confiança e desligou.");
            return;
        }
        if (selectedMethod != index) methodStep = 0;
        selectedMethod = index;
        lastCallerLine = "Vamos começar.";
        phase = Phase.MethodSteps;
    }

    private void AskQuestion(int questionIndex)
    {
        if (phase != Phase.MethodSteps) return;
        PaymentMethod method = Methods[selectedMethod];
        if (methodStep >= method.Questions.Length) return;

        if (questionIndex == methodStep)
        {
            patienceRemaining = Mathf.Max(0, patienceRemaining - 4f);
            lastCallerLine = method.Replies[methodStep];
            methodStep++;
        }
        else
        {
            patienceRemaining = Mathf.Max(0, patienceRemaining - 10f);
            callerTrust = Mathf.Max(0, callerTrust - 12);
            lastCallerLine = "Essa pergunta não combina com o que conversamos agora.";
        }

        if (callerTrust <= 0) EndCall("A pessoa perdeu a confiança e desligou.");
        else if (patienceRemaining <= 0) EndCall("A pessoa perdeu a paciência durante a tarefa.");
    }

    private void CompleteTask()
    {
        if (phase != Phase.MethodSteps || methodStep != Methods[selectedMethod].Questions.Length ||
            callerTrust < 55 || patienceRemaining <= 0) return;
        lastEarning = PayoutFor(selectedMethod);
        earnedToday += lastEarning;
        EndCall("Tarefa fictícia concluída no computador. Nenhum dado de pagamento real foi usado.", false);
    }

    private void BeginShift()
    {
        shiftRemaining = ShiftSeconds;
        phase = Phase.Standing;
    }

    private void StartRinging()
    {
        ringRemaining = RingSeconds;
        phase = Phase.Ringing;
    }

    private void EndCall(string reason, bool zeroEarning = true)
    {
        if (phase != Phase.Ringing && phase != Phase.Talking &&
            phase != Phase.MethodSelection && phase != Phase.MethodSteps) return;
        callSerial++;
        awaitingReply = false;
        if (zeroEarning) lastEarning = 0;
        callsCompleted++;
        resultText = reason;
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
        float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
        float left = (Screen.width - 1080f * scale) * 0.5f;
        float top = (Screen.height - 1920f * scale) * 0.5f;
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(left, top, 0), Quaternion.identity, new Vector3(scale, scale, 1));
        EnsureStyles();

        GUI.Box(new Rect(35, 35, 1010, 430), "", panelStyle);
        GUI.Label(new Rect(80, 75, 930, 90), "SCAN WITH YOUR FRIENDS", titleStyle);
        GUI.Label(new Rect(80, 180, 930, 75), "Dia " + day + "  |  Chamadas " + callsCompleted + "/" + CallsPerDay, bodyStyle);
        GUI.Label(new Rect(80, 280, 930, 75), "Dinheiro fictício: $" + earnedToday + " / $" + DailyQuota, bodyStyle);
        string statusLine = phase == Phase.Morning ? "Meta do expediente" : "Tempo " + FormatTime(shiftRemaining);
        if (phase == Phase.Ringing) statusLine += "  |  Toques " + Mathf.CeilToInt(ringRemaining) + "s";
        if (phase == Phase.Talking || phase == Phase.MethodSelection || phase == Phase.MethodSteps)
            statusLine += "  |  Confiança " + callerTrust + "%  |  Paciência " + Mathf.CeilToInt(patienceRemaining) + "s";
        GUI.Label(new Rect(80, 370, 930, 65), statusLine, smallStyle);

        bool viewingOffice = phase == Phase.Morning || phase == Phase.Standing;
        GUI.Box(viewingOffice ? new Rect(35, 1390, 1010, 490) : new Rect(35, 855, 1010, 1025), "", panelStyle);
        if (phase == Phase.Morning)
        {
            GUI.Label(new Rect(80, 1430, 930, 160), "O expediente vai começar. Entre no escritório e vá até sua mesa.", bodyStyle);
            Button(1670, "INICIAR DIA", BeginShift);
        }
        else if (phase == Phase.Standing)
        {
            GUI.Label(new Rect(80, 1430, 930, 160), "Sua mesa está à frente. Sente-se para usar o telefone e os programas do PC.", bodyStyle);
            Button(1670, "SENTAR NO PC", StartRinging);
        }
        else if (phase == Phase.Ringing)
        {
            Label("O telefone está tocando. Uma pessoa precisa conversar com você.");
            Button(1510, "ATENDER CHAMADA", BeginCall);
        }
        else if (phase == Phase.Talking)
        {
            GUI.Label(new Rect(80, 900, 930, 100), activeCaller.Name, titleStyle);
            GUI.Label(new Rect(80, 1010, 930, 160), "Pessoa: " + lastCallerLine, bodyStyle);
            if (lastPlayerLine.Length > 0)
                GUI.Label(new Rect(80, 1180, 930, 90), "Você: " + lastPlayerLine, smallStyle);
            GUI.Label(new Rect(80, 1300, 930, 65), dialogueStatus, smallStyle);
            typedMessage = GUI.TextField(new Rect(80, 1380, 930, 100), typedMessage, 160, fieldStyle);
            GUI.enabled = !awaitingReply;
            Button(1510, awaitingReply ? "AGUARDANDO..." : "ENVIAR RESPOSTA", SendMessage);
            if (conversationTurns >= 2 && !awaitingReply)
                Button(1655, "ABRIR MÉTODOS NO PC", delegate { phase = Phase.MethodSelection; });
            GUI.enabled = true;
            SmallButton(1790, "DESLIGAR", delegate { EndCall("Você encerrou a chamada."); });
        }
        else if (phase == Phase.MethodSelection)
        {
            GUI.Label(new Rect(80, 900, 930, 100), "Método para esta tarefa", titleStyle);
            GUI.Label(new Rect(80, 1020, 930, 100), "Escolha uma forma fictícia de concluir a chamada:", bodyStyle);
            for (int i = 0; i < Methods.Length; i++)
            {
                int methodIndex = i;
                Button(1190 + i * 160, Methods[i].Name + "  $" + PayoutFor(i),
                    delegate { ChooseMethod(methodIndex); });
            }
            SmallButton(1680, "VOLTAR À CONVERSA", delegate { phase = Phase.Talking; });
            SmallButton(1790, "DESLIGAR", delegate { EndCall("Você encerrou a chamada."); });
        }
        else if (phase == Phase.MethodSteps)
        {
            PaymentMethod method = Methods[selectedMethod];
            GUI.Label(new Rect(80, 900, 930, 110), method.Name, titleStyle);
            GUI.Label(new Rect(80, 1010, 930, 75), "Etapa " + (methodStep + 1 > method.Questions.Length ? method.Questions.Length : methodStep + 1)
                + " / " + method.Questions.Length + "  •  Escolha a próxima pergunta", smallStyle);
            GUI.Label(new Rect(80, 1120, 930, 150), "Pessoa: " + lastCallerLine, bodyStyle);
            if (methodStep < method.Questions.Length)
            {
                for (int row = 0; row < method.Questions.Length; row++)
                {
                    int questionIndex = (row + callsCompleted + day) % method.Questions.Length;
                    Button(1285 + row * 145, method.Questions[questionIndex],
                        delegate { AskQuestion(questionIndex); });
                }
            }
            else
                Button(1450, "CONCLUIR TAREFA NO PC", CompleteTask);
            SmallButton(1700, "VOLTAR À CONVERSA", delegate { phase = Phase.Talking; });
            SmallButton(1790, "DESLIGAR", delegate { EndCall("Você encerrou a chamada."); });
        }
        else if (phase == Phase.Result)
        {
            Label(resultText);
            GUI.Label(new Rect(80, 1250, 930, 100), "+$" + lastEarning, titleStyle);
            Button(1510, "CONTINUAR", delegate
            {
                if (callsCompleted >= CallsPerDay || shiftRemaining <= 0) EndShift();
                else StartRinging();
            });
        }
        else
        {
            bool passed = earnedToday >= DailyQuota;
            GUI.Label(new Rect(80, 900, 930, 150), passed ? "Meta cumprida!" : "Meta não atingida", titleStyle);
            GUI.Label(new Rect(80, 1080, 930, 180),
                "Avaliação do chefe: $" + earnedToday + " arrecadados em " + CallsPerDay + " chamadas. Meta: $" + DailyQuota + ".", bodyStyle);
            Button(1510, passed ? "PRÓXIMO DIA" : "REINICIAR DIA", delegate { ResetDay(passed); });
        }

        GUI.matrix = previous;
    }

    private void Label(string text)
    {
        GUI.Label(new Rect(80, 920, 930, 290), text, bodyStyle);
    }

    private void Button(float y, string text, System.Action action)
    {
        if (GUI.Button(new Rect(80, y, 930, 120), text, buttonStyle)) action();
    }

    private void SmallButton(float y, string text, System.Action action)
    {
        if (GUI.Button(new Rect(80, y, 930, 75), text, buttonStyle)) action();
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
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 46, fontStyle = FontStyle.Bold, wordWrap = true };
        titleStyle.normal.textColor = new Color(0.52f, 0.94f, 0.93f);
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 38, wordWrap = true };
        bodyStyle.normal.textColor = Color.white;
        smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 29, wordWrap = true };
        smallStyle.normal.textColor = new Color(0.76f, 0.85f, 0.9f);
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
