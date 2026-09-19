using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Order
{
    private readonly IngredientData[] required;
    private readonly bool[] filled;

    public IReadOnlyList<IngredientData> Required => required;
    public float ElapsedTime { get; private set; }

    public bool IsComplete => filled.All(f => f);
    public bool IsFilled(int index) => filled[index];

    // Sum of ingredient values minus whole seconds elapsed (rounded down). Can go negative.
    public int Score => required.Sum(i => i.scoreValue) - Mathf.FloorToInt(ElapsedTime);

    public Order(IEnumerable<IngredientData> ingredients)
    {
        required = ingredients.ToArray();
        filled = new bool[required.Length];
    }

    public void Tick(float deltaTime) => ElapsedTime += deltaTime;

    // Accepts an item only if it is prepared and the order still needs that ingredient.
    public bool TryFulfil(Item item)
    {
        if (!item.IsReady) return false;

        for (int i = 0; i < required.Length; i++)
        {
            if (filled[i] || required[i] != item.Data) continue;
            filled[i] = true;
            return true;
        }
        return false;
    }
}