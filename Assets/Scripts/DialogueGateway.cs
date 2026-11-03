using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class DialogueGateway
{
    [Serializable]
    private sealed class DialogueRequest
    {
        public string callerName;
        public string problem;
        public string history;
        public string playerMessage;
    }

    [Serializable]
    private sealed class DialogueReply
    {
        public string reply;
    }

    public static IEnumerator Ask(string endpoint, string callerName, string problem,
        string history, string playerMessage, Action<string, string> completed)
    {
        DialogueRequest payload = new DialogueRequest
        {
            callerName = callerName,
            problem = problem,
            history = history,
            playerMessage = playerMessage
        };
        byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));
        using (UnityWebRequest request = new UnityWebRequest(endpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 35;
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                completed(null, "Serviço de IA indisponível. Resposta local usada.");
                yield break;
            }

            DialogueReply response = null;
            try
            {
                response = JsonUtility.FromJson<DialogueReply>(request.downloadHandler.text);
            }
            catch (ArgumentException)
            {
                // The local dialogue remains available when a backend returns malformed JSON.
            }
            if (response == null || string.IsNullOrWhiteSpace(response.reply))
                completed(null, "Resposta da IA inválida. Resposta local usada.");
            else
                completed(response.reply.Trim(), null);
        }
    }
}
