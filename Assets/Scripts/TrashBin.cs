using UnityEngine;

public class TrashBin : MonoBehaviour, IInteractable
{
    public void Interact(PlayerHand hand) => hand.Clear();
}