using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Full-screen "Yes, Chef!" splash shown once when the game launches (about 2.5 seconds),
// then it fades out to reveal the start menu underneath. Click or press any key to skip.
// Uses unscaled time because the game clock is frozen (timeScale 0) while in the menu.
public class SplashScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private RectTransform logo;
    [SerializeField] private LoadingScreen loading;   // splash starts only after this has finished
    [SerializeField] private float fadeIn = 0.45f;
    [SerializeField] private float hold = 1.9f;
    [SerializeField] private float fadeOut = 0.55f;

    private static bool shownThisRun;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => shownThisRun = false;

    private void Awake()
    {
        if (shownThisRun) { gameObject.SetActive(false); return; }
        shownThisRun = true;
        group.alpha = 0f;
        group.blocksRaycasts = true;   // menu buttons can't be clicked through the splash
    }

    private void Start()
    {
        if (gameObject.activeSelf) StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (loading != null && !loading.IsDone) yield return null;

        // fade in + gentle pop of the logo
        for (float t = 0f; t < fadeIn; t += Time.unscaledDeltaTime)
        {
            float k = t / fadeIn;
            group.alpha = k;
            if (logo != null) logo.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, 1f - (1f - k) * (1f - k));
            yield return null;
        }
        group.alpha = 1f;

        // hold (slow zoom), skippable
        for (float t = 0f; t < hold; t += Time.unscaledDeltaTime)
        {
            if (logo != null) logo.localScale = Vector3.one * (1f + 0.04f * (t / hold));
            if (SkipPressed()) break;
            yield return null;
        }

        // fade out to the start menu
        for (float t = 0f; t < fadeOut; t += Time.unscaledDeltaTime)
        {
            group.alpha = 1f - t / fadeOut;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    private static bool SkipPressed()
    {
        return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
    }
}
