using UnityEngine;

public sealed class CallCenterPrototype : MonoBehaviour
{
    private const int CallsPerDay = 5;
    private const int FirstQuota = 600;

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
    private Caller activeCaller;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle smallStyle;
    private GUIStyle buttonStyle;
    private GUIStyle fieldStyle;

    private int DailyQuota { get { return FirstQuota * day; } }

    private int PayoutFor(int methodIndex) { return Methods[methodIndex].FictionalAmount * day; }

    private void Awake()
    {
        BuildOffice();
    }

    private void BeginCall()
    {
        activeCaller = Callers[(callsCompleted + day - 1) % Callers.Length];
        conversationTurns = 0;
        typedMessage = "";
        lastPlayerLine = "";
        lastCallerLine = activeCaller.Problem;
        phase = Phase.Talking;
    }

    private void SendMessage()
    {
        string message = typedMessage.Trim();
        if (phase != Phase.Talking || message.Length < 3) return;

        lastPlayerLine = message;
        lastCallerLine = conversationTurns == 0 ? activeCaller.FollowUp : activeCaller.ReadyLine;
        conversationTurns++;
        typedMessage = "";
    }

    private void ChooseMethod(int index)
    {
        if (phase != Phase.MethodSelection) return;
        selectedMethod = index;
        methodStep = 0;
        lastCallerLine = "Vamos começar.";
        phase = Phase.MethodSteps;
    }

    private void AskNextQuestion()
    {
        if (phase != Phase.MethodSteps) return;
        PaymentMethod method = Methods[selectedMethod];
        if (methodStep >= method.Questions.Length) return;
        lastCallerLine = method.Replies[methodStep];
        methodStep++;
    }

    private void CompleteTask()
    {
        if (phase != Phase.MethodSteps || methodStep != Methods[selectedMethod].Questions.Length) return;
        lastEarning = PayoutFor(selectedMethod);
        earnedToday += lastEarning;
        callsCompleted++;
        resultText = "Tarefa fictícia concluída no computador. Nenhum dado de pagamento real foi usado.";
        phase = Phase.Result;
    }

    private void ResetDay(bool advance)
    {
        if (advance) day++;
        callsCompleted = 0;
        earnedToday = 0;
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

        GUI.Box(new Rect(35, 35, 1010, 430), "");
        GUI.Label(new Rect(80, 75, 930, 90), "SCAN WITH YOUR FRIENDS", titleStyle);
        GUI.Label(new Rect(80, 180, 930, 75), "Dia " + day + "  |  Chamadas " + callsCompleted + "/" + CallsPerDay, bodyStyle);
        GUI.Label(new Rect(80, 280, 930, 75), "Dinheiro fictício: $" + earnedToday + " / $" + DailyQuota, bodyStyle);
        GUI.Label(new Rect(80, 370, 930, 55), "Meta do expediente", smallStyle);

        GUI.Box(new Rect(35, 855, 1010, 1025), "");
        if (phase == Phase.Morning)
        {
            Label("O expediente vai começar. Atenda chamadas e conclua tarefas no computador para bater a meta.");
            Button(1510, "INICIAR DIA", delegate { phase = Phase.Standing; });
        }
        else if (phase == Phase.Standing)
        {
            Label("Você está no escritório. A sua mesa está logo à frente.");
            Button(1510, "SENTAR NO PC", delegate { phase = Phase.Ringing; });
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
            GUI.Label(new Rect(80, 1310, 930, 55), "Sua resposta (simulação local, ainda sem IA)", smallStyle);
            typedMessage = GUI.TextField(new Rect(80, 1380, 930, 100), typedMessage, 160, fieldStyle);
            Button(1510, "ENVIAR RESPOSTA", SendMessage);
            if (conversationTurns >= 2)
                Button(1655, "ABRIR MÉTODOS NO PC", delegate { phase = Phase.MethodSelection; });
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
        }
        else if (phase == Phase.MethodSteps)
        {
            PaymentMethod method = Methods[selectedMethod];
            GUI.Label(new Rect(80, 900, 930, 110), method.Name, titleStyle);
            GUI.Label(new Rect(80, 1010, 930, 75), "Etapa " + (methodStep + 1 > method.Questions.Length ? method.Questions.Length : methodStep + 1)
                + " / " + method.Questions.Length, smallStyle);
            GUI.Label(new Rect(80, 1120, 930, 150), "Pessoa: " + lastCallerLine, bodyStyle);
            if (methodStep < method.Questions.Length)
                Button(1450, method.Questions[methodStep], AskNextQuestion);
            else
                Button(1450, "CONCLUIR TAREFA NO PC", CompleteTask);
        }
        else if (phase == Phase.Result)
        {
            Label(resultText);
            GUI.Label(new Rect(80, 1250, 930, 100), "+$" + lastEarning, titleStyle);
            Button(1510, "CONTINUAR", delegate { phase = callsCompleted >= CallsPerDay ? Phase.Review : Phase.Ringing; });
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

    private void EnsureStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 46, fontStyle = FontStyle.Bold, wordWrap = true };
        titleStyle.normal.textColor = Color.white;
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 38, wordWrap = true };
        bodyStyle.normal.textColor = Color.white;
        smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 29, wordWrap = true };
        smallStyle.normal.textColor = Color.white;
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 31, fontStyle = FontStyle.Bold, wordWrap = true };
        fieldStyle = new GUIStyle(GUI.skin.textField) { fontSize = 34 };
    }

    private void BuildOffice()
    {
        Camera camera = new GameObject("Camera").AddComponent<Camera>();
        camera.transform.position = new Vector3(5, 5, -8);
        camera.transform.LookAt(new Vector3(0, 0.7f, 0));
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.08f, 0.11f, 0.19f);

        Light light = new GameObject("Office Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.5f;
        light.transform.rotation = Quaternion.Euler(45, -30, 0);

        Cube("Floor", new Vector3(0, -0.2f, 0), new Vector3(9, 0.25f, 8), new Color(0.14f, 0.2f, 0.28f));
        Cube("Desk", new Vector3(0, 0.8f, 0), new Vector3(4, 0.25f, 2), new Color(0.47f, 0.3f, 0.2f));
        Cube("Left Leg", new Vector3(-1.7f, 0.35f, 0), new Vector3(0.2f, 0.8f, 1.5f), new Color(0.3f, 0.2f, 0.17f));
        Cube("Right Leg", new Vector3(1.7f, 0.35f, 0), new Vector3(0.2f, 0.8f, 1.5f), new Color(0.3f, 0.2f, 0.17f));
        Cube("Monitor", new Vector3(0, 1.65f, 0.3f), new Vector3(1.7f, 1.2f, 0.12f), new Color(0.05f, 0.6f, 0.7f));
        Cube("Chair", new Vector3(0, 0.5f, -2.3f), new Vector3(1.4f, 1, 0.35f), new Color(0.3f, 0.38f, 0.8f));
    }

    private static void Cube(string label, Vector3 position, Vector3 size, Color color)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = label;
        cube.transform.position = position;
        cube.transform.localScale = size;
        cube.GetComponent<Renderer>().material.color = color;
    }
}
