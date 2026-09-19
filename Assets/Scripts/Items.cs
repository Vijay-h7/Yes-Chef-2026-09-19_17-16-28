using UnityEngine;

public class Item
{
    public IngredientData Data { get; }
    public bool IsPrepared { get; set; }

    // Cheese needs no prep, so it is always ready to serve.
    public bool IsReady => Data.prepType == PrepType.None || IsPrepared;
    public Color CurrentColor => IsPrepared ? Data.preparedColor : Data.rawColor;

    public Item(IngredientData data) { Data = data; }
}