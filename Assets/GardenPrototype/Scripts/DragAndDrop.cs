using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Image rangeDisplay; // Image named "RangeDisplay"

    public bool normal, shade, sun, wet, dry; // Plant environment preferences
    public bool RatEats, SnailEats; // Determines if pests can eat the plant
    public bool protectedRat, protectedSnail; // Determines if the plant is protected from pests
    public bool protectsFromRat, protectsFromSnail; // Determines if it protects others

    public float protectionRadius = 169f; // Range to protect nearby plants

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;

        // Find the child object specifically named "RangeDisplay"
        Transform childTransform = transform.Find("RangeDisplay");
        if (childTransform != null)
        {
            rangeDisplay = childTransform.GetComponent<Image>();
        }

        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(false); // Hide at the start
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(true); // Show when dragging starts
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
            ApplyProtection();

            if (rangeDisplay != null)
            {
                rangeDisplay.gameObject.SetActive(true); // Keep image visible when placed
            }
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;

            if (rangeDisplay != null)
            {
                rangeDisplay.gameObject.SetActive(false); // Hide image if reset
            }
        }
    }

    private Transform FindMatchingPlantSpot()
    {
        GameObject[] plantSpots = GameObject.FindGameObjectsWithTag("PlantSpot");
        foreach (GameObject spot in plantSpots)
        {
            RectTransform spotRect = spot.GetComponent<RectTransform>();
            PlantSpotVar spotData = spot.GetComponent<PlantSpotVar>(); // Get spot properties
            if (spotData != null && RectTransformUtility.RectangleContainsScreenPoint(spotRect, rectTransform.position, canvas.worldCamera))
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
            if (plant == this.gameObject) continue; // Skip self

            RectTransform plantRect = plant.GetComponent<RectTransform>();
            DragAndDrop plantVars = plant.GetComponent<DragAndDrop>();

            if (plantVars != null && IsInRange(plantRect))
            {
                if (protectsFromRat)
                {
                    plantVars.protectedRat = true;
                }
                if (protectsFromSnail)
                {
                    plantVars.protectedSnail = true;
                }
            }
        }
    }

    private bool IsInRange(RectTransform plantRect)
    {
        float distance = Vector2.Distance(rectTransform.anchoredPosition, plantRect.anchoredPosition);
        return distance < protectionRadius;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (rectTransform == null) return;

        UnityEditor.Handles.color = Color.green;
        UnityEditor.Handles.DrawWireDisc(rectTransform.position, Vector3.forward, protectionRadius);
    }
#endif
}
