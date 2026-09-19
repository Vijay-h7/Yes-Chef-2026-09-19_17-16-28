using UnityEngine;
using UnityEngine.UI;

public class PrepSlot : MonoBehaviour
{
    [SerializeField] private Renderer itemVisual;
    [SerializeField] private GameObject progressRoot;
    [SerializeField] private Image progressFill;

    private Item item;
    private float timer;

    public bool IsEmpty => item == null;
    public bool IsDone => item != null && item.IsPrepared;

    private void Start()
    {
        GameManager.Instance.GameStarted += Clear;
        Refresh();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.GameStarted -= Clear;
    }

    private void Update()
    {
        if (item == null || item.IsPrepared) return;

        timer += Time.deltaTime;
        float duration = item.Data.prepTime;
        progressFill.fillAmount = Mathf.Clamp01(timer / duration);

        if (timer >= duration)
        {
            item.IsPrepared = true;
            Refresh();
        }
    }

    public void Begin(Item newItem)
    {
        item = newItem;
        timer = 0f;
        Refresh();
    }

    public Item Take()
    {
        var taken = item;
        Clear();
        return taken;
    }

    public void Clear()
    {
        item = null;
        timer = 0f;
        Refresh();
    }

    private void Refresh()
    {
        itemVisual.gameObject.SetActive(item != null);
        progressRoot.SetActive(item != null && !item.IsPrepared);
        progressFill.fillAmount = 0f;
        if (item != null) itemVisual.material.color = item.CurrentColor;
    }
}