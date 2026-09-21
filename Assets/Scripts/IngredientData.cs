using UnityEngine;

public enum PrepType { None, Chop, Cook }

[CreateAssetMenu(menuName = "YesChef/Ingredient", fileName = "Ingredient_New")]
public class IngredientData : ScriptableObject
{
    public string shortLabel = "?";   // shown on the order icons: V, C, M
    public int scoreValue;
    public PrepType prepType;
    public float prepTime;            // seconds to chop or cook (0 if none)
    public Color rawColor = Color.white;
    public Color preparedColor = Color.white;
}

// A runtime ingredient: what the chef holds, or what sits on a station.
public class Item
{
    public readonly IngredientData Data;
    public bool IsPrepared;

    public Item(IngredientData data) { Data = data; }

    // Cheese needs no prep, so it can be served straight away.
    public bool IsReady => Data.prepType == PrepType.None || IsPrepared;
    public Color CurrentColor => IsPrepared ? Data.preparedColor : Data.rawColor;
}
