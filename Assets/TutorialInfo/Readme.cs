using System;
using UnityEngine;

// Stub: this was Unity's default new-project welcome/tutorial asset. It has no bearing on
// Yes Chef's gameplay. Kept as an inert data container only so the pre-existing Readme.asset
// doesn't become a broken/missing-script reference. See README.md at the project root for the
// real project documentation.
public class Readme : ScriptableObject
{
    public string title;

    [Serializable]
    public class Section
    {
        public string heading, text, linkText, url;
    }
}
