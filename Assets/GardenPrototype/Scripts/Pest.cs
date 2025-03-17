using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pest : MonoBehaviour
{
    [SerializeField] private PestType _type;
    [SerializeField] private float _moveSpeed = 200f;
    [SerializeField] private float _detectionRadius = 200f;

    private Transform targetPlant;
    private bool isMoving = false;

    private RectTransform rectTransform;
    private List<Transform> initialTargets = new List<Transform>();
    private bool hasLockedTargets = false;
    private Vector2 startPosition;
    private bool returningToStart = false;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;

        GameManager.AddPuzzleCondition(FindInitialTargetPlants);
    }

    private void Update()
    {
        if (isMoving && targetPlant != null)
        {
            StartCoroutine(MoveTowardsPlant());
        }

        if (returningToStart)
        {
            StartCoroutine(MoveBackToStart());
        }
    }

    private bool FindInitialTargetPlants()
    {
        initialTargets.Clear();

        Collider2D[] coveredSpots = Physics2D.OverlapCircleAll(rectTransform.position, _detectionRadius);

        List<ImprovedPlantSpot> plantSpots = new();

        int emptySpots = 0;
        int plantSpotsTotal = 0;

        foreach (Collider2D collider in coveredSpots)
        {
            ImprovedPlantSpot plantSpot = collider.GetComponentInParent<ImprovedPlantSpot>();

            if (plantSpot == null)
                continue;

            plantSpotsTotal++;

            if (!plantSpot.IsOccupied)
            {
                emptySpots++;
                continue;
            }

            plantSpots.Add(plantSpot);
        }

        Debug.Log($"[{name}]: Checking spots to find edibles. {plantSpotsTotal} spots found, but {emptySpots} spots are empty.\n" +
            $"I have {plantSpotsTotal - emptySpots} plants to look at.");

        foreach (ImprovedPlantSpot spot in plantSpots)
        {
            Plant plant = spot.CurrentPlant;
            RectTransform plantSpotTransform = spot.GetComponent<RectTransform>();

            Debug.Log($"[{name}]: Inspecting plant: {plant.name}");

            if (!CanEatPlant(plant))
            {
                Debug.Log($"[{name}]: Skipping plant, because I can't eat it.");
                continue;
            }

            if (IsPlantProtected(spot))
            {
                Debug.Log($"[{name}]: Skipping plant, because I'm afraid of it's friend.");
                continue;
            }

            Debug.Log($"[{name}]: I will be eating {plant.name} :P");
            initialTargets.Add(plantSpotTransform);
        }

        if (initialTargets.Count > 0)
        {
            targetPlant = initialTargets[0];
            isMoving = true;
            return false;
        }

        Debug.Log($"[{name}]: I did not find any edible plants.");
        return true;
    }

    //private bool IsInRange(RectTransform plantSpotTransform) => Vector2.Distance(rectTransform.anchoredPosition, plantSpotTransform.anchoredPosition) < _detectionRadius;

    private IEnumerator MoveTowardsPlant()
    {
        rectTransform.anchoredPosition = Vector2.MoveTowards(
            rectTransform.anchoredPosition,
            ((RectTransform)targetPlant).anchoredPosition,
            _moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(rectTransform.anchoredPosition, ((RectTransform)targetPlant).anchoredPosition) < 5f)
        {
            EatPlant();
            yield break;
        }
        yield return null;
    }

    private bool CanEatPlant(Plant plant) => plant.CanBeEatedBy(_type);
    private bool IsPlantProtected(ImprovedPlantSpot plantSpot) => plantSpot.IsProtectedFrom(_type);

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

    private IEnumerator MoveBackToStart()
    {
        rectTransform.anchoredPosition = Vector2.MoveTowards(
            rectTransform.anchoredPosition,
            startPosition,
            _moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(rectTransform.anchoredPosition, startPosition) < 5f)
        {
            yield break;
        }
        yield return null;
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
        UnityEditor.Handles.DrawWireDisc(rectTransform.position, Vector3.forward, _detectionRadius);
    }
#endif
}