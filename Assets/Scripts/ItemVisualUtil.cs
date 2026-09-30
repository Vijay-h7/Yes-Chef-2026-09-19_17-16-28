using UnityEngine;

// Shows an ingredient's real 3D model (raw/prepared prefab from IngredientData) as a child
// of the given anchor, falling back to tinting a plain mesh if no model prefab is assigned.
// Used by both Chef (held item) and Station (table/stove slots).
public static class ItemVisualUtil
{
    public static void Apply(Transform anchor, Renderer fallbackRenderer, Item item, bool isHeldByChef = false)
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
        instance.transform.localPosition = isHeldByChef ? item.Data.heldLocalPositionOffset : Vector3.zero;
        Quaternion baseRotation = prefab.transform.localRotation;   // preserve the artist's authored pose (e.g. a carrot lying flat)
        if (isHeldByChef)
        {
            // The hand anchor itself can carry an unrelated rotation (the character model needed a
            // 180-degree fix so turning left/right reads correctly). Composing heldExtraRotationEuler
            // as a LOCAL offset on top of that anchor rotation can silently cancel out instead of
            // adding up (e.g. 180 + 180 = 360 = back to 0). Instead, compute the rotation relative to
            // the chef's own root, which always faces the direction the player is actually moving/
            // facing, so the requested extra rotation reliably shows up exactly as specified.
            Quaternion desiredWorldRotation = anchor.root.rotation * Quaternion.Euler(item.Data.heldExtraRotationEuler) * baseRotation;
            instance.transform.rotation = desiredWorldRotation;
        }
        else
        {
            instance.transform.localRotation = baseRotation;
        }
        // The anchor may carry scale of its own that has nothing to do with the ingredient
        // (e.g. it also sizes its own fallback mesh, or sits under a scaled character model).
        // Compensate for the anchor's accumulated world scale so the model always renders at
        // the size its own prefab was authored at, regardless of the parent chain.
        Vector3 anchorLossy = anchor.lossyScale;
        Vector3 targetScale = prefab.transform.localScale;
        instance.transform.localScale = new Vector3(
            anchorLossy.x != 0f ? targetScale.x / anchorLossy.x : targetScale.x,
            anchorLossy.y != 0f ? targetScale.y / anchorLossy.y : targetScale.y,
            anchorLossy.z != 0f ? targetScale.z / anchorLossy.z : targetScale.z);

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
