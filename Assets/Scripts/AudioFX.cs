using UnityEngine;

public static class AudioFX
{
    private static AudioClip _chop;
    private static AudioClip _sizzle;
    private static AudioClip _ding;
    private static AudioClip _trash;
    private static AudioClip _warn;
    private static AudioClip _miss;
    private static AudioClip _burn;

    private const int SampleRate = 44100;

    public static void PlayChop(Vector3 pos) => PlayOneShot(GetChop(), pos, 0.5f);
    public static void PlayDing(Vector3 pos) => PlayOneShot(GetDing(), pos, 0.8f);
    public static void PlayTrash(Vector3 pos) => PlayOneShot(GetTrash(), pos, 0.7f);
    public static void PlayWarn(Vector3 pos) => PlayOneShot(GetWarn(), pos, 0.45f);
    public static void PlayMiss(Vector3 pos) => PlayOneShot(GetMiss(), pos, 0.7f);
    public static void PlayBurn(Vector3 pos) => PlayOneShot(GetBurn(), pos, 0.6f);

    public static AudioClip SizzleLoop => GetSizzle();

    private static void PlayOneShot(AudioClip clip, Vector3 pos, float vol)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, pos, vol);
    }

    private static AudioClip GetChop()
    {
        if (_chop == null)
            _chop = BuildTone("chop", 0.07f, t => Mathf.Sin(2f * Mathf.PI * 1400f * t) * Mathf.Exp(-t * 45f));
        return _chop;
    }

    private static AudioClip GetDing()
    {
        if (_ding == null)
            _ding = BuildTone("ding", 0.5f, t =>
                (Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * 1318.5f * t) * 0.4f)
                * Mathf.Exp(-t * 4.5f));
        return _ding;
    }

    private static AudioClip GetTrash()
    {
        if (_trash == null)
            _trash = BuildTone("trash", 0.18f, t =>
                (Mathf.Sin(2f * Mathf.PI * 160f * t) + (Random.value * 2f - 1f) * 0.3f) * Mathf.Exp(-t * 18f));
        return _trash;
    }

    private static AudioClip GetWarn()
    {
        if (_warn == null)
            _warn = BuildTone("warn", 0.09f, t => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 1000f * t)) * 0.3f * Mathf.Exp(-t * 20f));
        return _warn;
    }

    private static AudioClip GetMiss()
    {
        if (_miss == null)
            _miss = BuildTone("miss", 0.4f, t =>
                Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(220f, 110f, t / 0.4f) * t) * Mathf.Exp(-t * 4f));
        return _miss;
    }

    private static AudioClip GetBurn()
    {
        if (_burn == null)
            _burn = BuildTone("burn", 0.35f, t => (Random.value * 2f - 1f) * 0.25f * Mathf.Exp(-t * 6f));
        return _burn;
    }

    private static AudioClip GetSizzle()
    {
        if (_sizzle == null)
        {
            int samples = SampleRate;
            var clip = AudioClip.Create("sizzle", samples, 1, SampleRate, false);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SampleRate;
                float noise = Random.value * 2f - 1f;
                float mod = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 6f * t);
                data[i] = noise * 0.18f * mod;
            }
            clip.SetData(data, 0);
            _sizzle = clip;
        }
        return _sizzle;
    }

    private delegate float Waveform(float t);

    private static AudioClip BuildTone(string name, float duration, Waveform wave)
    {
        int samples = Mathf.CeilToInt(SampleRate * duration);
        var clip = AudioClip.Create(name, samples, 1, SampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)SampleRate;
            data[i] = Mathf.Clamp(wave(t), -1f, 1f);
        }
        clip.SetData(data, 0);
        return clip;
    }
}
