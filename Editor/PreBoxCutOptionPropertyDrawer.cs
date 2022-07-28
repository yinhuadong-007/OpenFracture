using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PreBoxCutOptions))]
public class PreBoxCutOptionsPropertyDrawer : PropertyDrawer
{
    private static bool foldout = true;

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.indentLevel = 0;
        foldout = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, label);

        if (foldout)
        {
            EditorGUI.indentLevel = 1;

            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(16);
            if (GUILayout.Button("PreBoxCut Mesh", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(32) }))
            {
                ((Src_PreBoxCut)property.serializedObject.targetObject).ComputeBoxCut();
            }
            GUILayout.Space(16);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUI.indentLevel = 0;
    }

    // Hack to prevent extra space at top of property drawer. This is due to using EditorGUILayout
    // in OnGUI, but I don't want to have to manually specify control sizes
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0; }
}