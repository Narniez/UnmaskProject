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
    public bool protectedRat, protectedSnail; // <-- Protection is restored

    public bool protectsFromRat, protectsFromSnail;  // Circular protection
    public bool protectFromRatAB; // Above-below protection

    [SerializeField] private float protectionRadius = 169f; // Circular protection range
    [SerializeField] private float protectionWidth = 200f;  // Rectangular protection width
    [SerializeField] private float protectionHeight = 150f; // Rectangular protection height

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
            FreePreviousSpot();

            rectTransform.position = closestSpot.position;

            PlantSpotVar spotData = closestSpot.GetComponent<PlantSpotVar>();
            if (spotData != null)
            {
                spotData.isOccupied = true;
            }

            UpdateAllPlantProtections(); // <-- Protection is now updated again

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
                    if (distance < closestDistance)
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
                if (distance < 10f)
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

        foreach (GameObject plant in plants)
        {
            DragAndDrop plantVars = plant.GetComponent<DragAndDrop>();
            if (plantVars != null)
            {
                plantVars.protectedRat = false;
                plantVars.protectedSnail = false;
            }
        }

        foreach (GameObject plant in plants)
        {
            DragAndDrop plantVars = plant.GetComponent<DragAndDrop>();
            if (plantVars != null)
            {
                plantVars.ApplyProtection();
            }
        }
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
                if (protectsFromRat || protectFromRatAB)
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

    private bool IsInCircleRange(RectTransform plantRect)
    {
        float distance = Vector2.Distance(rectTransform.anchoredPosition, plantRect.anchoredPosition);
        return (protectsFromRat || protectsFromSnail) && distance < protectionRadius;
    }

    private bool IsInBoxRange(RectTransform plantRect)
    {
        if (!protectFromRatAB) return false;

        float xDifference = Mathf.Abs(plantRect.anchoredPosition.x - rectTransform.anchoredPosition.x);
        float yDifference = Mathf.Abs(plantRect.anchoredPosition.y - rectTransform.anchoredPosition.y);

        return xDifference < (protectionWidth / 2) && yDifference < (protectionHeight / 2);
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

        if (protectsFromRat || protectsFromSnail)
        {
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(position, Vector3.forward, protectionRadius);
        }

        if (protectFromRatAB)
        {
            UnityEditor.Handles.color = Color.green;
            Vector3 size = new Vector3(protectionWidth, protectionHeight, 1);
            UnityEditor.Handles.DrawWireCube(position, size);
        }
    }
#endif
}