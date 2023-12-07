using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Button : MonoBehaviour
{
    private EventSystem eventSystem;
    private Selectable selectedButton;
    private Image focusImage;

    void Start()
    {
        eventSystem = EventSystem.current;
    }

    void Update()
    {
        if (eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponent<Selectable>() != null)
        {
            // ボタンが選択されている場合
            if (selectedButton != eventSystem.currentSelectedGameObject.GetComponent<Selectable>())
            {
                // 選択されたボタンが変更された場合
                selectedButton = eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
                AddFocusEffect(selectedButton);
            }
        }
        else
        {
            // フォーカスがない場合、フォーカスをクリア
            ClearFocusEffect();
        }
    }

    void AddFocusEffect(Selectable button)
    {
        // フォーカスエフェクトを追加
        if (focusImage == null)
        {
            focusImage = button.gameObject.GetComponent<Image>();
            focusImage.color = Color.red; // フォーカス時の色を設定
            focusImage.raycastTarget = false; // インタラクティブでないようにする
        }
    }

    void ClearFocusEffect()
    {
        focusImage = gameObject.GetComponent<Image>();
        focusImage.color = Color.white; // フォーカス時の色を設定
        focusImage.raycastTarget = false; // インタラクティブでないようにする
    }
}
