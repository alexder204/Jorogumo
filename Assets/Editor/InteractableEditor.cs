using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Interactable))]
public class InteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw the enum dropdown
        EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));

        // If the type is LockedDoor, show the requiredItem field
        Interactable interactable = (Interactable)target;
        if (interactable.type == Interactable.InteractableType.LockedDoor)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredItem"));
        }

        // Draw the interactIcon field
        EditorGUILayout.PropertyField(serializedObject.FindProperty("interactIcon"));

        serializedObject.ApplyModifiedProperties();
    }

}
