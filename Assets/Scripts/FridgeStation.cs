using System.Collections;
using UnityEngine;

// The single fridge. E opens the 3-choice menu, but only when the chef's hand is empty.
public class FridgeStation : MonoBehaviour, IInteractable
{
    [SerializeField] private FridgeMenu menu;
    [Tooltip("Vegetable, Cheese, Meat")]
    [SerializeField] private IngredientData[] options;

    private Coroutine punch;

    public void Interact(Chef chef)
    {
        if (FridgeMenu.IsOpen) return;

        if (!chef.IsHandEmpty)
        {
            // Hands full: tiny grey puff so the player sees why nothing happened.
            FX.Burst(transform.position + Vector3.up * 2f, new Color(0.6f, 0.6f, 0.6f), 5, 0.8f, 0.05f, 0.35f);
            return;
        }

        menu.Open(chef, options);
        if (punch != null) StopCoroutine(punch);
        punch = StartCoroutine(Punch());
    }

    private IEnumerator Punch()
    {
        Vector3 baseScale = Vector3.one;
        for (float t = 0f; t < 0.18f; t += Time.deltaTime)
        {
            float k = Mathf.Sin(t / 0.18f * Mathf.PI);
            transform.localScale = baseScale * (1f + 0.06f * k);
            yield return null;
        }
        transform.localScale = baseScale;
    }
}
