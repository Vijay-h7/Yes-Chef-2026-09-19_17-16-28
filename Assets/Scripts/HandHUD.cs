using TMPro;
using UnityEngine;
using UnityEngine.UI;

// HUD line that always shows what the chef's hand holds: HANDS: EMPTY / HANDS: Meat (cooked) + a colour dot.
public class HandHUD : MonoBehaviour
{
    [SerializeField] private Chef chef;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image swatch;
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.55f);
    [SerializeField] private Color holdingColor = new Color(1f, 0.86f, 0.35f, 1f);

    private Item shown;
    private bool shownPrepared, shownBurnt, dirty = true;

    private void Update()
    {
        var item = chef.Held;
        bool changed = dirty || item != shown
            || (item != null && (item.IsPrepared != shownPrepared || item.IsBurnt != shownBurnt));
        if (!changed) return;

        dirty = false;
        shown = item;
        if (item == null)
        {
            label.text = "HANDS: EMPTY";
            label.color = emptyColor;
            swatch.color = new Color(1f, 1f, 1f, 0.2f);
            return;
        }

        shownPrepared = item.IsPrepared;
        shownBurnt = item.IsBurnt;
        label.text = $"HANDS: {item.Data.name.ToUpper()}{State(item)}";
        label.color = holdingColor;
        swatch.color = item.CurrentColor;
    }

    private static string State(Item item)
    {
        if (item.IsBurnt) return " (burnt)";
        if (item.IsPrepared) return item.Data.prepType == PrepType.Chop ? " (chopped)" : " (cooked)";
        return item.Data.prepType == PrepType.None ? "" : " (raw)";
    }
}
