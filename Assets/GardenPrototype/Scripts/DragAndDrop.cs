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
    public bool protectFromRatAB; // above-below protection

    [SerializeField] private float protectionRadius = 169f; // circular protection range
    [SerializeField] private float protectionWidth = 200f;  // rectangular protection width
    [SerializeField] private float protectionHeight = 150f; // rectangular protection height

    private AudioSource soundManagerAudioSource;
    [SerializeField] private AudioClip plantPlacedSound;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;

        soundManagerAudioSource = GameObject.Find("SoundManager").GetComponent<AudioSource>();

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
            // Free up the previous spot before moving
            FreePreviousSpot();

            // Move the plant to the new valid spot
            rectTransform.position = closestSpot.position;

            // Mark the new spot as occupied
            PlantSpotVar spotData = closestSpot.GetComponent<PlantSpotVar>();
            if (spotData != null)
            {
                spotData.isOccupied = true;
            }

            // Apply protection to all plants when a new plant is placed
            UpdateAllPlantProtections();

            // Play the sound effect when the plant is placed
            if (soundManagerAudioSource != null && plantPlacedSound != null)
            {
                soundManagerAudioSource.PlayOneShot(plantPlacedSound);
            }

            if (rangeDisplay != null)
            {
                rangeDisplay.gameObject.SetActive(true);
            }
        }
        else
        {
            // Reset protection if returning to the original position
            FreePreviousSpot();
            rectTransform.anchoredPosition = originalPosition;

            if (rangeDisplay != null)
            {
                rangeDisplay.gameObject.SetActive(false);
            }
        }
    }

    private Transform FindMatchingPlantSpot()
    {
        GameObject[] plantSpots = GameObject.FindGameObjectsWithTag("PlantSpot");
        Transform bestSpot = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject spot in plantSpots)
        {
            RectTransform spotRect = spot.GetComponent<RectTransform>();
            PlantSpotVar spotData = spot.GetComponent<PlantSpotVar>();

            if (spotData != null && !spotData.isOccupied &&
                RectTransformUtility.RectangleContainsScreenPoint(spotRect, rectTransform.position, canvas.worldCamera))
            {
                if (MatchesPlantSpot(spotData))
                {
                    float distance = Vector2.Distance(spot.transform.position, rectTransform.position);
                    if (distance < closestDistance) // Pick the closest valid spot
                    {
                        closestDistance = distance;
                        bestSpot = spot.transform;
                    }
                }
            }
        }
        return bestSpot;
    }

    private void FreePreviousSpot()
    {
        GameObject[] plantSpots = GameObject.FindGameObjectsWithTag("PlantSpot");

        foreach (GameObject spot in plantSpots)
        {
            PlantSpotVar spotData = spot.GetComponent<PlantSpotVar>();
            if (spotData != null && spotData.isOccupied)
            {
                float distance = Vector2.Distance(spot.transform.position, rectTransform.position);
                if (distance < 10f)  // Adjust threshold if needed
                {
                    spotData.isOccupied = false;
                }
            }
        }
    }

    private bool MatchesPlantSpot(PlantSpotVar spot)
    {
        return (normal && spot.normal) || (shade && spot.shade) || (sun && spot.sun) || (wet && spot.wet) || (dry && spot.dry);
    }

    private void UpdateAllPlantProtections()
    {
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
        GameObject[] plantSpots = GameObject.FindGameObjectsWithTag("PlantSpot");

        // Reset all plant spots before checking
        foreach (GameObject spot in plantSpots)
        {
            PlantSpotVar spotData = spot.GetComponent<PlantSpotVar>();
            if (spotData != null)
            {
                spotData.isOccupied = false;  // Reset occupancy
            }
        }

        // Reapply protection and update occupied spots
        foreach (GameObject plant in plants)
        {
            DragAndDrop plantVars = plant.GetComponent<DragAndDrop>();
            if (plantVars != null)
            {
                Transform matchingSpot = plantVars.FindMatchingPlantSpot();
                if (matchingSpot != null)
                {
                    PlantSpotVar spotData = matchingSpot.GetComponent<PlantSpotVar>();
                    if (spotData != null)
                    {
                        spotData.isOccupied = true;  // Mark this spot as occupied
                    }
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