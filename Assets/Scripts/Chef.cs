using UnityEngine;
using UnityEngine.InputSystem;

// Anything the chef can press E on implements this.
public interface IInteractable
{
    void Interact(Chef chef);
}

[RequireComponent(typeof(CharacterController))]
public class Chef : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float turnSpeed = 15f;
    [SerializeField] private float interactRadius = 1.6f;
    [SerializeField] private Renderer heldVisual;   // the HeldItem sphere

    public Item Held { get; private set; }
    public bool IsHandEmpty => Held == null;

    private CharacterController controller;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        RefreshHeldVisual();
    }

    // Subscribe in Start (not Awake/OnEnable) so GameManager.Instance already exists.
    private void Start() => GameManager.Instance.GameStarted += ResetChef;

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.GameStarted -= ResetChef;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsPlaying) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        Move(kb);
        if (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
            FindNearest()?.Interact(this);
    }

    // ---- Hand ----
    public void Pick(Item item) { Held = item; RefreshHeldVisual(); }

    public Item Take()
    {
        var item = Held;
        Held = null;
        RefreshHeldVisual();
        return item;
    }

    public void Clear() => Take();

    private void RefreshHeldVisual()
    {
        if (heldVisual == null) return;
        heldVisual.gameObject.SetActive(Held != null);
        ItemVisualUtil.Apply(heldVisual.transform, heldVisual, Held);
    }

    // ---- Movement ----
    private void Move(Keyboard kb)
    {
        float x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1 : 0)
                - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1 : 0);
        float z = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1 : 0)
                - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1 : 0);

        var dir = new Vector3(x, 0f, z).normalized;
        if (dir == Vector3.zero) return;

        controller.Move(dir * (moveSpeed * Time.deltaTime));
        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
    }

    // ---- Interaction ----
    private IInteractable FindNearest()
    {
        IInteractable best = null;
        float bestDistance = float.MaxValue;

        foreach (var hit in Physics.OverlapSphere(transform.position, interactRadius))
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

    // ---- Restart ----
    private void ResetChef()
    {
        // A CharacterController overrides teleports unless it is disabled first.
        controller.enabled = false;
        transform.SetPositionAndRotation(startPosition, startRotation);
        controller.enabled = true;
        Clear();
    }
}
