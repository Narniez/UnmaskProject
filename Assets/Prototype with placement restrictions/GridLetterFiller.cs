using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GridLetterFiller : MonoBehaviour
{
    public GridLayoutGroup grid;
    public Image selectionHighlight;
    public GameObject victoryPopup;
    public TMP_Text victoryText;
    public Button nextLevelButton;

    [SerializeField] private Sprite ropeSprite;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform lineContainer;

    // Serialized fields for assigning plants in the Inspector
    [SerializeField] private Transform plant1;
    [SerializeField] private Transform plant2;
    [SerializeField] private Transform plant3;
    [SerializeField] private Transform plant4;
    [SerializeField] private Transform plant5;
    [SerializeField] private Transform plant6;
    [SerializeField] private Transform plant7;
    [SerializeField] private Transform plant8;
    [SerializeField] private Transform plant9;

    [SerializeField] private int startingRow = 1; // Default to 1st row (row 0 in code)

    private RectTransform[] tiles;
    private bool isRowMode = true;
    private int currentRow = 0;
    private int currentColumn = -1;
    private List<Transform> placedPlants = new List<Transform>();
    private List<Transform> availablePlants = new List<Transform>();
    private List<Image> lineSegments = new List<Image>();
    private HashSet<int> occupiedTiles = new HashSet<int>();
    private int gridColumns;
    private int gridRows;

    // Track the last placed plant's tile index, terrain type, and plant type
    private int lastPlacedTileIndex = -1;
    private string lastPlacedTerrainType = null;
    private string lastPlacedPlantType = null;

    void Start()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found! Please assign the Canvas in the Inspector.", this);
            }
        }

        if (ropeSprite == null)
        {
            Debug.LogError("Rope Sprite is not assigned in the Inspector! The rope will not be visible.", this);
        }

        if (lineContainer == null)
        {
            Debug.LogError("LineContainer is not assigned in the Inspector! Please assign a RectTransform to hold the rope segments.", this);
            return;
        }

        // Populate availablePlants using the serialized fields
        if (plant1 != null) availablePlants.Add(plant1);
        if (plant2 != null) availablePlants.Add(plant2);
        if (plant3 != null) availablePlants.Add(plant3);
        if (plant4 != null) availablePlants.Add(plant4);
        if (plant5 != null) availablePlants.Add(plant5);
        if (plant6 != null) availablePlants.Add(plant6);
        if (plant7 != null) availablePlants.Add(plant7);
        if (plant8 != null) availablePlants.Add(plant8);
        if (plant9 != null) availablePlants.Add(plant9);

        FillGridWithTiles();

        // Initialize currentRow based on startingRow (convert from 1-based to 0-based)
        currentRow = Mathf.Clamp(startingRow - 1, 0, gridRows - 1);

        UpdateSelectionHighlight();
        victoryPopup.SetActive(false);
        nextLevelButton.onClick.AddListener(OnNextLevelClick);

        UpdatePlantInteraction();
    }

    void UpdateStringLine()
    {
        foreach (var segment in lineSegments)
        {
            Destroy(segment.gameObject);
        }
        lineSegments.Clear();

        if (placedPlants.Count < 2)
        {
            return;
        }

        for (int i = 0; i < placedPlants.Count - 1; i++)
        {
            Vector2 startPos = RectTransformUtility.WorldToScreenPoint(null, placedPlants[i].position);
            Vector2 endPos = RectTransformUtility.WorldToScreenPoint(null, placedPlants[i + 1].position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(lineContainer, startPos, null, out startPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(lineContainer, endPos, null, out endPos);

            GameObject segmentObj = new GameObject($"LineSegment_{i}");
            segmentObj.transform.SetParent(lineContainer, false);
            Image segmentImage = segmentObj.AddComponent<Image>();
            segmentImage.sprite = ropeSprite;
            RectTransform segmentRect = segmentImage.GetComponent<RectTransform>();

            Vector2 midPos = (startPos + endPos) / 2;
            segmentRect.anchoredPosition = midPos;

            Vector2 direction = (endPos - startPos).normalized;
            float distance = Vector2.Distance(startPos, endPos);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            segmentRect.sizeDelta = new Vector2(distance, 5f);
            segmentRect.rotation = Quaternion.Euler(0, 0, angle);

            lineSegments.Add(segmentImage);
        }
    }

    void OnNextLevelClick()
    {
        FillGridWithTiles();
        placedPlants.Clear();
        occupiedTiles.Clear();
        foreach (var segment in lineSegments)
        {
            Destroy(segment.gameObject);
        }
        lineSegments.Clear();
        victoryPopup.SetActive(false);
        isRowMode = true;
        currentRow = Mathf.Clamp(startingRow - 1, 0, gridRows - 1); // Reset to the starting row
        currentColumn = -1;
        lastPlacedTileIndex = -1; // Reset last placed tile
        lastPlacedTerrainType = null; // Reset last terrain type
        lastPlacedPlantType = null; // Reset last plant type
        UpdateSelectionHighlight();

        DragAndDrop1[] allPlants = FindObjectsByType<DragAndDrop1>(FindObjectsSortMode.None);
        foreach (var plant in allPlants)
        {
            plant.ResetForNewLevel();
        }

        UpdatePlantInteraction();
    }

    public bool IsValidPlacement(int index)
    {
        // Check if the target tile is already occupied
        if (occupiedTiles.Contains(index))
        {
            return false;
        }

        // Get the next plant to place
        Transform nextPlant = null;
        foreach (var plant in availablePlants)
        {
            DragAndDrop1 dragHandler = plant.GetComponent<DragAndDrop1>();
            if (dragHandler != null && !dragHandler.IsPlaced())
            {
                nextPlant = plant;
                break;
            }
        }

        if (nextPlant == null)
        {
            return false; // No plant to place
        }

        // Check the terrain type of the target tile
        string targetTerrainType = GetTerrainType(index);

        // Check the plant type of the next plant
        DragAndDrop1 nextPlantHandler = nextPlant.GetComponent<DragAndDrop1>();
        string nextPlantType = GetPlantType(nextPlantHandler);

        // Ensure the plant type matches the target terrain type
        if (nextPlantType != GetPlantTypeForTerrain(targetTerrainType))
        {
            return false; // Plant type must match the target terrain type
        }

        // If the last plant's type matches the expected plant type for the last placed tile's terrain, allow crossing that terrain type
        bool canCrossLastTerrain = lastPlacedTileIndex != -1 && lastPlacedPlantType == GetPlantTypeForTerrain(lastPlacedTerrainType);

        // Apply terrain crossing restrictions and check for occupied tiles in the path
        if (lastPlacedTileIndex != -1 && targetTerrainType != null && nextPlantType != null)
        {
            int lastRow = lastPlacedTileIndex / gridColumns;
            int lastCol = lastPlacedTileIndex % gridColumns;
            int targetRow = index / gridColumns;
            int targetCol = index % gridColumns;

            // Check for horizontal placement (same row, different columns)
            if (lastRow == targetRow)
            {
                int startCol = lastCol;
                int endCol = targetCol;
                int step = (startCol < endCol) ? 1 : -1;
                for (int i = startCol + step; i != endCol + step; i += step)
                {
                    int tileIndex = lastRow * gridColumns + i;
                    // Check if the tile in the path is occupied
                    if (occupiedTiles.Contains(tileIndex))
                    {
                        return false; // Occupied tile blocks the path
                    }
                    string terrainType = GetTerrainType(tileIndex);
                    // Allow the tile if it matches the last placed terrain type and can be crossed
                    if (terrainType != null && terrainType == lastPlacedTerrainType && canCrossLastTerrain)
                    {
                        continue;
                    }
                    // Allow the tile if it matches the target terrain type
                    if (terrainType != null && terrainType == targetTerrainType)
                    {
                        continue;
                    }
                    // Allow the tile if it's a grass tile (all plants can jump over grass)
                    if (terrainType == "grass")
                    {
                        continue;
                    }
                    // Block placement if none of the above conditions are met
                    return false;
                }
            }
            // Check for vertical placement (same column, different rows)
            else if (lastCol == targetCol)
            {
                int startRow = lastRow;
                int endRow = targetRow;
                int step = (startRow < endRow) ? 1 : -1;
                for (int i = startRow + step; i != endRow + step; i += step)
                {
                    int tileIndex = i * gridColumns + lastCol;
                    // Check if the tile in the path is occupied
                    if (occupiedTiles.Contains(tileIndex))
                    {
                        return false; // Occupied tile blocks the path
                    }
                    string terrainType = GetTerrainType(tileIndex);
                    // Allow the tile if it matches the last placed terrain type and can be crossed
                    if (terrainType != null && terrainType == lastPlacedTerrainType && canCrossLastTerrain)
                    {
                        continue;
                    }
                    // Allow the tile if it matches the target terrain type
                    if (terrainType != null && terrainType == targetTerrainType)
                    {
                        continue;
                    }
                    // Allow the tile if it's a grass tile (all plants can jump over grass)
                    if (terrainType == "grass")
                    {
                        continue;
                    }
                    // Block placement if none of the above conditions are met
                    return false;
                }
            }
            else
            {
                // If the target tile is not in the same row or column as the last placed tile, block placement
                return false;
            }
        }

        return true;
    }

    // Helper method to determine the terrain type of a tile
    private string GetTerrainType(int tileIndex)
    {
        if (tileIndex < 0 || tileIndex >= tiles.Length)
        {
            return null;
        }

        PlantSpotVar1 spot = tiles[tileIndex].GetComponent<PlantSpotVar1>();
        if (spot == null)
        {
            return null;
        }

        if (spot.grass) return "grass";
        if (spot.wet) return "wet";
        if (spot.sand) return "sand";
        if (spot.sun) return "sun";
        return null;
    }

    // Helper method to determine the plant type
    private string GetPlantType(DragAndDrop1 plantHandler)
    {
        if (plantHandler == null)
        {
            return null;
        }

        if (plantHandler.normal) return "normal";
        if (plantHandler.wet) return "wet";
        if (plantHandler.dry) return "dry";
        if (plantHandler.sun) return "sun";
        return null;
    }

    // Helper method to map terrain type to expected plant type
    private string GetPlantTypeForTerrain(string terrainType)
    {
        switch (terrainType)
        {
            case "grass": return "normal";
            case "wet": return "wet";
            case "sand": return "dry";
            case "sun": return "sun";
            default: return null;
        }
    }

    public RectTransform[] GetTiles()
    {
        return tiles;
    }

    public void OnPlantPlaced(Transform plant, int tileIndex)
    {
        if (occupiedTiles.Contains(tileIndex))
        {
            return;
        }

        placedPlants.Add(plant);
        occupiedTiles.Add(tileIndex);
        UpdateStringLine();

        // Update the last placed tile index, terrain type, and plant type
        lastPlacedTileIndex = tileIndex;
        lastPlacedTerrainType = GetTerrainType(tileIndex);
        DragAndDrop1 plantHandler = plant.GetComponent<DragAndDrop1>();
        lastPlacedPlantType = GetPlantType(plantHandler);

        if (isRowMode)
        {
            currentColumn = tileIndex % gridColumns;
            isRowMode = false;
        }
        else
        {
            currentRow = tileIndex / gridColumns;
            isRowMode = true;
        }

        UpdateSelectionHighlight();
        CheckForVictory();
        UpdatePlantInteraction();
    }

    void CheckForVictory()
    {
        int totalTiles = tiles.Length;

        if (placedPlants.Count >= totalTiles)
        {
            victoryPopup.SetActive(true);
            if (victoryText != null)
            {
                victoryText.text = "Good job, you did it!";
            }
        }
    }

    void FillGridWithTiles()
    {
        tiles = grid.GetComponentsInChildren<PlantSpotVar1>()
            .Select(spot => spot.GetComponent<RectTransform>())
            .ToArray();

        gridColumns = grid.constraintCount;
        gridRows = tiles.Length / gridColumns;

        if (tiles.Length % gridColumns != 0)
        {
            Debug.LogError($"Grid has {tiles.Length} tiles, which is not evenly divisible by {gridColumns} columns!");
            return;
        }
    }

    void UpdateSelectionHighlight()
    {
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        RectTransform highlightRect = selectionHighlight.GetComponent<RectTransform>();

        Vector2 cellSize = grid.cellSize;
        Vector2 spacing = grid.spacing;

        float totalWidth = (cellSize.x * gridColumns) + (spacing.x * (gridColumns - 1));
        float totalHeight = (cellSize.y * gridRows) + (spacing.y * (gridRows - 1));

        highlightRect.anchorMin = new Vector2(0, 1);
        highlightRect.anchorMax = new Vector2(0, 1);
        highlightRect.pivot = new Vector2(0, 1);

        if (isRowMode)
        {
            highlightRect.sizeDelta = new Vector2(totalWidth, cellSize.y);
            float yPos = -currentRow * (cellSize.y + spacing.y);
            highlightRect.anchoredPosition = new Vector2(0, yPos);
        }
        else
        {
            highlightRect.sizeDelta = new Vector2(cellSize.x, totalHeight);
            float xPos = currentColumn * (cellSize.x + spacing.x);
            highlightRect.anchoredPosition = new Vector2(xPos, 0);
        }
    }

    public bool IsNextPlantToPlace(Transform plant)
    {
        foreach (var availablePlant in availablePlants)
        {
            DragAndDrop1 dragHandler = availablePlant.GetComponent<DragAndDrop1>();
            if (dragHandler != null && !dragHandler.IsPlaced())
            {
                return availablePlant == plant;
            }
        }
        return false;
    }

    private void UpdatePlantInteraction()
    {
        Transform nextPlant = null;

        // Find the next unplaced plant
        foreach (var plant in availablePlants)
        {
            DragAndDrop1 dragHandler = plant.GetComponent<DragAndDrop1>();
            if (dragHandler != null && !dragHandler.IsPlaced())
            {
                nextPlant = plant;
                break;
            }
        }

        // Enable interaction only for the next plant
        foreach (var plant in availablePlants)
        {
            DragAndDrop1 dragHandler = plant.GetComponent<DragAndDrop1>();
            if (dragHandler != null)
            {
                bool isNext = (plant == nextPlant);
                dragHandler.SetInteractable(isNext);
            }
        }
    }
}