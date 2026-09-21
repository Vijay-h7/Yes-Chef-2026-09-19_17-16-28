using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Pure logic. No UnityEngine components, so it is easy to test.
public class Order
{
    private readonly IngredientData[] required;
    private readonly bool[] filled;

    public IReadOnlyList<IngredientData> Required => required;
    public float Elapsed { get; private set; }
    public bool IsComplete => filled.All(f => f);

    // Sum of ingredient values minus whole seconds elapsed (rounded down). Can go negative.
    public int Score => required.Sum(i => i.scoreValue) - Mathf.FloorToInt(Elapsed);

    public Order(IngredientData[] pool, float threeIngredientChance)
    {
        int count = Random.value < threeIngredientChance ? 3 : 2;
        required = new IngredientData[count];
        filled = new bool[count];
        for (int i = 0; i < count; i++)
            required[i] = pool[Random.Range(0, pool.Length)];   // duplicates allowed
    }

    public bool IsFilled(int index) => filled[index];
    public void Tick(float deltaTime) => Elapsed += deltaTime;

    // Accept only prepared items that the order still needs.
    public bool TryFulfil(Item item)
    {
        if (!item.IsReady) return false;

        for (int i = 0; i < required.Length; i++)
        {
            if (filled[i] || required[i] != item.Data) continue;
            filled[i] = true;
            return true;
        }
        return false;
    }
}

public class CustomerWindow : MonoBehaviour, IInteractable
{
    [Header("Rules")]
    [SerializeField] private IngredientData[] ingredientPool;
    [SerializeField, Range(0f, 1f)] private float threeIngredientChance = 0.5f;
    [SerializeField] private float respawnDelay = 5f;

    [Header("World-space UI")]
    [SerializeField] private GameObject orderRoot;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private float popupDuration = 2.5f;

    private Order current;
    private readonly List<CanvasGroup> icons = new List<CanvasGroup>();

    private void Start()
    {
        GameManager.Instance.GameStarted += BeginRound;
        popupText.gameObject.SetActive(false);
        ShowOrder(null);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.GameStarted -= BeginRound;
    }

    private void Update()
    {
        if (current == null) return;
        current.Tick(Time.deltaTime);
        timerText.text = $"{Mathf.FloorToInt(current.Elapsed)}s";
    }

    private void BeginRound()
    {
        StopAllCoroutines();                       // cancel leftover respawn or popup from last round
        popupText.gameObject.SetActive(false);
        NewOrder();
    }

    private void NewOrder()
    {
        current = new Order(ingredientPool, threeIngredientChance);
        ShowOrder(current);
    }

    public void Interact(Chef chef)
    {
        if (current == null || chef.IsHandEmpty) return;
        if (!current.TryFulfil(chef.Held)) return;   // not needed here: item stays in hand

        chef.Clear();
        RefreshIcons();
        if (!current.IsComplete) return;

        int score = current.Score;
        current = null;
        ShowOrder(null);
        GameManager.Instance.AddScore(score);
        StartCoroutine(ShowPopup(score));
        StartCoroutine(RespawnAfterDelay());
    }

    // WaitForSeconds uses scaled time, so pausing also pauses the respawn.
    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        NewOrder();
    }

    private void ShowOrder(Order order)
    {
        foreach (var icon in icons) Destroy(icon.gameObject);
        icons.Clear();

        orderRoot.SetActive(order != null);
        if (order == null) return;

        foreach (var ingredient in order.Required)
        {
            var icon = Instantiate(iconPrefab, iconContainer);
            icon.GetComponent<Image>().color = ingredient.preparedColor;
            icon.GetComponentInChildren<TMP_Text>().text = ingredient.shortLabel;
            icons.Add(icon.GetComponent<CanvasGroup>());
        }
    }

    private void RefreshIcons()
    {
        for (int i = 0; i < icons.Count; i++)
            icons[i].alpha = current.IsFilled(i) ? 0.25f : 1f;   // dim delivered ingredients
    }

    private IEnumerator ShowPopup(int score)
    {
        popupText.gameObject.SetActive(true);
        popupText.text = score >= 0 ? $"+{score}" : score.ToString();

        Color color = score >= 0 ? Color.green : Color.red;
        for (float t = 0f; t < popupDuration; t += Time.deltaTime)
        {
            color.a = 1f - t / popupDuration;
            popupText.color = color;
            yield return null;
        }
        popupText.gameObject.SetActive(false);
    }
}
