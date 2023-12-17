using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text;

public class ExampleClass : MonoBehaviour
{
    // 在Unity Inspector視窗中指定的變數
    public string apiKey; // apiKey = "YOUR_API_KEY"
    public string GeminiEndpoint; // GeminiEndpoint = "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent"
    public string message;
    public InputField userInputField; // 接收使用者輸入的 InputField
    public Text outputText; // 用於設置 Text UI 的參考

    void Start()
    {
        // 偵測到 Eneter 才 SendRequestToGemini()
        userInputField.onEndEdit.AddListener(delegate { SendRequestToGemini();});
    }

    public void SendRequestToGemini()
    {
        // 讀取使用者輸入的 InputField
        string message = userInputField.text;
        // 創建要發送的JSON數據，並設定使用用戶輸入的提示文本
        string requestData = "{ \"contents\":[{\"role\": \"user\",\"parts\":[{\"text\": \"" + message + "\"}]}]}";
        // 創建一個新的UnityWebRequest並設定為POST
        UnityWebRequest request = new UnityWebRequest(GeminiEndpoint, "POST");
        // 使用 UTF-8 編碼轉換成一個位元組陣列
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestData);
        // 設定上傳資料，將原始二進制數據發送到伺服器
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        // 設定下載資料，將從伺服器接收到的數據保存在內存中
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        // 設定 HTTP 請求的標頭
        request.SetRequestHeader("x-goog-api-key", apiKey);
        request.SetRequestHeader("Content-Type", "application/json");
        StartCoroutine(SendRequest(request));
    }

    IEnumerator SendRequest(UnityWebRequest request)
    {
        // 發送請求並等待完成
        yield return request.SendWebRequest();

        // 判斷請求是否成功
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            // 請求成功，解析 JSON 並提取 text 部分
            string jsonResponse = request.downloadHandler.text;
            ExtractAndLogText(jsonResponse);
        }
    }

    void ExtractAndLogText(string jsonResponse)
    {
        // 解析 JSON 字串
        var json = JsonUtility.FromJson<GeminiResponse>(jsonResponse);

        // 檢查是否存在 candidates
        if (json.candidates != null && json.candidates.Length > 0)
        {
            // 檢查是否存在 content 和 parts
            if (json.candidates[0].content != null && json.candidates[0].content.parts != null && json.candidates[0].content.parts.Length > 0)
            {
                // 取得並輸出 text 部分
                string text = json.candidates[0].content.parts[0].text;
                Debug.Log("Gemini Response Text: " + text);
                if (text != null)
                {
                    outputText.text = text;
                }
            }
        }
    }

    // 設定 JSON 字符串解析的結構
    [System.Serializable]
    public class GeminiResponse
    {
        public Candidate[] candidates;

        [System.Serializable]
        public class Candidate
        {
            public Content content;

            [System.Serializable]
            public class Content
            {
                public Part[] parts;

                [System.Serializable]
                public class Part
                {
                    public string text;
                }
            }
        }
    }
}
