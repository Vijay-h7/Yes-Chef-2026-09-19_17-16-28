using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientIcon : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text label;
    [SerializeField] private CanvasGroup group;

    public void Setup(IngredientData data)
    {
        background.color = data.preparedColor;
        label.text = data.shortLabel;
    }

    public void SetFilled(bool filled) => group.alpha = filled ? 0.25f : 1f;
}