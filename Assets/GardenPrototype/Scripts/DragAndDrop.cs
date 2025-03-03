using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;

    public bool normal, shade, sun, wet, dry; // Plant environment preferences
    public bool RatEats, SnailEats; // Determines if pests can eat the plant
    public bool protectedRat, protectedSnail; // Determines if the plant is protected from pests

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // No need to reset originalPosition, as it's already set in Awake
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
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
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
}
