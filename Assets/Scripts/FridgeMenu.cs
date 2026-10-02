using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Popup with 3 ingredient choices. The game clock keeps running (Overcooked-style),
// but the chef can't move while it is open. Pick with 1/2/3 or click; E / Space closes.
public class FridgeMenu : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TMP_Text[] optionLabels;
    [SerializeField] private Image[] optionSwatches;
    [SerializeField] private Button closeButton;

    private IngredientData[] options;
    private Chef chef;
    private int openedFrame = -1;

    private void Awake()
    {
        IsOpen = false;
        panel.SetActive(false);
    }

    private void Start()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => Choose(index));
        }
        closeButton.onClick.AddListener(Close);
        GameManager.Instance.StateChanged += OnStateChanged;
        GameManager.Instance.GameStarted += Close;
    }

    private void OnDestroy()
    {
        IsOpen = false;
        if (GameManager.Instance == null) return;
        GameManager.Instance.StateChanged -= OnStateChanged;
        GameManager.Instance.GameStarted -= Close;
    }

    private void OnStateChanged(GameState state)
    {
        if (state != GameState.Playing) Close();
    }

    public void Open(Chef who, IngredientData[] choices)
    {
        if (IsOpen || choices == null) return;
        chef = who;
        options = choices;
        for (int i = 0; i < optionButtons.Length && i < choices.Length; i++)
        {
            optionLabels[i].text = $"<b>{i + 1}   {choices[i].DisplayName}</b>\n<size=62%>{Hint(choices[i])}</size>";
            if (choices[i].icon != null)
            {
                optionSwatches[i].sprite = choices[i].icon;
                optionSwatches[i].color = Color.white;
            }
            else
            {
                optionSwatches[i].sprite = null;
                optionSwatches[i].color = choices[i].rawColor;
            }
        }
        openedFrame = Time.frameCount;
        panel.SetActive(true);
        IsOpen = true;
    }

    private void Update()
    {
        if (!IsOpen || Time.frameCount == openedFrame) return;   // ignore the E press that opened us
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) Choose(0);
        else if (kb.digit2Key.wasPressedThisFrame) Choose(1);
        else if (kb.digit3Key.wasPressedThisFrame) Choose(2);
        else if (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) Close();
    }

    private void Choose(int index)
    {
        if (!IsOpen || options == null || index >= options.Length) return;
        if (chef != null && chef.IsHandEmpty) chef.Pick(new Item(options[index]));
        Close();
    }

    public void Close()
    {
        IsOpen = false;
        if (panel != null) panel.SetActive(false);
    }

    private static string Hint(IngredientData d)
    {
        switch (d.prepType)
        {
            case PrepType.Chop: return $"Chop on the counter ({d.prepTime:0}s)";
            case PrepType.Cook: return $"Cook on the stove ({d.prepTime:0}s)";
            default: return "Ready to serve";
        }
    }
}
