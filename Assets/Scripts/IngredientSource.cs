using UnityEngine;

public class IngredientSource : MonoBehaviour, IInteractable
{
    [SerializeField] private IngredientData ingredient;

    public void Interact(PlayerHand hand)
    {
        if (hand.IsEmpty) hand.Pick(new Item(ingredient));
    }
}