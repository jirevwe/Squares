using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(KeyboardTracker))]
public class KeyboardTrackerEditor : Editor {

    public override void OnInspectorGUI()
    {
        KeyboardTracker kt = target as KeyboardTracker;

        //Axes
        EditorGUILayout.LabelField("Axes", EditorStyles.boldLabel);
        //EditorGUI.indentLevel++;

        if(kt.axisKeys.Length == 0)
        {
            EditorGUILayout.HelpBox("No axes defined in InputManager", MessageType.Info);
        }
        else
        {
            SerializedProperty prop = serializedObject.FindProperty("axisKeys");
            for (int i = 0; i < kt.axisKeys.Length; i++)
            {
                EditorGUILayout.PropertyField(prop.GetArrayElementAtIndex(i), new GUIContent("Axis " + i));
            }
        }
        //EditorGUI.indentLevel--;

        //Buttons
        EditorGUILayout.LabelField("Buttons", EditorStyles.boldLabel);
        //EditorGUI.indentLevel++;

        if (kt.buttonKeys.Length == 0)
        {
            EditorGUILayout.HelpBox("No buttons defined in InputManager", MessageType.Info);
        }
        else
        {
            for (int i = 0; i < kt.buttonKeys.Length; i++)
            {
                kt.buttonKeys[i] = (KeyCode)EditorGUILayout.EnumPopup("Button " + i, kt.buttonKeys[i]);
            }
        }
        //EditorGUI.indentLevel--;
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
    }
}
