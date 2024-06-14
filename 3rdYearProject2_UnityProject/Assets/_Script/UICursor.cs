using UnityEngine;
using UnityEngine.UI;

public class UICursor : MonoBehaviour
{
    public RectTransform cursorTransform;

    void Start()
    {
        // 隱藏系統鼠標
        Cursor.visible = false;
    }

    void Update()
    {
        // 更新 UI 圖像的位置以匹配鼠標位置
        Vector2 cursorPosition = Input.mousePosition;
        cursorTransform.position = cursorPosition;
    }

    public void SetCursorImage(Sprite newCursorSprite)
    {
        cursorTransform.GetComponent<Image>().sprite = newCursorSprite;
    }

    public void SetCursorSize(Vector2 newSize)
    {
        cursorTransform.sizeDelta = newSize;
    }
}


