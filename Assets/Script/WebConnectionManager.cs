using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebConnectionManager : MonoBehaviour
{
    // 伺服器URL
    public string apiUrl = "https://pas2-game-rd-lb.sayyogames.com:61337/api/unityexam/getroll";

    public static WebConnectionManager Instance { get; private set; }

    // 傳送 POST 請求，paramJson 是 JSON 格式字串
    public void PostRequest(string paramJson, Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(PostRequestCoroutine(paramJson, onSuccess, onError));
    }

    private IEnumerator PostRequestCoroutine(string jsonData, Action<string> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError("Web request error: " + request.error);
                onError?.Invoke(request.error);
            }
            else
            {
                Debug.Log("Web request success: " + request.downloadHandler.text);
                onSuccess?.Invoke(request.downloadHandler.text);
            }
        }
    }
}

