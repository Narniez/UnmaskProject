using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(HexGrid))]
public class HexGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HexGrid hexGrid = (HexGrid)target;

        if (GUILayout.Button("Generate Grid"))
        {
            hexGrid.GenerateGrid();
        }

        if (GUILayout.Button("Clear Grid"))
        {
            hexGrid.ClearGrid();
        }
        if (GUILayout.Button("Update Grid Visuals"))
        {
            hexGrid.UpdateGridVisuals();
        }

        if (GUILayout.Button("Save Grid as Prefab"))
        {
            hexGrid.SaveGridAsPrefab();
        }

        if (GUILayout.Button("Force Clear"))
        {
            hexGrid.ForceDeleteAllChildren();
        }
    }
}
