using UnityEngine;

public static class FX
{
    private static Material _mat;

    private static Material GetMat()
    {
        if (_mat == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            _mat = new Material(shader);
        }
        return _mat;
    }

    public static ParticleSystem CreateLoopingPuff(Transform parent, Vector3 localPos, Color color, float size, float speed)
    {
        var go = new GameObject("PuffFX");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.startLifetime = 1.1f;
        main.startSpeed = speed;
        main.startSize = size;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.05f;

        var emission = ps.emission;
        emission.rateOverTime = 10f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.06f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new[] { new GradientAlphaKey(color.a, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 2.2f));

        ps.GetComponent<ParticleSystemRenderer>().material = GetMat();
        ps.Stop();
        return ps;
    }

    public static void Burst(Vector3 worldPos, Color color, int count, float speed, float size, float life)
    {
        var go = new GameObject("BurstFX");
        go.transform.position = worldPos;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.startLifetime = life;
        main.startSpeed = speed;
        main.startSize = size;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;

        ps.GetComponent<ParticleSystemRenderer>().material = GetMat();
        ps.Play();
        Object.Destroy(go, life + 0.5f);
    }

    public static void ConfettiBurst(Vector3 worldPos)
    {
        Color a = Color.yellow;
        Color b = Color.red;
        var go = new GameObject("ConfettiFX");
        go.transform.position = worldPos;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.startLifetime = 1.2f;
        main.startSpeed = 3.5f;
        main.startSize = 0.1f;
        main.startColor = new ParticleSystem.MinMaxGradient(a, b);
        main.gravityModifier = 1.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        shape.angle = 45f;

        ps.GetComponent<ParticleSystemRenderer>().material = GetMat();
        ps.Play();
        Object.Destroy(go, 2f);
    }
}
