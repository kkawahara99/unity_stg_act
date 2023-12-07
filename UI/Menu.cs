using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Menu : MonoBehaviour
{
    private EventSystem eventSystem;
    private GameObject currentSelected;
    private Controller controller; // コントローラ
    public bool isActive; // このメニューが活性かどうか
    private GameObject previousMenu; // 前メニュー

    [SerializeField] private GameObject cursorIconPrefab; // カーソルアイコン
    [SerializeField] private List<GameObject> menuPrefabs; // Menuプレハブ

    private void Start()
    {
        // 必要な他コンポーネント取得
        controller = GameObject.Find("EventSystem").GetComponent<Controller>();
        eventSystem = EventSystem.current;

        // 初期フォーカスを設定する
        eventSystem.SetSelectedGameObject(transform.GetChild(1).GetChild(0).gameObject);
        currentSelected = eventSystem.currentSelectedGameObject;
        CreateCursorIcon(currentSelected);

        // メニューの活性化
        isActive = true;
    }

    void Update()
    {
        if (isActive){
            // 活性の時のみ操作を受け付ける
            OnMove();
            OnShoot();
            OnSlash();
        }
    }

    // 十字キー
    public void OnMove()
    {
        // ボタン押下開始時以外はreturn
        if (controller.MovePhase != InputActionPhase.Started) return;

        controller.SetMovePhase(InputActionPhase.Performed);
        Vector2 direction = controller._direction;
        Debug.Log(direction);

        if (direction.y > 0.99f || direction.y < -0.99f)
        {
            // 上下方向が押されたとき
            SelectButton(direction.y);
        }

    }

    // 決定ボタン
    public void OnShoot()
    {
        // ボタン押下開始時以外はreturn
        if (controller.ShootPhase != InputActionPhase.Started) return;

        controller.SetShootPhase(InputActionPhase.Performed);

        // 選択したボタンのメニューを生成する
        int n = GetButtonIndex(currentSelected.GetComponent<Selectable>());
        GameObject newMenuObject = Instantiate(menuPrefabs[n], transform.parent);

        // 生成した新しいメニューにこのメニュー情報を渡す
        newMenuObject.GetComponent<Menu>().SetPreviousMenu(this.gameObject);

        // このメニューを非活性にする
        isActive = false;
    }

    // 取消ボタン
    public void OnSlash()
    {
        // ボタン押下開始時以外はreturn
        if (controller.SlashPhase != InputActionPhase.Started) return;
        Debug.Log("Cancel");

        controller.SetSlashPhase(InputActionPhase.Performed);

        // 前メニューを活性にする
        previousMenu.GetComponent<Menu>().SetIsActive(true);

        // このメニューを破棄する
        Destroy(this.gameObject);
    }

    // ボタン選択
    void SelectButton(float vertical)
    {
        Selectable nextButton = null;

        if (vertical > 0)
        {
            nextButton = currentSelected.GetComponent<Selectable>().FindSelectableOnUp();
        }
        else if (vertical < 0)
        {
            nextButton = currentSelected.GetComponent<Selectable>().FindSelectableOnDown();
        }

        if (nextButton != null)
        {
            // 現在のボタンのカーソルアイコンを非表示
            RemoveCursorIcon(currentSelected);
            currentSelected = nextButton.gameObject;

            // 次のボタンのカーソルアイコンを表示
            CreateCursorIcon(currentSelected);

            eventSystem.SetSelectedGameObject(currentSelected);
        }
    }

    // カーソルアイコンの生成
    private void CreateCursorIcon(GameObject buttonObject)
    {
        if (buttonObject != null)
        {
            GameObject cursorObject = Instantiate(cursorIconPrefab, buttonObject.transform);
            RectTransform cursorRectTransform = cursorObject.GetComponent<RectTransform>();
            cursorRectTransform.anchorMin = new Vector2(0f, 0.5f);
            cursorRectTransform.anchorMax = new Vector2(0f, 0.5f);
            cursorRectTransform.pivot = new Vector2(0.5f, 0.5f);
            cursorRectTransform.anchoredPosition = new Vector2(-30f, 0f);
            cursorObject.name = "Cursor";
        }
    }

    // カーソルアイコンの削除
    private void RemoveCursorIcon(GameObject buttonObject)
    {
        if (buttonObject != null)
        {
            GameObject cursorObject = buttonObject.transform.Find("Cursor").gameObject;
            Destroy(cursorObject);
        }
    }

    // ボタンが何番目のボタンかを取得
    int GetButtonIndex(Selectable selectable)
    {
        // ボタンがアタッチされている親オブジェクトの中でSelectableコンポーネントを持つ全ての子オブジェクトを取得
        Selectable[] allSelectables = selectable.transform.parent.GetComponentsInChildren<Selectable>(true);

        // ボタンが何番目かを検索
        for (int i = 0; i < allSelectables.Length; i++)
        {
            if (allSelectables[i] == selectable)
            {
                return i;
            }
        }

        return -1; // 見つからなかった場合は-1を返すか、適切なエラー処理を行う
    }

    public void SetPreviousMenu(GameObject previousMenu)
    {
        this.previousMenu = previousMenu;
    }

    public void SetIsActive(bool isActive)
    {
        this.isActive = isActive;
    }
}
