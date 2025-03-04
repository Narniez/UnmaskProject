using UnityEngine;
using System.Collections.Generic;

public class Pests : MonoBehaviour
{
    public bool isRat, isSnail; // Define pest type
    public float moveSpeed = 200f; // Adjust this value for UI movement
    private Transform targetPlant;
    private bool isMoving = false;
    public static bool PuzzleFinished = false; // Determines when pests should move
    public float detectionRadius = 200f; // Adjust based on UI scale

    private RectTransform rectTransform; // Pest's UI position reference
    private List<Transform> initialTargets = new List<Transform>(); // Stores plants in range when puzzle starts
    private bool hasLockedTargets = false; // Prevents finding new targets after moving starts
    private Vector2 startPosition; // Stores initial position

    private bool returningToStart = false; // Flag to track return movement

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>(); // Get RectTransform for UI positioning
        startPosition = rectTransform.anchoredPosition; // Store the starting position
    }

    private void Update()
    {
        if (PuzzleFinished && !hasLockedTargets) // Only detect plants ONCE when the puzzle starts
        {
            FindInitialTargetPlants();
            hasLockedTargets = true; // Lock in targets, prevent further checks
        }

        if (isMoving && targetPlant != null)
        {
            MoveTowardsPlant();
        }

        if (returningToStart)
        {
            MoveBackToStart();
        }
    }

    private void FindInitialTargetPlants()
    {
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
        foreach (GameObject plant in plants)
        {
            RectTransform plantRect = plant.GetComponent<RectTransform>();
            DragAndDrop plantVars = plant.GetComponent<DragAndDrop>();

            if (plantVars != null && CanEatPlant(plantVars) && !IsPlantProtected(plantVars) && IsInRange(plantRect))
            {
                initialTargets.Add(plant.transform);
            }
        }

        if (initialTargets.Count > 0)
        {
            targetPlant = initialTargets[0]; // Set first target
            isMoving = true;
        }
    }

    private bool IsInRange(RectTransform plantRect)
    {
        float distance = Vector2.Distance(rectTransform.anchoredPosition, plantRect.anchoredPosition);
        return distance < detectionRadius; // Adjust based on UI scale
    }

    private void MoveTowardsPlant()
    {
        rectTransform.anchoredPosition = Vector2.MoveTowards(
            rectTransform.anchoredPosition,
            ((RectTransform)targetPlant).anchoredPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(rectTransform.anchoredPosition, ((RectTransform)targetPlant).anchoredPosition) < 5f)
        {
            EatPlant();
        }
    }

    private bool CanEatPlant(DragAndDrop plantVars)
    {
        return (isRat && plantVars.RatEats) || (isSnail && plantVars.SnailEats);
    }

    private bool IsPlantProtected(DragAndDrop plantVars)
    {
        return (isRat && plantVars.protectedRat) || (isSnail && plantVars.protectedSnail);
    }

    private void EatPlant()
    {
        if (targetPlant != null)
        {
            targetPlant.gameObject.SetActive(false);
            initialTargets.Remove(targetPlant);
        }

        if (initialTargets.Count > 0)
        {
            targetPlant = initialTargets[0]; // Move to next plant from the original list
        }
        else
        {
            targetPlant = null;
            isMoving = false; // Stop moving when all original targets are eaten
            returningToStart = true; // Start moving back to start
        }
    }

    private void MoveBackToStart()
    {
        rectTransform.anchoredPosition = Vector2.MoveTowards(
            rectTransform.anchoredPosition,
            startPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(rectTransform.anchoredPosition, startPosition) < 5f)
        {
            returningToStart = false; // Stop movement once back
        }
    }

    public void PuzzleDone()
    {
        PuzzleFinished = true;
    }
}