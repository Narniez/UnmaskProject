using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RectTransform currentPosition;
    private Canvas canvas;
    private Vector2 originalPosition;
    private Image rangeDisplay;

    [SerializeField] private Plant _plant;

    private void Awake()
    {
        currentPosition = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = currentPosition.anchoredPosition;

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

        foreach (ImprovedPlantSpot spot in FindObjectsByType<ImprovedPlantSpot>(FindObjectsSortMode.None))
        {
            if (!spot.IsOccupied) continue;

            if (spot.CurrentPlant == _plant)
            {
                spot.RemovePlant();
                break;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        currentPosition.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        foreach (ImprovedPlantSpot spot in FindObjectsByType<ImprovedPlantSpot>(FindObjectsSortMode.None))
        {
            if (spot.IsOccupied) continue;

            RectTransform spotRect = spot.GetComponent<RectTransform>();

            if (RectTransformUtility.RectangleContainsScreenPoint(spotRect, currentPosition.position, canvas.worldCamera))
            {
                if (spot.TrySetPlant(_plant))
                {
                    currentPosition.position = spot.transform.position;

                    if (rangeDisplay != null)
                    {
                        rangeDisplay.gameObject.SetActive(true);
                    }
                    return;
                }
            }
        }
        ResetPos();
    }

    void ResetPos()
    {
        currentPosition.anchoredPosition = originalPosition;

        if (rangeDisplay != null)
        {
            rangeDisplay.gameObject.SetActive(false);
        }
    }


    private void OnDrawGizmos()
    {
        if (!_plant.Protection.DoesProtect) return;

        if (currentPosition == null) return;

        switch (_plant.Protection.Type)
        {
            case ProtectionType.Circular:
                DrawCircularProtection(_plant.Protection.EffectiveUnits);
                break;
            case ProtectionType.AboveBelow:
                DrawBoxProtection(new(50, _plant.Protection.EffectiveUnits));
                break;
            case ProtectionType.LeftRight:
                DrawBoxProtection(new(_plant.Protection.EffectiveUnits, 50));
                break;
        }
    }
    private void DrawCircularProtection(float radius)
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(currentPosition.position, Vector3.forward, radius);
    }

    private void DrawBoxProtection(Vector2 size)
    {
        Handles.color = Color.yellow;
        Handles.DrawWireCube(currentPosition.position, size);
    }
}