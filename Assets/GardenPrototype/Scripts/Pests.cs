using UnityEngine;
using System.Collections.Generic;

public class Pests : MonoBehaviour
{
    public bool isRat, isSnail;
    public float moveSpeed = 200f;
    private Transform targetPlant;
    private bool isMoving = false;
    public static bool PuzzleCheck = false;
    public static bool PuzzleCompleated = false;
    public float detectionRadius = 200f;

    private RectTransform rectTransform;
    private List<Transform> initialTargets = new List<Transform>();
    private bool hasLockedTargets = false;
    private Vector2 startPosition;
    private bool returningToStart = false;

    private static int activePests = 0;

    private void Awake()
    {
        PuzzleCheck = false;
        PuzzleCompleated = false;
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        if (PuzzleCheck && !hasLockedTargets)
        {
            FindInitialTargetPlants();
            hasLockedTargets = true;

            if (initialTargets.Count == 0)
            {
                CheckAllPestsCompleted();
            }
            else
            {
                activePests++;
            }
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
            targetPlant = initialTargets[0];
            isMoving = true;
            activePests++; 
        }
        else
        {
            CheckAllPestsCompleted(); // No valid targets, check if all pests are done
        }
    }

    private bool IsInRange(RectTransform plantRect)
    {
        float distance = Vector2.Distance(rectTransform.anchoredPosition, plantRect.anchoredPosition);
        return distance < detectionRadius;
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
            targetPlant = initialTargets[0];
        }
        else
        {
            targetPlant = null;
            isMoving = false;
            returningToStart = true;
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
            returningToStart = false;
            activePests--;

            CheckAllPestsCompleted();
        }
    }

    public void PuzzleChecking()
    {
        PuzzleCheck = true;
    }

    private void CheckAllPestsCompleted()
    {
        if (activePests <= 0)
        {
            PuzzleCompleated = true;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;
        }

        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireDisc(rectTransform.position, Vector3.forward, detectionRadius);
    }
#endif
}