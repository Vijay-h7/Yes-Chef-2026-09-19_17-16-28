using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private CustomerWindow[] windows;

    private void Start()
    {
        GameManager.Instance.GameStarted += OnGameStarted;

        foreach (var window in windows)
        {
            var w = window;   // capture for the lambda
            w.OrderCompleted += score =>
            {
                ScoreManager.Instance.Add(score);
                StartCoroutine(RespawnAfterDelay(w));
            };
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.GameStarted -= OnGameStarted;
    }

    private void OnGameStarted()
    {
        StopAllCoroutines();   // cancel respawns left over from the previous round
        foreach (var window in windows) window.AssignOrder(GenerateOrder());
    }

    // WaitForSeconds uses scaled time, so pausing also pauses the respawn delay.
    private IEnumerator RespawnAfterDelay(CustomerWindow window)
    {
        yield return new WaitForSeconds(config.orderRespawnDelay);
        window.AssignOrder(GenerateOrder());
    }

    private Order GenerateOrder()
    {
        int count = Random.value < config.threeIngredientChance ? 3 : 2;
        var list = new List<IngredientData>(count);
        for (int i = 0; i < count; i++)
            list.Add(config.ingredients[Random.Range(0, config.ingredients.Length)]);
        return new Order(list);
    }
}