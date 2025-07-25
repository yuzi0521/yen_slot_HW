using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Reel : MonoBehaviour
{
    public RectTransform reelContent;
    public Sprite[] itemSprites;

    public float spinSpeed = 300f;    // 控制下滑速度
    public float imageHeight = 100f;    // 每張圖高度

    private float currentY = 0f;
    private int totalImages;
    private bool isSpinning = false;
    private bool stopRequested = false;
    private string targetSymbol = "";       // API 指定要停下來的符號名稱

    void Start()
    {
        //totalImages = reelContent.childCount;
        currentY = reelContent.anchoredPosition.y; 
    }
    public void SetTargetSymbol(string symbolName)
    {
        targetSymbol = symbolName;
    }

    public void StartSpin()
    {
        ShuffleSprites(); // 洗牌一次
        isSpinning = true;
        stopRequested = false;
    }

    public void StopSpin()
    {
        stopRequested = true;
    }

    void Update()
    {
        if (!isSpinning) return;

        currentY -= spinSpeed * Time.deltaTime;
        reelContent.anchoredPosition = new Vector2(0, currentY);

        HandleLoop();

        if (stopRequested)
        {
            isSpinning = false;
            stopRequested = false;

            // 確保停在目標符號（若有）
            if (!string.IsNullOrEmpty(targetSymbol))
            {
                AlignToTargetSymbol();
            }

            // 最終對齊最近格子位置
            float snappedY = Mathf.Round(currentY / imageHeight) * imageHeight;
            currentY = snappedY;
            reelContent.anchoredPosition = new Vector2(0, currentY);
        }
    }
    void HandleLoop()
    {
        for (int i = 0; i < reelContent.childCount; i++)
        {
            RectTransform child = reelContent.GetChild(i) as RectTransform;

            Vector3 worldPos = child.position;
            Vector3 viewPos = Camera.main.WorldToViewportPoint(worldPos);

            // 如果圖片已經完全離開底部畫面 (y < 0)
            if (viewPos.y < 0)
            {
                // 將此 child 移到最上方
                float newY = GetMaxChildY() + imageHeight;
                Vector2 anchored = child.anchoredPosition;
                child.anchoredPosition = new Vector2(anchored.x, newY);

            }
        }
    }
        float GetMaxChildY()
    {
        float maxY = float.MinValue;
        foreach (Transform child in reelContent)
        {
            RectTransform rt = child as RectTransform;
            if (rt.anchoredPosition.y > maxY)
                maxY = rt.anchoredPosition.y;
        }
        return maxY;
    }

    void ShuffleSprites()
    {
        // 先把所有 child 存下來
        List<Transform> children = new List<Transform>();

        foreach (Transform child in reelContent)
        {
            children.Add(child);
        }

        // 打亂順序（Fisher-Yates 洗牌）
        for (int i = 0; i < children.Count; i++)
        {
            int randomIndex = Random.Range(i, children.Count);
            // 交換 anchoredPosition 而非 child 本身順序（保留原層級）
            Vector2 tempPos = ((RectTransform)children[i]).anchoredPosition;
            ((RectTransform)children[i]).anchoredPosition = ((RectTransform)children[randomIndex]).anchoredPosition;
            ((RectTransform)children[randomIndex]).anchoredPosition = tempPos;
        }
    }
    public List<string> GetCurrentReelResults()
    {
        List<Transform> sortedChildren = new List<Transform>();

        foreach (Transform child in reelContent)
        {
            sortedChildren.Add(child);
        }

        sortedChildren.Sort((a, b) =>
        {
            float yA = ((RectTransform)a).anchoredPosition.y;
            float yB = ((RectTransform)b).anchoredPosition.y;
            return yB.CompareTo(yA);
        });

        List<string> results = new List<string>();
        foreach (var child in sortedChildren)
        {
            results.Add(child.name);
        }

        return results;
    }
    void AlignToTargetSymbol()
    {
        List<Transform> sortedChildren = new List<Transform>();

        foreach (Transform child in reelContent)
        {
            sortedChildren.Add(child);
        }

        // 根據位置從上到下排序
        sortedChildren.Sort((a, b) =>
        {
            float yA = ((RectTransform)a).anchoredPosition.y;
            float yB = ((RectTransform)b).anchoredPosition.y;
            return yB.CompareTo(yA);
        });

        // 找到第一個匹配的圖案
        foreach (Transform child in sortedChildren)
        {
            Image img = child.GetComponent<Image>();
            if (img.sprite.name == targetSymbol)
            {
                // 對齊此物件
                currentY = -((RectTransform)child).anchoredPosition.y;
                reelContent.anchoredPosition = new Vector2(0, currentY);
                break;
            }
        }
    }
    public string GetVisibleSymbol()
    {
        Transform closestChild = null;
        float closestDistance = float.MaxValue;

        foreach (Transform child in reelContent)
        {
            RectTransform rt = child as RectTransform;
            float distance = Mathf.Abs(rt.position.y - transform.position.y);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestChild = child;
            }
        }

        if (closestChild != null)
        {
            Image img = closestChild.GetComponent<Image>();
            return img != null && img.sprite != null ? img.sprite.name : " ";
        }

        return " ";
    }
}