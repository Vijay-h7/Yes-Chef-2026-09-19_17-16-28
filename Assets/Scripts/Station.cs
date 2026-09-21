using UnityEngine;
using UnityEngine.UI;

public enum StationType { IngredientSource, Table, Stove, Trash }

public class Station : MonoBehaviour, IInteractable
{
    // One spot on a table or stove: what is on it, its timer, and its visuals.
    [System.Serializable]
    public class Slot
    {
        public Renderer itemVisual;
        public GameObject progressRoot;
        public Image progressFill;
        [System.NonSerialized] public Item item;
        [System.NonSerialized] public float timer;
    }

    [SerializeField] private StationType type;
    [Tooltip("Only for IngredientSource: what this crate gives out.")]
    [SerializeField] private IngredientData ingredient;
    [Tooltip("Only for Table (1 slot) and Stove (2 slots).")]
    [SerializeField] private Slot[] slots;

    private void Start()
    {
        GameManager.Instance.GameStarted += ResetStation;
        ResetStation();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.GameStarted -= ResetStation;
    }

    private void Update()
    {
        if (slots == null) return;
        foreach (var slot in slots) TickSlot(slot);
    }

    public void Interact(Chef chef)
    {
        switch (type)
        {
            case StationType.IngredientSource:
                if (chef.IsHandEmpty && ingredient != null) chef.Pick(new Item(ingredient));
                break;

            case StationType.Trash:
                chef.Clear();
                break;

            case StationType.Table:
            case StationType.Stove:
                InteractWithSlots(chef);
                break;
        }
    }

    private void InteractWithSlots(Chef chef)
    {
        if (slots == null) return;
        var required = type == StationType.Table ? PrepType.Chop : PrepType.Cook;

        // Empty hand: pick up a finished item.
        if (chef.IsHandEmpty)
        {
            foreach (var slot in slots)
            {
                if (slot.item == null || !slot.item.IsPrepared) continue;
                chef.Pick(slot.item);
                ClearSlot(slot);
                return;
            }
            return;
        }

        // Holding a raw item of the right kind: put it in a free slot.
        var held = chef.Held;
        if (held.IsPrepared || held.Data.prepType != required) return;

        foreach (var slot in slots)
        {
            if (slot.item != null) continue;
            slot.item = chef.Take();
            slot.timer = 0f;
            RefreshSlot(slot);
            return;
        }
    }

    private void TickSlot(Slot slot)
    {
        if (slot.item == null || slot.item.IsPrepared) return;

        slot.timer += Time.deltaTime;
        float duration = slot.item.Data.prepTime;
        slot.progressFill.fillAmount = Mathf.Clamp01(slot.timer / duration);

        if (slot.timer >= duration)
        {
            slot.item.IsPrepared = true;
            RefreshSlot(slot);
        }
    }

    private void ClearSlot(Slot slot)
    {
        slot.item = null;
        slot.timer = 0f;
        RefreshSlot(slot);
    }

    private void RefreshSlot(Slot slot)
    {
        slot.itemVisual.gameObject.SetActive(slot.item != null);
        slot.progressRoot.SetActive(slot.item != null && !slot.item.IsPrepared);
        slot.progressFill.fillAmount = 0f;
        if (slot.item != null) slot.itemVisual.material.color = slot.item.CurrentColor;
    }

    private void ResetStation()
    {
        if (slots == null) return;
        foreach (var slot in slots) ClearSlot(slot);
    }
}
