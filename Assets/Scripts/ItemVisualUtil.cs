using UnityEngine;

// Shows an ingredient's real 3D model (raw/prepared prefab from IngredientData) as a child
// of the given anchor, falling back to tinting a plain mesh if no model prefab is assigned.
// Used by both Chef (held item) and Station (table/stove slots).
public static class ItemVisualUtil
{
    public static void Apply(Transform anchor, Renderer fallbackRenderer, Item item)
    {
        // Remove whatever model was shown here before.
        for (int i = anchor.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(anchor.GetChild(i).gameObject);

        if (item == null)
        {
            if (fallbackRenderer != null) fallbackRenderer.enabled = false;
            return;
        }

        GameObject prefab = item.Data.GetModelPrefab(item.IsPrepared);
        if (prefab == null)
        {
            // No 3D model assigned for this ingredient yet - old colored-cube behaviour.
            if (fallbackRenderer != null)
            {
                fallbackRenderer.enabled = true;
                fallbackRenderer.material.color = item.CurrentColor;
            }
            return;
        }

        if (fallbackRenderer != null) fallbackRenderer.enabled = false;

        var instance = Object.Instantiate(prefab);
        instance.transform.SetParent(anchor, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = prefab.transform.localRotation;   // preserve the artist's authored pose (e.g. a carrot lying flat)
        instance.transform.localScale = prefab.transform.localScale;

        // Burnt items get darkened via a property block (doesn't touch the shared material asset).
        Color tint = item.IsBurnt ? item.Data.burntColor : Color.white;
        var mpb = new MaterialPropertyBlock();
        foreach (var r in instance.GetComponentsInChildren<Renderer>())
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", tint);
            mpb.SetColor("_Color", tint);
            r.SetPropertyBlock(mpb);
        }
    }
}
