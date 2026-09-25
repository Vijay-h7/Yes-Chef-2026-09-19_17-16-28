using System.Collections.Generic;
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
    [Header("3D Model Replacement")]
    [Tooltip("Drag any imported 3D character FBX model or prefab here from Assets/Models/ or Assets/Prefabs/")]
    [SerializeField] private GameObject customChefModelPrefab;

    public Item Held { get; private set; }
    public bool IsHandEmpty => Held == null;

    private CharacterController controller;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private Vector3 originalScale = Vector3.one;
    private float squashTimer = 0f;
    private float walkBobTime = 0f;
    private float flusteredTimer = 0f;
    private GameObject activeChefModel;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        originalScale = transform.localScale;

        // Completely disable old primitive capsule MeshRenderer and MeshFilter on the root Chef object
        MeshRenderer rootRenderer = GetComponent<MeshRenderer>();
        if (rootRenderer != null) rootRenderer.enabled = false;

        MeshFilter rootFilter = GetComponent<MeshFilter>();
        if (rootFilter != null) rootFilter.sharedMesh = null;

        // Bind Mr. Chef 3D model
        SetupChefCharacterModel();

        RefreshHeldVisual();
    }

    private void SetupChefCharacterModel()
    {
        // 1. First check if Mr.Chef Player model or visible chef object is already present as a child transform in the Scene
        foreach (Transform child in transform)
        {
            if (child != heldVisual?.transform && (child.name.Contains("Mr") || child.name.Contains("Chef") || child.name.Contains("Cube")))
            {
                activeChefModel = child.gameObject;
                activeChefModel.name = "Mr_Chef_PlayerModel";
                break;
            }
        }

        // 2. Remove any other unexpected legacy child objects
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in transform)
        {
            if (child != heldVisual?.transform && child.gameObject != activeChefModel)
            {
                toDestroy.Add(child.gameObject);
            }
        }
        foreach (var obj in toDestroy) Destroy(obj);

        // 3. If no existing child model found, instantiate customChefModelPrefab assigned in Inspector
        if (activeChefModel == null && customChefModelPrefab != null)
        {
            activeChefModel = Instantiate(customChefModelPrefab, transform);
            activeChefModel.transform.localPosition = Vector3.zero;
            activeChefModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // model's front faces -Z as authored
            activeChefModel.name = "Mr_Chef_PlayerModel";
        }

        // 4. Auto-load Mr.Chef Player from Assets/Models/ or Assets/Prefabs/
        if (activeChefModel == null)
        {
#if UNITY_EDITOR
            GameObject fbxModel = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Mr.Chef Player.fbx");
            if (fbxModel == null) fbxModel = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mr.Chef Player.prefab");
            if (fbxModel == null) fbxModel = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/chef.fbx");
            if (fbxModel == null) fbxModel = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Chef_Male.fbx");
            if (fbxModel != null)
            {
                activeChefModel = Instantiate(fbxModel, transform);
                activeChefModel.transform.localPosition = Vector3.zero;
                activeChefModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // model's front faces -Z as authored
                activeChefModel.transform.localScale = Vector3.one;
                activeChefModel.name = "Mr_Chef_PlayerModel";
            }
#endif
        }

        // Connect HeldItemAnchor to hand, normalize height, and clean up child colliders/rigidbodies
        if (activeChefModel != null)
        {
            foreach (var rb in activeChefModel.GetComponentsInChildren<Rigidbody>()) Destroy(rb);
            foreach (var col in activeChefModel.GetComponentsInChildren<Collider>()) Destroy(col);

            activeChefModel.transform.localPosition = Vector3.zero;
            activeChefModel.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // model's front faces -Z as authored

            // Normalize model scale so character height is ~1.8m tall in world space
            Renderer[] renderers = activeChefModel.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds combinedBounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    combinedBounds.Encapsulate(renderers[i].bounds);
                }
                // Fixed model scale (requested: 0.75), instead of auto-normalizing to a target height.
                activeChefModel.transform.localScale = Vector3.one * 0.75f;

                // Ground the model: after scaling, its feet may sit above or below y = 0. Shift it down/up so they touch the floor.
                Renderer[] groundingRenderers = activeChefModel.GetComponentsInChildren<Renderer>();
                if (groundingRenderers.Length > 0)
                {
                    Bounds groundedBounds = groundingRenderers[0].bounds;
                    for (int i = 1; i < groundingRenderers.Length; i++) groundedBounds.Encapsulate(groundingRenderers[i].bounds);
                    float feetOffset = groundedBounds.min.y - transform.position.y;
                    activeChefModel.transform.localPosition -= new Vector3(0f, feetOffset, 0f);
                }
            }

            Transform anchor = activeChefModel.transform.Find("HeldItemAnchor");
            if (anchor == null)
            {
                var newAnchor = new GameObject("HeldItemAnchor");
                newAnchor.transform.SetParent(activeChefModel.transform, false);
                newAnchor.transform.localPosition = new Vector3(0f, 1.0f, 0.55f);
                anchor = newAnchor.transform;
            }

            if (heldVisual != null && anchor != null)
            {
                heldVisual.transform.position = anchor.position;
                heldVisual.transform.SetParent(anchor, true);
            }
        }

        // Configure CharacterController collision bounds so feet sit on ground
        if (controller != null)
        {
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.height = 1.8f;
            controller.radius = 0.4f;
        }
    }

    // Subscribe in Start (not Awake/OnEnable) so GameManager.Instance already exists.
    private void Start() => GameManager.Instance.GameStarted += ResetChef;

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.GameStarted -= ResetChef;
    }

    private void Update()
    {
        UpdateJuiceAnimations();

        if (!GameManager.Instance.IsPlaying) return;
        if (FridgeMenu.IsOpen) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        Move(kb);
        if (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
            FindNearest()?.Interact(this);
    }

    // ---- Hand ----
    public void Pick(Item item)
    {
        Held = item;
        TriggerPickSquash();
        RefreshHeldVisual();
    }

    public Item Take()
    {
        var item = Held;
        Held = null;
        TriggerPickSquash();
        RefreshHeldVisual();
        return item;
    }

    public void Clear() => Take();

    public void TriggerFlustered()
    {
        flusteredTimer = 0.65f;
        // Angry steam puff rising from chef hat top
        FX.Burst(transform.position + Vector3.up * 2.6f, new Color(0.9f, 0.2f, 0.15f, 0.8f), 15, 1.8f, 0.12f, 0.6f);
    }

    private void TriggerPickSquash()
    {
        squashTimer = 0.25f;
    }

    private void UpdateJuiceAnimations()
    {
        if (activeChefModel == null) return;

        // Squash & Stretch Spring applied to active 3D model
        Vector3 targetScale = Vector3.one;
        if (squashTimer > 0f)
        {
            squashTimer -= Time.deltaTime;
            float progress = squashTimer / 0.25f;
            float scaleY = 1.0f + Mathf.Sin(progress * Mathf.PI) * 0.35f;
            float scaleXZ = 1.0f - Mathf.Sin(progress * Mathf.PI) * 0.2f;
            targetScale = new Vector3(scaleXZ, scaleY, scaleXZ);
        }

        // Flustered Angry Shake
        if (flusteredTimer > 0f)
        {
            flusteredTimer -= Time.deltaTime;
            float shakeX = Mathf.Sin(Time.time * 40f) * 0.15f;
            activeChefModel.transform.localScale = targetScale + new Vector3(shakeX, -shakeX * 0.5f, shakeX);
        }
        else
        {
            activeChefModel.transform.localScale = Vector3.Lerp(activeChefModel.transform.localScale, targetScale, Time.deltaTime * 12f);
        }
    }

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

        // Constant gravity ensures player stays firmly grounded
        Vector3 moveVelocity = dir * moveSpeed;
        moveVelocity.y = -9.81f;
        controller.Move(moveVelocity * Time.deltaTime);

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);

            // Visual-only walking bobbing applied to child model
            if (activeChefModel != null)
            {
                walkBobTime += Time.deltaTime * 14f;
                float bobOffset = Mathf.Abs(Mathf.Sin(walkBobTime)) * 0.04f;
                activeChefModel.transform.localPosition = new Vector3(0f, bobOffset, 0f);
            }
        }
        else
        {
            if (activeChefModel != null)
            {
                activeChefModel.transform.localPosition = Vector3.Lerp(activeChefModel.transform.localPosition, Vector3.zero, Time.deltaTime * 10f);
            }
        }
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

