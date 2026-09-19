using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OrderView : MonoBehaviour
{
    [SerializeField] private CustomerWindow window;
    [SerializeField] private GameObject orderRoot;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private IngredientIcon iconPrefab;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private float popupDuration = 2.5f;

    private readonly List<IngredientIcon> icons = new();
    private Coroutine popupRoutine;

    private void Start()
    {
        window.OrderAssigned += Rebuild;
        window.OrderProgressed += Refresh;
        window.OrderCompleted += ShowPopup;

        popupText.gameObject.SetActive(false);
        Rebuild(null);
    }

    private void OnDestroy()
    {
        if (window == null) return;
        window.OrderAssigned -= Rebuild;
        window.OrderProgressed -= Refresh;
        window.OrderCompleted -= ShowPopup;
    }

    private void Update()
    {
        if (window.Current != null)
            timerText.text = $"{Mathf.FloorToInt(window.Current.ElapsedTime)}s";
    }

    private void Rebuild(Order order)
    {
        foreach (var icon in icons) Destroy(icon.gameObject);
        icons.Clear();

        orderRoot.SetActive(order != null);
        if (order == null) return;

        foreach (var ingredient in order.Required)
        {
            var icon = Instantiate(iconPrefab, iconContainer);
            icon.Setup(ingredient);
            icons.Add(icon);
        }
        Refresh();
    }

    private void Refresh()
    {
        var order = window.Current;
        if (order == null) return;
        for (int i = 0; i < icons.Count; i++) icons[i].SetFilled(order.IsFilled(i));
    }

    private void ShowPopup(int score)
    {
        if (popupRoutine != null) StopCoroutine(popupRoutine);
        popupRoutine = StartCoroutine(PopupRoutine(score));
    }

    private IEnumerator PopupRoutine(int score)
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