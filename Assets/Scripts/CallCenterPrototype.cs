using UnityEngine;

public sealed class CallCenterPrototype : MonoBehaviour
{
    private const int CallsPerDay = 5;
    private const int DailyGoal = 300;

    private enum Phase { Ready, OnCall, Result, DayEnd }

    private struct Caller
    {
        public string Name;
        public string Message;
        public int FavoriteOption;

        public Caller(string name, string message, int favoriteOption)
        {
            Name = name;
            Message = message;
            FavoriteOption = favoriteOption;
        }
    }

    private static readonly Caller[] Callers =
    {
        new Caller("Lia, astronauta", "Preciso de algo para as segundas-feiras na Lua.", 0),
        new Caller("Beto, jardineiro", "Minhas plantas querem tirar férias.", 1),
        new Caller("Nina, inventora", "Quero uma ideia que ninguém leve a sério.", 2),
        new Caller("Téo, padeiro", "Meu pão começou a contar piadas.", 0),
        new Caller("Rita, capitã", "Meu navio está navegando em uma xícara.", 1)
    };

    private static readonly string[] Offers =
    {
        "Clube das segundas-feiras leves",
        "Férias imaginárias para objetos",
        "Curso de ideias absurdas"
    };

    private Phase phase = Phase.Ready;
    private Caller caller;
    private int day = 1;
    private int callsMade;
    private int earnings;
    private int lastAward;
    private string resultText = "";
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle buttonStyle;

    private void Awake()
    {
        BuildOffice();
    }

    private void StartCall()
    {
        caller = Callers[(callsMade + day - 1) % Callers.Length];
        phase = Phase.OnCall;
    }

    private void ChooseOffer(int index)
    {
        if (phase != Phase.OnCall) return;

        bool matched = index == caller.FavoriteOption;
        lastAward = matched ? 100 : 20;
        earnings += lastAward;
        callsMade++;
        resultText = matched
            ? caller.Name + " adorou a ideia. Você ganhou 100 pontos!"
            : caller.Name + " achou engraçado, mas preferia outra proposta. Você ganhou 20 pontos.";
        phase = Phase.Result;
    }

    private void Continue()
    {
        phase = callsMade >= CallsPerDay ? Phase.DayEnd : Phase.Ready;
    }

    private void NextDay()
    {
        day++;
        callsMade = 0;
        earnings = 0;
        lastAward = 0;
        phase = Phase.Ready;
    }

    private void OnGUI()
    {
        float scale = Mathf.Min(Screen.width / 1080f, Screen.height / 1920f);
        float left = (Screen.width - 1080f * scale) * 0.5f;
        float top = (Screen.height - 1920f * scale) * 0.5f;
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(left, top, 0), Quaternion.identity, new Vector3(scale, scale, 1));
        EnsureStyles();

        GUI.Box(new Rect(40, 40, 1000, 440), "");
        GUI.Label(new Rect(85, 85, 910, 100), "SCAN WITH YOUR FRIENDS", titleStyle);
        GUI.Label(new Rect(85, 200, 910, 80), "Dia " + day + "  •  Chamada " + callsMade + "/" + CallsPerDay, bodyStyle);
        GUI.Label(new Rect(85, 295, 910, 80), "Pontos: " + earnings + " / " + DailyGoal, bodyStyle);

        GUI.Box(new Rect(40, 970, 1000, 900), "");
        if (phase == Phase.Ready)
        {
            GUI.Label(new Rect(85, 1020, 910, 170), "Seu turno começou. Atenda chamadas e apresente propostas absurdas para cumprir a meta.", bodyStyle);
            if (GUI.Button(new Rect(85, 1420, 910, 130), "ATENDER CHAMADA", buttonStyle)) StartCall();
        }
        else if (phase == Phase.OnCall)
        {
            GUI.Label(new Rect(85, 1015, 910, 100), caller.Name, titleStyle);
            GUI.Label(new Rect(85, 1130, 910, 170), caller.Message, bodyStyle);
            for (int i = 0; i < Offers.Length; i++)
            {
                if (GUI.Button(new Rect(85, 1360 + i * 155, 910, 125), Offers[i], buttonStyle)) ChooseOffer(i);
            }
        }
        else if (phase == Phase.Result)
        {
            GUI.Label(new Rect(85, 1040, 910, 260), resultText, bodyStyle);
            GUI.Label(new Rect(85, 1290, 910, 90), "+" + lastAward + " pontos", titleStyle);
            if (GUI.Button(new Rect(85, 1510, 910, 130), "CONTINUAR", buttonStyle)) Continue();
        }
        else
        {
            string summary = earnings >= DailyGoal ? "Meta cumprida!" : "Meta não atingida. Tente outra estratégia!";
            GUI.Label(new Rect(85, 1050, 910, 120), summary, titleStyle);
            GUI.Label(new Rect(85, 1190, 910, 170), "Você encerrou o dia com " + earnings + " pontos em " + CallsPerDay + " chamadas.", bodyStyle);
            if (GUI.Button(new Rect(85, 1510, 910, 130), "PRÓXIMO DIA", buttonStyle)) NextDay();
        }

        GUI.matrix = previous;
    }

    private void EnsureStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 48, fontStyle = FontStyle.Bold, wordWrap = true };
        titleStyle.normal.textColor = Color.white;
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 38, wordWrap = true };
        bodyStyle.normal.textColor = Color.white;
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 34, fontStyle = FontStyle.Bold, wordWrap = true };
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
