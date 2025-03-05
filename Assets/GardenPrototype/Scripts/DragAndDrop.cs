using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Image rangeDisplay;

    public bool normal, shade, sun, wet, dry;
    public bool RatEats, SnailEats;
    public bool protectedRat, protectedSnail;

    public bool protectsFromRat, protectsFromSnail;  // circular protection
    public bool protectFromRatAB; //above-below protection

    [SerializeField] private float protectionRadius = 169f; // circular protection range
    [SerializeField] private float protectionWidth = 200f;  // rectangular protection width
    [SerializeField] private float protectionHeight = 150f; // rectangular protection height

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;

        Transform childTransform = transform.Find("RangeDisplay");
        if (childTransform != null)
        {
            rangeDisplay = childTransform.GetComponent<Image>();
        }

        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Transform closestSpot = FindMatchingPlantSpot();
        if (closestSpot != null)
        {
            rectTransform.position = closestSpot.position;

            // Mark the spot as occupied
            PlantSpotVar spotData = closestSpot.GetComponent<PlantSpotVar>();
            if (spotData != null)
            {
                spotData.isOccupied = true;
            }

            // Apply protection to all plants when a new plant is placed
            UpdateAllPlantProtections();

            if (rangeDisplay != null)
            {
                rangeDisplay.gameObject.SetActive(true);
            }
        }
        else
        {
            // Reset protection if returning to the original position
            ResetProtection();
            rectTransform.anchoredPosition = originalPosition;

            if (rangeDisplay != null)
            {
                rangeDisplay.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateAllPlantProtections()
    {
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");

        // Reset protection for all plants first
        foreach (GameObject plant in plants)
        {
            DragAndDrop plantVars = plant.GetComponent<DragAndDrop>();
            if (plantVars != null)
            {
                plantVars.protectedRat = false;
                plantVars.protectedSnail = false;
            }
        }

        // Reapply protection based on the current state of the scene
        foreach (GameObject plant in plants)
        {
            DragAndDrop plantVars = plant.GetComponent<DragAndDrop>();
            if (plantVars != null)
            {
                plantVars.ApplyProtection();
            }
        }
    }

    private Transform FindMatchingPlantSpot()
    {
        GameObject[] plantSpots = GameObject.FindGameObjectsWithTag("PlantSpot");
        foreach (GameObject spot in plantSpots)
        {
            RectTransform spotRect = spot.GetComponent<RectTransform>();
            PlantSpotVar spotData = spot.GetComponent<PlantSpotVar>();

            // Check if spot is valid and not occupied
            if (spotData != null && !spotData.isOccupied && RectTransformUtility.RectangleContainsScreenPoint(spotRect, rectTransform.position, canvas.worldCamera))
            {
                if (MatchesPlantSpot(spotData))
                {
                    return spot.transform;
                }
            }
        }
        return null;
    }

    private bool MatchesPlantSpot(PlantSpotVar spot)
    {
        return (normal && spot.normal) || (shade && spot.shade) || (sun && spot.sun) || (wet && spot.wet) || (dry && spot.dry);
    }

    private void ApplyProtection()
    {
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
        foreach (GameObject plant in plants)
        {
            if (plant == this.gameObject) continue;

            RectTransform plantRect = plant.GetComponent<RectTransform>();
            DragAndDrop plantVars = plant.GetComponent<DragAndDrop>();

            if (plantVars != null && (IsInCircleRange(plantRect) || IsInBoxRange(plantRect)))
            {
                if (protectsFromRat || protectFromRatAB)  // Protect from Rat in either range
                {
                    plantVars.protectedRat = true;
                }
                if (protectsFromSnail)  // Protect from Snail only in circular range
                {
                    plantVars.protectedSnail = true;
                }
            }
        }
    }

    private bool IsInCircleRange(RectTransform plantRect)
    {
        float distance = Vector2.Distance(rectTransform.anchoredPosition, plantRect.anchoredPosition);
        return protectsFromRat || protectsFromSnail ? distance < protectionRadius : false;
    }

    private bool IsInBoxRange(RectTransform plantRect)
    {
        if (!protectFromRatAB) return false;  // Only check if protectFromRatAB is true

        float xDifference = Mathf.Abs(plantRect.anchoredPosition.x - rectTransform.anchoredPosition.x);
        float yDifference = Mathf.Abs(plantRect.anchoredPosition.y - rectTransform.anchoredPosition.y);

        return xDifference < (protectionWidth / 2) && yDifference < (protectionHeight / 2);
    }

    private void ResetProtection()
    {
        protectedRat = false;
        protectedSnail = false;

        // Free up the spot when plant is removed
        GameObject[] plantSpots = GameObject.FindGameObjectsWithTag("PlantSpot");
        foreach (GameObject spot in plantSpots)
        {
            if (Vector2.Distance(spot.transform.position, rectTransform.position) < 10f)
            {
                PlantSpotVar spotData = spot.GetComponent<PlantSpotVar>();
                if (spotData != null)
                {
                    spotData.isOccupied = false;
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!protectsFromRat && !protectsFromSnail && !protectFromRatAB) return;

        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null) return;
        }

        Vector3 position = rectTransform.position;

        // Draw circular protection if enabled
        if (protectsFromRat || protectsFromSnail)
        {
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(position, Vector3.forward, protectionRadius);
        }

        // Draw box protection if enabled
        if (protectFromRatAB)
        {
            UnityEditor.Handles.color = Color.green;
            Vector3 size = new Vector3(protectionWidth, protectionHeight, 1);
            UnityEditor.Handles.DrawWireCube(position, size);
        }
    }
#endif
}