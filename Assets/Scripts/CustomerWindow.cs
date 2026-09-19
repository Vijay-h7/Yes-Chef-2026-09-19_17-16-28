using System;
using UnityEngine;

public class CustomerWindow : MonoBehaviour, IInteractable
{
    public Order Current { get; private set; }

    public event Action<Order> OrderAssigned;   // null means "window is now empty"
    public event Action OrderProgressed;        // an ingredient was delivered
    public event Action<int> OrderCompleted;    // final score for the order

    public void AssignOrder(Order order)
    {
        Current = order;
        OrderAssigned?.Invoke(order);
    }

    private void Update()
    {
        Current?.Tick(Time.deltaTime);
    }

    public void Interact(PlayerHand hand)
    {
        if (hand.IsEmpty || Current == null) return;
        if (!Current.TryFulfil(hand.Held)) return;   // item stays in hand

        hand.Clear();
        OrderProgressed?.Invoke();

        if (!Current.IsComplete) return;

        int score = Current.Score;
        Current = null;
        OrderAssigned?.Invoke(null);
        OrderCompleted?.Invoke(score);
    }
}