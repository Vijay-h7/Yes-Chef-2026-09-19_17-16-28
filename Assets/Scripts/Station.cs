using UnityEngine;
using UnityEngine.UI;

public enum StationType { IngredientSource, Table, Stove, Trash }

public class Station : MonoBehaviour, IInteractable
{
    // One spot on a table or stove: what is on it, its timer, and its visuals.
    [System.Serializable]
    public class Slot
    {
        public Renderer itemVisual;
        public GameObject progressRoot;
        public Image progressFill;
        [System.NonSerialized] public Item item;
        [System.NonSerialized] public float timer;
        [System.NonSerialized] public float doneTimer;
        [System.NonSerialized] public float fxTimer;
        [System.NonSerialized] public ParticleSystem steam;
        [System.NonSerialized] public ParticleSystem smoke;
    }

    [SerializeField] private StationType type;
    [Tooltip("Only for IngredientSource: what this crate gives out.")]
    [SerializeField] private IngredientData ingredient;
    [Tooltip("Only for Table (1 slot) and Stove (2 slots).")]
    [SerializeField] private Slot[] slots;

    private AudioSource sfxSource;
    private bool sizzlePlaying;

    private void Start()
    {
        GameManager.Instance.GameStarted += ResetStation;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.spatialBlend = 1f;
        sfxSource.loop = true;
        sfxSource.volume = 0.45f;

        if (slots != null)
        {
            foreach (var slot in slots)
            {
                if (slot.itemVisual == null) continue;
                // Burnt/smoke FX only ever makes sense at the Stove. Table (chopping) slots
                // never get a smoke puff created at all, so chopping can never show it -
                // even if an ingredient's burn settings change later.
                if (type == StationType.Stove)
                {
                    slot.steam = FX.CreateLoopingPuff(slot.itemVisual.transform, Vector3.up * 0.15f,
                        new Color(1f, 1f, 1f, 0.35f), 0.12f, 0.5f);
                    slot.smoke = FX.CreateLoopingPuff(slot.itemVisual.transform, Vector3.up * 0.2f,
                        new Color(0.15f, 0.15f, 0.15f, 0.55f), 0.18f, 0.35f);
                }
            }
        }

        ResetStation();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.GameStarted -= ResetStation;
    }

    private void Update()
    {
        if (slots == null) return;
        bool anyCooking = false;
        foreach (var slot in slots)
        {
            TickSlot(slot);
            if (type == StationType.Stove && slot.item != null && !slot.item.IsPrepared) anyCooking = true;
        }

        if (type == StationType.Stove)
        {
            if (anyCooking && !sizzlePlaying)
            {
                sfxSource.clip = AudioFX.SizzleLoop;
                sfxSource.Play();
                sizzlePlaying = true;
            }
            else if (!anyCooking && sizzlePlaying)
            {
                sfxSource.Stop();
                sizzlePlaying = false;
            }
        }
    }

    public void Interact(Chef chef)
    {
        switch (type)
        {
            case StationType.IngredientSource:
                if (chef.IsHandEmpty && ingredient != null) chef.Pick(new Item(ingredient));
                break;

            case StationType.Trash:
                if (!chef.IsHandEmpty)
                {
                    AudioFX.PlayTrash(transform.position);
                    FX.Burst(transform.position + Vector3.up * 1f, new Color(0.35f, 0.35f, 0.35f), 10, 1.5f, 0.08f, 0.5f);
                }
                chef.Clear();
                break;

            case StationType.Table:
            case StationType.Stove:
                InteractWithSlots(chef);
                break;
        }
    }

    private void InteractWithSlots(Chef chef)
    {
        if (slots == null) return;
        var required = type == StationType.Table ? PrepType.Chop : PrepType.Cook;

        // Empty hand: pick up a finished (non-burnt) item first, otherwise a burnt one (so it can be trashed).
        if (chef.IsHandEmpty)
        {
            foreach (var slot in slots)
            {
                if (slot.item == null || !slot.item.IsPrepared || slot.item.IsBurnt) continue;
                chef.Pick(slot.item);
                ClearSlot(slot);
                return;
            }
            foreach (var slot in slots)
            {
                if (slot.item == null || !slot.item.IsBurnt) continue;
                chef.Pick(slot.item);
                ClearSlot(slot);
                return;
            }
            return;
        }

        // Holding a raw item of the right kind: put it in a free slot.
        var held = chef.Held;
        if (held.IsPrepared || held.Data.prepType != required) return;

        foreach (var slot in slots)
        {
            if (slot.item != null) continue;
            slot.item = chef.Take();
            slot.timer = 0f;
            slot.doneTimer = 0f;
            RefreshSlot(slot);
            if (type == StationType.Table) AudioFX.PlayChop(transform.position);
            return;
        }
    }

    private void TickSlot(Slot slot)
    {
        if (slot.item == null) return;

        if (!slot.item.IsPrepared)
        {
            slot.timer += Time.deltaTime;
            float duration = slot.item.Data.prepTime;
            slot.progressFill.fillAmount = Mathf.Clamp01(slot.timer / duration);

            if (type == StationType.Table)
            {
                slot.fxTimer += Time.deltaTime;
                if (slot.fxTimer >= 0.35f)
                {
                    slot.fxTimer = 0f;
                    FX.Burst(slot.itemVisual.transform.position, slot.item.Data.rawColor, 4, 0.8f, 0.04f, 0.35f);
                    AudioFX.PlayChop(slot.itemVisual.transform.position);
                }
            }

            if (type == StationType.Stove && slot.steam != null && !slot.steam.isPlaying) slot.steam.Play();

            if (slot.timer >= duration)
            {
                slot.item.IsPrepared = true;
                RefreshSlot(slot);
            }
            return;
        }

        if (slot.steam != null && slot.steam.isPlaying) slot.steam.Stop();

        if (slot.item.IsBurnt) return;

        if (type == StationType.Stove && slot.item.CanBurn)
        {
            slot.doneTimer += Time.deltaTime;
            if (slot.doneTimer >= slot.item.Data.burnTime)
            {
                slot.item.IsBurnt = true;
                RefreshSlot(slot);
                AudioFX.PlayBurn(slot.itemVisual.transform.position);
                if (slot.smoke != null) slot.smoke.Play();
            }
        }
    }

    private void ClearSlot(Slot slot)
    {
        slot.item = null;
        slot.timer = 0f;
        slot.doneTimer = 0f;
        slot.fxTimer = 0f;
        if (slot.steam != null) slot.steam.Stop();
        if (slot.smoke != null) slot.smoke.Stop();
        RefreshSlot(slot);
    }

    private void RefreshSlot(Slot slot)
    {
        slot.itemVisual.gameObject.SetActive(slot.item != null);
        slot.progressRoot.SetActive(slot.item != null && !slot.item.IsPrepared);
        slot.progressFill.fillAmount = 0f;
        if (slot.item != null) slot.itemVisual.material.color = slot.item.CurrentColor;
    }

    private void ResetStation()
    {
        if (slots == null) return;
        foreach (var slot in slots) ClearSlot(slot);
        if (sfxSource != null) sfxSource.Stop();
        sizzlePlaying = false;
    }
}
