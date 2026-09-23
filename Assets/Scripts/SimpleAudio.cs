using UnityEngine;

public static class SimpleAudio
{
    private static AudioClip ding, clatter, sizzleLoop, chopLoop;

    public static void Ding(Vector3 pos)
    {
        if (ding == null) ding = Chime(880f, 1320f, 0.35f);
        AudioSource.PlayClipAtPoint(ding, pos, 0.6f);
    }

    public static void Clatter(Vector3 pos)
    {
        if (clatter == null) clatter = NoiseBurst(0.18f);
        AudioSource.PlayClipAtPoint(clatter, pos, 0.5f);
    }

    public static AudioClip SizzleLoop()
    {
        if (sizzleLoop == null) sizzleLoop = NoiseLoop(1.0f, 0.12f);
        return sizzleLoop;
    }

    public static AudioClip ChopLoop()
    {
        if (chopLoop == null) chopLoop = TickLoop(0.5f, 520f);
        return chopLoop;
    }

    static AudioClip Chime(float f1, float f2, float dur)
    {
        int sr = 44100;
        int n = Mathf.CeilToInt(sr * dur);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)sr;
            float env = Mathf.Exp(-t * 6f);
            float s = Mathf.Sin(2 * Mathf.PI * f1 * t) * 0.6f + Mathf.Sin(2 * Mathf.PI * f2 * t) * 0.4f;
            data[i] = s * env * 0.5f;
        }
        var clip = AudioClip.Create("Ding", n, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip NoiseBurst(float dur)
    {
        int sr = 44100;
        int n = Mathf.CeilToInt(sr * dur);
        var data = new float[n];
        var rng = new System.Random(1234);
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)sr;
            float env = Mathf.Exp(-t * 18f);
            data[i] = ((float)rng.NextDouble() * 2f - 1f) * env * 0.5f;
        }
        var clip = AudioClip.Create("Clatter", n, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip NoiseLoop(float dur, float amp)
    {
        int sr = 44100;
        int n = Mathf.CeilToInt(sr * dur);
        var data = new float[n];
        var rng = new System.Random(99);
        float prev = 0f;
        for (int i = 0; i < n; i++)
        {
            float white = (float)rng.NextDouble() * 2f - 1f;
            prev = prev * 0.92f + white * 0.08f;
            data[i] = prev * amp;
        }
        var clip = AudioClip.Create("Sizzle", n, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip TickLoop(float dur, float freq)
    {
        int sr = 44100;
        int n = Mathf.CeilToInt(sr * dur);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)sr;
            float env = Mathf.Exp(-(t % 0.25f) * 40f);
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * env * 0.35f;
        }
        var clip = AudioClip.Create("Chop", n, 1, sr, false);
        clip.SetData(data, 0);
        return clip;
    }
}
