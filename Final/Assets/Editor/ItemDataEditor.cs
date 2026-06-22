using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemData))]
[CanEditMultipleObjects]
public class ItemDataEditor : Editor
{
    SerializedProperty itemTypeProperty;
    SerializedProperty weaponTypeProperty;
    private void Onable()
    {
        itemTypeProperty = serializedObject.FindProperty("itemType");
        weaponTypeProperty = serializedObject.FindProperty("weaponType");
    }

    public override void OnInspectorGUI()
    {
        if(serializedObject == null || target == null) return;
        serializedObject.Update();
        if (itemTypeProperty == null || weaponTypeProperty == null)
        {
            DrawDefaultInspector();
            return;
        }
        DrawPropertiesExcluding(serializedObject,new string[] {"weaponType"});
        if(itemTypeProperty.enumValueIndex == (int)ItemType.Weapon)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(weaponTypeProperty);
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
