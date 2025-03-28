using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDrop1 : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Image rangeDisplay;
    private CanvasGroup canvasGroup;

    public bool normal, sun, wet, dry;

    private AudioSource soundManagerAudioSource;
    [SerializeField] private AudioClip plantPlacedSound;

    private GridLetterFiller gridLetterFiller;
    private Transform currentSpot;
    private bool isPlaced = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;

        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Initialize as non-interactable
        SetInteractable(false);

        soundManagerAudioSource = GameObject.Find("SoundManager")?.GetComponent<AudioSource>();

        Transform childTransform = transform.Find("RangeDisplay");
        if (childTransform != null)
        {
            rangeDisplay = childTransform.GetComponent<Image>();
        }

        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(false);
        }

        gridLetterFiller = FindFirstObjectByType<GridLetterFiller>();
        if (gridLetterFiller == null)
        {
            Debug.LogError("GridLetterFiller not found in the scene!", this);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPlaced || !canvasGroup.interactable)
        {
            return;
        }

        if (!gridLetterFiller.IsNextPlantToPlace(transform))
        {
            return;
        }

        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced || !canvasGroup.interactable)
        {
            return;
        }

        if (canvas == null) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPlaced || !canvasGroup.interactable)
        {
            return;
        }

        Transform closestSpot = FindMatchingPlantSpot(eventData.position);

        if (closestSpot != null)
        {
            int tileIndex = GetTileIndex(closestSpot);
            if (tileIndex == -1)
            {
                ResetToOriginalPosition();
                return;
            }

            if (gridLetterFiller.IsValidPlacement(tileIndex))
            {
                FreePreviousSpot();

                rectTransform.position = closestSpot.position;
                currentSpot = closestSpot;

                if (soundManagerAudioSource != null && plantPlacedSound != null)
                {
                    soundManagerAudioSource.PlayOneShot(plantPlacedSound);
                }

                if (rangeDisplay != null)
                {
                    rangeDisplay.gameObject.SetActive(true);
                }

                isPlaced = true;
                gridLetterFiller.OnPlantPlaced(transform, tileIndex);
            }
            else
            {
                ResetToOriginalPosition();
            }
        }
        else
        {
            FreePreviousSpot();
            ResetToOriginalPosition();
        }
    }

    private void ResetToOriginalPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
        currentSpot = null;

        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(false);
        }
    }

    private int GetTileIndex(Transform spot)
    {
        RectTransform[] tiles = gridLetterFiller.GetTiles();
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].transform == spot)
            {
                return i;
            }
        }
        return -1;
    }

    private Transform FindMatchingPlantSpot(Vector2 mousePosition)
    {
        GameObject[] plantSpots = GameObject.FindGameObjectsWithTag("PlantSpot");
        Transform bestSpot = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject spot in plantSpots)
        {
            RectTransform spotRect = spot.GetComponent<RectTransform>();
            PlantSpotVar1 spotData = spot.GetComponent<PlantSpotVar1>();

            if (spotData == null)
            {
                continue;
            }

            bool isMouseOver = RectTransformUtility.RectangleContainsScreenPoint(
                spotRect,
                mousePosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera
            );

            if (isMouseOver)
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
        currentSpot = null;
    }

    private bool MatchesPlantSpot(PlantSpotVar1 spot)
    {
        bool spotNormal = spot.grass;
        bool spotDry = spot.sand;
        bool spotWet = spot.wet;

        return (normal && spotNormal) || (sun && spot.sun) || (wet && spotWet) || (dry && spotDry);
    }

    public void ResetForNewLevel()
    {
        isPlaced = false;
        currentSpot = null;
        rectTransform.anchoredPosition = originalPosition;
        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(false);
        }
        SetInteractable(false);
    }

    public bool IsPlaced()
    {
        return isPlaced;
    }

    public void SetInteractable(bool interactable)
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
            canvasGroup.alpha = interactable ? 1f : 0.5f;
        }
    }
}