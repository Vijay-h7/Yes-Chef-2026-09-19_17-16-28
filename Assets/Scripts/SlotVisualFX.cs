using UnityEngine;

public enum SlotFxMode { Steam, Dust }

public class SlotVisualFX : MonoBehaviour
{
    [SerializeField] private SlotFxMode mode = SlotFxMode.Steam;

    private Renderer itemVisual;
    private GameObject progressRoot;
    private ParticleSystem fx;
    private AudioSource audioSource;

    private static readonly Color BurntColor = new Color(0.12f, 0.11f, 0.10f);

    private enum State { Idle, Prepping, Burning }
    private State current = State.Idle;

    private void Awake()
    {
        var prepared = transform.Find("PreparedModel");
        itemVisual = prepared != null ? prepared.GetComponentInChildren<Renderer>() : transform.Find("Item")?.GetComponent<Renderer>();
        progressRoot = transform.Find("Progress")?.gameObject;
        fx = GetComponentInChildren<ParticleSystem>(true);
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.volume = 0.3f;
            audioSource.clip = mode == SlotFxMode.Steam ? SimpleAudio.SizzleLoop() : SimpleAudio.ChopLoop();
        }
    }

    private void Update()
    {
        var next = State.Idle;
        if (itemVisual != null && itemVisual.gameObject.activeSelf)
        {
            bool burnt = itemVisual.material.color == BurntColor;
            if (burnt) next = State.Burning;
        }
        else if (progressRoot != null && progressRoot.activeSelf)
        {
            next = State.Prepping;
        }

        if (next == current) return;
        current = next;

        if (fx != null)
        {
            var main = fx.main;
            if (current == State.Prepping)
            {
                main.startColor = mode == SlotFxMode.Steam
                    ? new Color(1f, 1f, 1f, 0.5f)
                    : new Color(0.6f, 0.45f, 0.25f, 0.8f);
                fx.Play();
            }
            else if (current == State.Burning)
            {
                main.startColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);
                fx.Play();
            }
            else
            {
                fx.Stop();
            }
        }

        if (audioSource != null)
        {
            if (current == State.Prepping) audioSource.Play();
            else audioSource.Stop();
        }
    }
}
