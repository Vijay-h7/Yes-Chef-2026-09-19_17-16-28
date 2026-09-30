using System.Collections;
using TMPro;
using UnityEngine;

// Launch loading screen: shows the "Yes, Chef" artwork with a progress bar, then fades out.
// The splash screen waits for IsDone before it starts. Uses unscaled time (game clock is frozen in the menu).
public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform fill;      // stretched inside the bar track; its anchorMax.x is driven
    [SerializeField] private TMP_Text label;          // "Loading..."
    [SerializeField] private TMP_Text percent;        // "42%"
    [SerializeField] private float duration = 2.6f;
    [SerializeField] private float fadeOut = 0.45f;

    private static bool shownThisRun;

    public bool IsDone { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => shownThisRun = false;

    private void Awake()
    {
        if (shownThisRun) { IsDone = true; gameObject.SetActive(false); return; }
        shownThisRun = true;
        group.alpha = 1f;
        group.blocksRaycasts = true;
        SetProgress(0f, 0f);
    }

    private void Start()
    {
        if (!IsDone) StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = t / duration;
            // eased, with a small hesitation around 70% so it feels like real loading
            float p = Mathf.SmoothStep(0f, 1f, k);
            if (k > 0.62f && k < 0.74f) p = Mathf.Lerp(p, 0.72f, 0.7f);
            SetProgress(Mathf.Clamp01(p), t);
            yield return null;
        }
        SetProgress(1f, duration);
        yield return new WaitForSecondsRealtime(0.15f);

        for (float t = 0f; t < fadeOut; t += Time.unscaledDeltaTime)
        {
            group.alpha = 1f - t / fadeOut;
            yield return null;
        }
        IsDone = true;
        gameObject.SetActive(false);
    }

    private void SetProgress(float p, float time)
    {
        if (fill != null) fill.anchorMax = new Vector2(Mathf.Max(0.001f, p), 1f);
        if (percent != null) percent.text = Mathf.RoundToInt(p * 100f) + "%";
        if (label != null)
        {
            int dots = (int)(time * 3f) % 4;   // 0..3 visible dots; hidden ones keep the width so the text doesn't jitter
            label.text = "Loading" + new string('.', dots) + "<alpha=#00>" + new string('.', 3 - dots);
        }
    }
}
