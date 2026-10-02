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

    [Header("Burning (Stove-cooked items only)")]
    [Tooltip("Seconds an item can sit finished on the stove before it burns. 0 = never burns.")]
    public float burnTime = 6f;
    public Color burntColor = new Color(0.15f, 0.1f, 0.08f);

    [Header("3D Model (optional)")]
    [Tooltip("Real model shown raw / before prep. Leave empty to fall back to a plain colored cube.")]
    public GameObject rawModelPrefab;
    [Tooltip("Real model shown once prepared. Leave empty to reuse rawModelPrefab (e.g. cheese needs no change).")]
    public GameObject preparedModelPrefab;

    public GameObject GetModelPrefab(bool isPrepared)
    {
        if (isPrepared && preparedModelPrefab != null) return preparedModelPrefab;
        return rawModelPrefab;
    }

    [Header("Held-in-hand adjustment (optional)")]
    [Tooltip("Extra rotation (degrees) applied on top of the model's authored pose, ONLY while the chef is holding it in hand - not on the table/stove.")]
    public Vector3 heldExtraRotationEuler = Vector3.zero;

    [Tooltip("Position offset (in the hand anchor's local space, same numbers you would type in the Transform Position of the held model) applied ONLY while the chef holds it.")]
    public Vector3 heldLocalPositionOffset = Vector3.zero;

    [Tooltip("Name shown in the UI (falls back to the asset name).")]
    public string displayName;

    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;

    [Header("UI icon (optional)")]
    [Tooltip("Shown on the fridge menu (and anywhere else that wants it) instead of a plain color, if assigned.")]
    public Sprite icon;
}

// A runtime ingredient: what the chef holds, or what sits on a station.
public class Item
{
    public readonly IngredientData Data;
    public bool IsPrepared;
    public bool IsBurnt;

    public Item(IngredientData data) { Data = data; }

    // Cheese needs no prep, so it can be served straight away.
    public bool IsReady => (Data.prepType == PrepType.None || IsPrepared) && !IsBurnt;
    public bool CanBurn => Data.prepType == PrepType.Cook && Data.burnTime > 0f;
    public Color CurrentColor => IsBurnt ? Data.burntColor : (IsPrepared ? Data.preparedColor : Data.rawColor);
}
