using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(InventorySelectionGlow))]
public sealed class InventorySelectionGlowEditor : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        foreach (string field in new[] { "glowIntensity", "animationSpeed", "borderWidth", "glowWidth", "shimmerColor" })
            EditorGUILayout.PropertyField(serializedObject.FindProperty(field));
        serializedObject.ApplyModifiedProperties();
    }
}
