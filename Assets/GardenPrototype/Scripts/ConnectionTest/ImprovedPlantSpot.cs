using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ImprovedPlantSpot : MonoBehaviour
{
    [SerializeField]
    private Image _renderer;

    public PlantProperties SpotProperty;
    public Plant CurrentPlant { get; private set; } = null;
    public bool IsOccupied => CurrentPlant != null;

    public bool IsProtectedFrom(PestType pestType)
    {
        ImprovedPlantSpot[] improvedPlantSpots = FindObjectsByType<ImprovedPlantSpot>(FindObjectsSortMode.None);

        foreach (ImprovedPlantSpot plantSpot in improvedPlantSpots)
        {
            if (!plantSpot.IsOccupied)
                continue;

            Plant plant = plantSpot.CurrentPlant;
        }

        foreach (Plant plant in FindObjectsByType<Plant>(FindObjectsSortMode.None))
        {
            if (plant.Protection.EffectiveAgainst != pestType)
                continue;

            if (plant.Protection.Type == ProtectionType.Circular)
            {
                if (IsInCircleRange(plant.GetComponent<RectTransform>().position, plant.Protection.EffectiveUnits))
                    return true;
            }
            else if (plant.Protection.Type == ProtectionType.AboveBelow)
            {
                if (IsInBoxRange(plant.GetComponent<RectTransform>().position, new Vector2(plant.Protection.EffectiveUnits, plant.Protection.EffectiveUnits)))
                    return true;
            }
        }
        return false;
    }

    private bool IsInCircleRange(Vector2 center, float radius) => Vector2.Distance(center, GetComponent<RectTransform>().position) < radius;

    private bool IsInBoxRange(Vector2 center, Vector2 protectionUnits)
    {
        RectTransform spotTransform = GetComponent<RectTransform>();
        float xDifference = Mathf.Abs(spotTransform.anchoredPosition.x - center.x);
        float yDifference = Mathf.Abs(spotTransform.anchoredPosition.y - center.y);

        return xDifference < (protectionUnits.x / 2) && yDifference < (protectionUnits.y / 2);
    }

    private void Start()
    {
        Sprite spr = FindAnyObjectByType<PropertySpriteLookupTable>().FromProperty(SpotProperty);
        if (spr == null)
            return;

        _renderer.enabled = true;
        _renderer.sprite = spr;
    }

    public bool TrySetPlant(Plant plant)
    {
        if (IsOccupied || SpotProperty != plant.Property)
            return false;

        CurrentPlant = plant;
        return true;
    }
    public void RemovePlant() => CurrentPlant = null;

    private void OnDrawGizmos()
    {
        if (!IsOccupied)
            return;
        //Gizmos.color = IsProtectedFrom(PestType.Bug) ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 200);
    }
}