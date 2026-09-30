using UnityEngine;

// Creates a Cozy Cartoon Lakeside Bistro environment around the kitchen scene.
[DefaultExecutionOrder(-100)]
public class BistroEnvironmentBuilder : MonoBehaviour
{
    private static bool _initialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSetup()
    {
        if (_initialized) return;

        // The exterior environment (deck, tables, lights, plants, fence) has been baked into the
        // scene as real, persistent GameObjects/prefabs under "BistroEnvironment", visible in the
        // Hierarchy at all times. Skip runtime generation entirely when that's present, so we don't
        // spawn a duplicate, invisible-in-the-editor copy on top of it.
        if (GameObject.Find("BistroEnvironment") != null)
        {
            _initialized = true;
            return;
        }

        var builder = FindAnyObjectByType<BistroEnvironmentBuilder>();
        if (builder == null)
        {
            var go = new GameObject("BistroEnvironmentBuilder");
            builder = go.AddComponent<BistroEnvironmentBuilder>();
        }
    }

    private void Awake()
    {
        _initialized = true;
        SetupLighting();
        BuildDeck();
        BuildTablesAndChairs();
        BuildStringLights();
        BuildPottedPlants();
        BuildMenuBoard();
        BuildFence();
    }

    private void SetupLighting()
    {
        // Directional Sunset Light
        Light dirLight = FindAnyObjectByType<Light>();
        if (dirLight == null || dirLight.type != LightType.Directional)
        {
            GameObject lightGo = new GameObject("SunsetDirectionalLight");
            dirLight = lightGo.AddComponent<Light>();
            dirLight.type = LightType.Directional;
        }

        dirLight.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
        dirLight.color = new Color(1.0f, 0.76f, 0.52f); // Warm Golden Sunset
        dirLight.intensity = 1.35f;
        dirLight.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(1.0f, 0.82f, 0.65f);
        RenderSettings.ambientEquatorColor = new Color(0.85f, 0.65f, 0.45f);
        RenderSettings.ambientGroundColor = new Color(0.45f, 0.32f, 0.22f);
    }

    private void BuildDeck()
    {
        GameObject deckRoot = new GameObject("BistroDeck");
        deckRoot.transform.position = Vector3.zero;

        // The deck must stay OUTSIDE the kitchen's own walls (outer wall faces),
        // not underlap the interior - a single slab under everything used to
        // z-fight with the interior floor and read as "inside the walls".
        const float wallOuterHalfWidth = 11f;  // Wall_East/West outer faces
        const float wallOuterHalfDepth = 7f;   // Wall_North/South outer faces
        const float deckOuterHalfWidth = 14f;
        const float deckOuterHalfDepth = 12f;
        const float deckY = -0.2f;
        const float deckThickness = 0.4f;

        void MakeSlab(string slabName, Vector3 center, Vector3 size)
        {
            GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deck.name = slabName;
            deck.transform.SetParent(deckRoot.transform, false);
            deck.transform.position = center;
            deck.transform.localScale = size;
            SetColor(deck, new Color(0.48f, 0.32f, 0.18f)); // Warm Wood
        }

        // North/South strips run the full outer width and also cover the corners.
        float nsDepth = deckOuterHalfDepth - wallOuterHalfDepth;
        float nsCenterZ = wallOuterHalfDepth + nsDepth / 2f;
        MakeSlab("DeckFloor_North", new Vector3(0f, deckY, nsCenterZ), new Vector3(deckOuterHalfWidth * 2f, deckThickness, nsDepth));
        MakeSlab("DeckFloor_South", new Vector3(0f, deckY, -nsCenterZ), new Vector3(deckOuterHalfWidth * 2f, deckThickness, nsDepth));

        // East/West strips fill the remaining band between the North/South strips.
        float ewWidth = deckOuterHalfWidth - wallOuterHalfWidth;
        float ewCenterX = wallOuterHalfWidth + ewWidth / 2f;
        MakeSlab("DeckFloor_East", new Vector3(ewCenterX, deckY, 0f), new Vector3(ewWidth, deckThickness, wallOuterHalfDepth * 2f));
        MakeSlab("DeckFloor_West", new Vector3(-ewCenterX, deckY, 0f), new Vector3(ewWidth, deckThickness, wallOuterHalfDepth * 2f));
    }

