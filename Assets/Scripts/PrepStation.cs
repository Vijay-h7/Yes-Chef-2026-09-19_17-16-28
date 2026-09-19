using UnityEngine;

public class PrepStation : MonoBehaviour, IInteractable
{
    [SerializeField] private PrepType acceptedPrep;   // Chop for table, Cook for stove
    [SerializeField] private PrepSlot[] slots;

    public void Interact(PlayerHand hand)
    {
        if (hand.IsEmpty) TakeFinished(hand);
        else TryPlace(hand);
    }

    private void TryPlace(PlayerHand hand)
    {
        var held = hand.Held;
        if (held.IsPrepared || held.Data.prepType != acceptedPrep) return;

        foreach (var slot in slots)
        {
            if (!slot.IsEmpty) continue;
            slot.Begin(hand.Take());
            return;
        }
    }

    private void TakeFinished(PlayerHand hand)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsDone) continue;
            hand.Pick(slot.Take());
            return;
        }
    }
}