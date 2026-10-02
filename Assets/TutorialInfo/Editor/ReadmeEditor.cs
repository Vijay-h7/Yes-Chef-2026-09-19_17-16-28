using UnityEditor;

// Stub: neutered version of Unity's default Readme inspector. The original auto-popped this
// asset on every project load and rendered a marketing/tutorial GUI - neither is relevant to
// a finished game, so both behaviors have been removed. Kept only so the CustomEditor binding
// for Readme.asset doesn't break.
[CustomEditor(typeof(Readme))]
sealed class ReadmeEditor : Editor
{
    protected sealed override void OnHeaderGUI() { }
    public sealed override void OnInspectorGUI() { }
}
