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
    [SerializeField] private Transform plantsParent; // Parent object containing the 9 initial plants

    private RectTransform[] tiles;
    private bool isRowMode = true;
    private int currentRow = 0;
    private int currentColumn = -1;
    private List<Transform> placedPlants = new List<Transform>(); // Plants that have been placed on the grid
    private List<Transform> availablePlants = new List<Transform>(); // Initial 9 plants in order
    private List<Image> lineSegments = new List<Image>();
    private HashSet<int> occupiedTiles = new HashSet<int>(); // Track occupied tile indices
    private int gridColumns;
    private int gridRows;

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

        if (plantsParent == null)
        {
            Debug.LogError("Plants Parent is not assigned in the Inspector! Please assign the parent object containing the initial plants.", this);
            return;
        }

        // Initialize the list of available plants in order
        foreach (Transform child in plantsParent)
        {
            if (child.GetComponent<DragAndDrop1>() != null) // Only add plants with DragAndDrop1
            {
                availablePlants.Add(child);
            }
        }

        FillGridWithTiles();
        UpdateSelectionHighlight();
        victoryPopup.SetActive(false);
        nextLevelButton.onClick.AddListener(OnNextLevelClick);
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
        currentRow = 0;
        currentColumn = -1;
        UpdateSelectionHighlight();

        // Reset the isPlaced flag on all plants so they can be dragged again
        DragAndDrop1[] allPlants = FindObjectsByType<DragAndDrop1>(FindObjectsSortMode.None);
        foreach (var plant in allPlants)
        {
            plant.ResetForNewLevel();
        }
    }

    public bool IsValidPlacement(int index)
    {
        if (occupiedTiles.Contains(index))
        {
            return false;
        }

        int row = index / gridColumns;
        int col = index % gridColumns;

        if (isRowMode)
            return row == currentRow;
        else
            return col == currentColumn;
    }

    public RectTransform[] GetTiles()
    {
        return tiles;
    }

    public void OnPlantPlaced(Transform plant, int tileIndex)
    {
        if (occupiedTiles.Contains(tileIndex))
        {
            Debug.LogWarning($"Tile {tileIndex} is already occupied! Plant placement rejected.");
            return;
        }

        placedPlants.Add(plant);
        occupiedTiles.Add(tileIndex);
        UpdateStringLine();

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

    // Method to check if a plant is the next one to be placed
    public bool IsNextPlantToPlace(Transform plant)
    {
        // Find the first unplaced plant in the availablePlants list
        foreach (var availablePlant in availablePlants)
        {
            DragAndDrop1 dragHandler = availablePlant.GetComponent<DragAndDrop1>();
            if (dragHandler != null && !dragHandler.IsPlaced())
            {
                return availablePlant == plant; // Return true if this is the next unplaced plant
            }
        }
        return false; // No unplaced plants left
    }
}