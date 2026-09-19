using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [SerializeField] private Renderer heldVisual;

    public Item Held { get; private set; }
    public bool IsEmpty => Held == null;

    private void Awake() => Refresh();

    public void Pick(Item item)
    {
        Held = item;
        Refresh();
    }

    public Item Take()
    {
        var item = Held;
        Held = null;
        Refresh();
        return item;
    }

    public void Clear() => Take();

    private void Refresh()
    {
        heldVisual.gameObject.SetActive(Held != null);
        if (Held != null) heldVisual.material.color = Held.CurrentColor;
    }
}