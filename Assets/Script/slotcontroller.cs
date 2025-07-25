using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class slotcontroller : MonoBehaviour
{
    public Button spinButton;
    public TextMeshProUGUI buttonText;

    public Transform gridParent;
    public GameObject slotPrefab;
    public Sprite[] itemSprites;

    private List<Reel> reels = new List<Reel>();
    private bool isSpinning = false;
    private Coroutine autoStopCoroutine;

    void Start()
    {
        CreateBoard();
        spinButton.onClick.AddListener(OnSpinButtonClick);
        RequestSpinResultFromAPI();
    }

    void CreateBoard()
    {
        reels.Clear();

        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < 15; i++) // 5x3
        {
            GameObject slot = Instantiate(slotPrefab, gridParent);
            Reel reel = slot.GetComponent<Reel>();
            reel.itemSprites = itemSprites;
            reels.Add(reel);
        }
    }

    void OnSpinButtonClick()
    {
        if (!isSpinning)
        {
            // 開始轉動
            foreach (var reel in reels)
            {
                reel.StartSpin();
            }

            isSpinning = true;
            buttonText.text = "Stop!";

            // 啟動自動停止協程（3 秒後自動停止）
            autoStopCoroutine = StartCoroutine(AutoStopAfterDelay(3f));
        }
        else
        {
            // 手動停止
            StopReels();
        }
    }

    IEnumerator AutoStopAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopReels();
    }

    void StopReels()
    {
        if (!isSpinning) return;

        // 停止所有轉盤（但讓它對齊）
        foreach (var reel in reels)
        {
            reel.StopSpin();
        }

        isSpinning = false;
        buttonText.text = "Spin";

        if (autoStopCoroutine != null)
        {
            StopCoroutine(autoStopCoroutine);
            autoStopCoroutine = null;
        }
        // 印出目前所有轉輪上的圖片名稱
        PrintAllReelResults();
    }
    private void PrintAllReelResults()
    {
        List<string> visibleResults = new List<string>();

        foreach (var reel in reels)
        {
            visibleResults.Add(reel.GetVisibleSymbol());
        }

        Debug.Log("盤面結果: [" + string.Join(", ", visibleResults) + "]");
    }
    void RequestSpinResultFromAPI()
    {
        string jsonParam = "{\"METHOD\":\"spin\",\"PARAMS\":\"test\"}";

        WebConnectionManager.Instance.PostRequest(jsonParam,
            (response) =>
            {
                // 成功回傳的結果字串，這裡可以解析 JSON 並帶入你的盤面
                Debug.Log("API 回傳: " + response);
                // 你可以呼叫解析函式來根據結果設定 reels 停下來的符號
            },
            (error) =>
            {
                Debug.LogError("API 請求錯誤: " + error);
            });
    }
}