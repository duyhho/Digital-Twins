
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OfficeFurnitureGenerator))]
public class EditorOfficeFurnitureGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        OfficeFurnitureGenerator ofg = (OfficeFurnitureGenerator)target;

        if (GUILayout.Button("Generate Furniture"))
        {
            ofg.generate();
        }
    }
}