    private void BuildTablesAndChairs()
    {
        GameObject tablesRoot = new GameObject("BistroDiningTables");

        Vector3[] tablePositions = new Vector3[]
        {
            new Vector3(-8.5f, 0f, 9f),
            new Vector3(-8.5f, 0f, -9f),
            new Vector3(8.5f, 0f, 9f),
            new Vector3(8.5f, 0f, -9f)
        };

        foreach (var pos in tablePositions)
        {
            GameObject tableGroup = new GameObject("DiningSet");
            tableGroup.transform.SetParent(tablesRoot.transform, false);
            tableGroup.transform.position = pos;

            // Table top
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            top.transform.SetParent(tableGroup.transform, false);
            top.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            top.transform.localScale = new Vector3(1.6f, 0.06f, 1.6f);
            SetColor(top, new Color(0.85f, 0.58f, 0.35f)); // Warm Wood Top

            // Table leg
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.transform.SetParent(tableGroup.transform, false);
            leg.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            leg.transform.localScale = new Vector3(0.18f, 0.42f, 0.18f);
            SetColor(leg, new Color(0.2f, 0.2f, 0.22f)); // Dark Metal Base

            // 2 Chairs per table
            for (int i = 0; i < 2; i++)
            {
                float angle = i * 180f;
                Vector3 chairOffset = Quaternion.Euler(0, angle, 0) * new Vector3(0f, 0f, 1.2f);
                GameObject chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chair.transform.SetParent(tableGroup.transform, false);
                chair.transform.localPosition = chairOffset + new Vector3(0f, 0.4f, 0f);
                chair.transform.localScale = new Vector3(0.6f, 0.45f, 0.6f);
                chair.transform.rotation = Quaternion.Euler(0, angle + 180f, 0);
                SetColor(chair, new Color(0.18f, 0.48f, 0.48f)); // Teal Accent Chair
            }
        }
    }

    private void BuildStringLights()
    {
        GameObject lightsRoot = new GameObject("StringLights");

        Vector3[] postPositions = new Vector3[]
        {
            new Vector3(-12f, 0f, -9f),
            new Vector3(-12f, 0f, 9f),
            new Vector3(12f, 0f, 9f),
            new Vector3(12f, 0f, -9f)
        };

        foreach (var pos in postPositions)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.transform.SetParent(lightsRoot.transform, false);
            post.transform.position = pos + new Vector3(0f, 2.2f, 0f);
            post.transform.localScale = new Vector3(0.15f, 2.2f, 0.15f);
            SetColor(post, new Color(0.35f, 0.24f, 0.15f));

            // Glowing Light Bulb atop post
            GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.transform.SetParent(post.transform, false);
            bulb.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            bulb.transform.localScale = new Vector3(2.2f, 0.15f, 2.2f);
            SetColor(bulb, new Color(1.0f, 0.92f, 0.55f));

            Light ptLight = bulb.AddComponent<Light>();
            ptLight.type = LightType.Point;
            ptLight.color = new Color(1.0f, 0.85f, 0.5f);
            ptLight.range = 8f;
            ptLight.intensity = 1.2f;
        }
    }

    private void BuildPottedPlants()
    {
        GameObject plantRoot = new GameObject("PottedPlants");

        Vector3[] plantPositions = new Vector3[]
        {
            new Vector3(-6f, 0f, -8f),
            new Vector3(6f, 0f, -8f),
            new Vector3(-12.5f, 0f, 0f),
            new Vector3(12.5f, 0f, 0f)
        };

        foreach (var pos in plantPositions)
        {
            GameObject pot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pot.transform.SetParent(plantRoot.transform, false);
            pot.transform.position = pos + new Vector3(0f, 0.35f, 0f);
            pot.transform.localScale = new Vector3(0.7f, 0.35f, 0.7f);
            SetColor(pot, new Color(0.85f, 0.35f, 0.22f)); // Terracotta Pot

            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.transform.SetParent(pot.transform, false);
            foliage.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            foliage.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            SetColor(foliage, new Color(0.22f, 0.65f, 0.32f)); // Cozy Green Foliage
        }
    }

    private void BuildMenuBoard()
    {
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = "BistroMenuBoard";
        board.transform.position = new Vector3(-4f, 1.2f, -8.2f);
        board.transform.localScale = new Vector3(1.4f, 1.8f, 0.1f);
        board.transform.rotation = Quaternion.Euler(-10f, 15f, 0f);
        SetColor(board, new Color(0.15f, 0.15f, 0.18f)); // Dark Chalkboard
    }

    private void BuildFence()
    {
        GameObject fenceRoot = new GameObject("BistroFence");

        // Perimeter fence along outer deck edges
        Vector3[] railingPos = new Vector3[]
        {
            new Vector3(0f, 0.5f, 11.5f),
            new Vector3(0f, 0.5f, -11.5f),
            new Vector3(-13.5f, 0.5f, 0f),
            new Vector3(13.5f, 0.5f, 0f)
        };

        Vector3[] railingScale = new Vector3[]
        {
            new Vector3(27f, 0.8f, 0.2f),
            new Vector3(27f, 0.8f, 0.2f),
            new Vector3(0.2f, 0.8f, 23f),
            new Vector3(0.2f, 0.8f, 23f)
        };

        for (int i = 0; i < railingPos.Length; i++)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.transform.SetParent(fenceRoot.transform, false);
            rail.transform.position = railingPos[i];
            rail.transform.localScale = railingScale[i];
            SetColor(rail, new Color(0.55f, 0.38f, 0.22f));
        }
    }

    private static void SetColor(GameObject go, Color color)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            renderer.material = new Material(urpLit != null ? urpLit : Shader.Find("Standard"));
            renderer.material.color = color;
        }
    }
}
