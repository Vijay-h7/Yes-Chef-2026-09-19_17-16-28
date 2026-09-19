using UnityEngine;

public enum PrepType { None, Chop, Cook }

[CreateAssetMenu(menuName = "YesChef/Ingredient", fileName = "Ingredient_New")]
public class IngredientData : ScriptableObject
{
    public string displayName;
    public string shortLabel;          // "V", "C", "M" for order icons
    public int scoreValue;
    public PrepType prepType;
    public float prepTime;             // seconds to chop or cook (0 if none)
    public Color rawColor = Color.white;
    public Color preparedColor = Color.white;
}