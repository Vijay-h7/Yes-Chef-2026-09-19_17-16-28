using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private PlayerHand hand;
    [SerializeField] private float turnSpeed = 15f;

    private CharacterController controller;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    // Subscribe in Start (not OnEnable) so GameManager.Instance is guaranteed to exist.
    private void Start() => GameManager.Instance.GameStarted += ResetPlayer;

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.GameStarted -= ResetPlayer;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsPlaying) return;

        Vector3 dir = ReadInput();
        if (dir == Vector3.zero) return;

        controller.Move(dir * (config.playerSpeed * Time.deltaTime));
        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
    }

    private static Vector3 ReadInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return Vector3.zero;

        float x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1 : 0)
                - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1 : 0);
        float z = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1 : 0)
                - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1 : 0);
        return new Vector3(x, 0f, z).normalized;
    }

    private void ResetPlayer()
    {
        // A CharacterController overrides teleports unless it is disabled first.
        controller.enabled = false;
        transform.SetPositionAndRotation(startPosition, startRotation);
        controller.enabled = true;
        hand.Clear();
    }
}