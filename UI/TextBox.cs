using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
using System.IO;
using UnityEngine.SceneManagement;

public class TextBox : MonoBehaviour
{
    [SerializeField] private Text speaker;
    [SerializeField] private Text dialogueText;
    [SerializeField] private Button continueButton;
    [SerializeField] private float messageSpeed;
    [SerializeField] private string dialogueFilePath; // 例："Assets/Scenarios/sample.csv"
    [SerializeField] private string distinationScene; // 遷移先のシーン

    private EventSystem eventSystem;
    private Controller controller; // コントローラ
    public bool isActive; // このメニューが活性かどうか

    private int currentCharIndex = 0;
    private string[] speakerLines;
    private string[] dialogueLines;
    private int currentLine = 0;
    private bool isTyping = false;
    private bool isSkipping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        // 必要な他コンポーネント取得
        controller = GameObject.Find("EventSystem").GetComponent<Controller>();
        eventSystem = EventSystem.current;

        // 初期フォーカスを設定する
        eventSystem.SetSelectedGameObject(transform.GetChild(0).gameObject);

        LoadDialogueData();
        typingCoroutine = StartCoroutine(TypeDialogue());
    }

    void Update()
    {
        if (isActive){
            // 活性の時のみ操作を受け付ける
            OnShoot();
        }
    }

    // 決定ボタン
    public void OnShoot()
    {
        // ボタン押下開始時以外はreturn
        if (controller.ShootPhase != InputActionPhase.Started) return;

        controller.SetShootPhase(InputActionPhase.Performed);

        // 効果音
        SoundManager.Instance.PlaySE(SESoundData.SE.Submit);

        // 会話送り
        ContinueDialogue();
    }

    // ダイアログ継続処理
    void ContinueDialogue()
    {
        if (isTyping)
        {
            // ダイアログ出力中のとき全文出力
            isSkipping = true;
        }
        else
        {
            // 次の行を出力する
            isSkipping = false;
            currentLine++;
            if (currentLine < dialogueLines.Length)
            {
                currentCharIndex = 0;
                typingCoroutine = StartCoroutine(TypeDialogue());
            }
            else
            {
                // 出力し終わったらステージへ
                SceneManager.LoadScene(distinationScene);
            }
        }
    }

    // シナリオテキストをロードする
    void LoadDialogueData()
    {
        if (File.Exists(dialogueFilePath))
        {
            // ファイルパスからファイルを読み込み
            string[] lines = File.ReadAllLines(dialogueFilePath);
            speakerLines = new string[lines.Length];
            dialogueLines = new string[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                // ファイル内の行数分話し手、テキストを配列に格納
                string[] parts = lines[i].Split(',');
                if (parts.Length > 0)
                {
                    speakerLines[i] = parts[0];
                    dialogueLines[i] = parts[1];
                }
            }
        }
        else
        {
            Debug.LogError("シナリオデータが見つかりませんでした: " + dialogueFilePath);
        }
    }

    // ダイアログ出力処理
    IEnumerator TypeDialogue()
    {
        isTyping = true;
        speaker.text = speakerLines[currentLine]; // 話し手名を表示
        dialogueText.text = ""; // テキストをクリア
        string dialogue = dialogueLines[currentLine];

        // 行内のテキストを1文字ずつ出力
        while (currentCharIndex < dialogue.Length)
        {
            char currentChar = dialogue[currentCharIndex];

            // '|'があれば改行
            if (currentChar == '|')
            {
                dialogueText.text += "\n";
            }
            else
            {
                dialogueText.text += currentChar;
            }
            currentCharIndex++;

            if (!isSkipping)
            {
                // オプションのメッセージスピードの分待機
                yield return new WaitForSeconds(messageSpeed);
            }
        }

        isTyping = false;
    }
}
