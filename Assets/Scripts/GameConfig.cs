using UnityEngine;

[CreateAssetMenu(menuName = "YesChef/Game Config", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Game")]
    public float gameDuration = 180f;
    public float orderRespawnDelay = 5f;

    [Header("Orders")]
    [Range(0f, 1f)] public float threeIngredientChance = 0.5f;
    public IngredientData[] ingredients;

    [Header("Player")]
    public float playerSpeed = 6f;
}