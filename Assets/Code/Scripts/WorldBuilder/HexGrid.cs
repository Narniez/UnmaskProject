using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HexGrid : MonoBehaviour
{
    public int width = 12;
    public int height = 12;
    public float tileSize = 1;
    public GameObject hexTilePrefab;


    private Dictionary<Vector2Int, HexTile> hexTiles = new Dictionary<Vector2Int, HexTile>();
    private HexTile clickedTile;

    private void Start()
    {
        UpdateGridVisuals();

    }

    public void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Create a new hex tile
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = GetWorldPosition(x, y);
                GameObject tileObj = Instantiate(hexTilePrefab, worldPos, Quaternion.identity, this.transform);
                tileObj.name = $"HexTile_{x}_{y}";
                HexTile hexTile = tileObj.GetComponent<HexTile>();

                hexTiles[gridPos] = hexTile;

                // Initialize the hex tile
                hexTile.Initialize(gridPos, this);
            }
        }
    }

    

    public void SaveGridAsPrefab()
    {
        string folderPath = "Assets/SavedGrids";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "SavedGrids");
        }

        string prefabPath = $"{folderPath}/HexGrid_{System.DateTime.Now:yyyyMMdd_HHmmss}.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(this.gameObject, prefabPath, InteractionMode.UserAction);

        Debug.Log($"Grid saved as prefab: {prefabPath}");
    }

    public void ClearGrid()
    {
        // Destroy all existing tiles before regenerating the grid
        foreach (var tile in hexTiles.Values)
        {
            if (tile != null)
                DestroyImmediate(tile.gameObject);
        }
        hexTiles.Clear();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif

    }

    public void UpdateGridVisuals()
    {
        foreach (var tile in hexTiles.Values)
        {
            if (tile != null)
            {
                tile.UpdateVisual();
            }
        }
    }

    public void RemoveTile(Vector2Int position)
    {
        if (hexTiles.TryGetValue(position, out HexTile tile))
        {
            DestroyImmediate(tile);
            hexTiles.Remove(position);
        }
    }

    public void ForceDeleteAllChildren()
    {
        // Iterate through all child objects and destroy them
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Clear the dictionary
        hexTiles.Clear();

#if UNITY_EDITOR
        // Mark the scene as dirty to save changes
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
    }

    Vector3 GetWorldPosition(int x, int y)
    {
        //POINTY TOP
        return new Vector3(x, 0, 0) * tileSize +
             new Vector3(0, y, 0) * tileSize * 0.75f +
             ((y % 2) == 1 ? new Vector3(1, 0, 0) * tileSize * 0.5f : Vector3.zero);


        // FLAT TOP currently not working
        //float hexWidth = Mathf.Sqrt(3) * tileSize; // Width of a flat-top hex
        //float hexHeight = 2f * tileSize; // Height of a flat-top hex

        //float worldX = x * hexWidth; // X position shifts by full hex width
        //float worldY = y * (hexHeight * 0.75f); // Y shifts by 3/4 of hex height

        //// Offset odd rows to stagger them correctly
        //if (y % 2 == 1)
        //{
        //    worldX += hexWidth * 0.5f;
        //}

        //return new Vector3(worldX, -worldY, 0); // Flip Y if necessary
    }

}