using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Interactable))]
public class InteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Interactable interactable = (Interactable)target;

        // Show the enum dropdown
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));

        // If the type is LockedDoor, show the requiredItem field
        if (interactable.type == Interactable.InteractableType.LockedDoor)
        {
            // Show the requiredItem field only for LockedDoor type
            EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredItem"));
        }

        // Show interactIcon and other UI elements
        EditorGUILayout.PropertyField(serializedObject.FindProperty("interactIcon"));

        serializedObject.ApplyModifiedProperties();
    }
}
