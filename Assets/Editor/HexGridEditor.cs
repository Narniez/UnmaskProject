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

        if (GUILayout.Button("Reset All Tiles"))
        {
            hexGrid.ClearGrid();
        }
        if (GUILayout.Button("Update Tiles"))
        {
            hexGrid.UpdateGridVisuals();
        }

        if (GUILayout.Button("Save Grid as Prefab"))
        {
            hexGrid.SaveGridAsPrefab();
        }

        if (GUILayout.Button("Delete Grid"))
        {
            hexGrid.ForceDeleteAllChildren();
        }
    }
}
