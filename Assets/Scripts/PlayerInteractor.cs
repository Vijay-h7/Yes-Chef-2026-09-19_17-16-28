using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private PlayerHand hand;
    [SerializeField] private float radius = 1.6f;

    private void Update()
    {
        if (!GameManager.Instance.IsPlaying) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
            FindNearest()?.Interact(hand);
    }

    private IInteractable FindNearest()
    {
        IInteractable best = null;
        float bestDistance = float.MaxValue;

        foreach (var hit in Physics.OverlapSphere(transform.position, radius))
        {
            var interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float distance = (hit.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = interactable;
            }
        }
        return best;
    }
}