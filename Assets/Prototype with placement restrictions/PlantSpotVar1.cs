using UnityEngine;
using UnityEngine.UI;

public class PlantSpotVar1 : MonoBehaviour
{
    public bool sun, wet, sand, grass;
    [SerializeField] private Image sunImage;
    private Image tileImage;

    private void Awake()
    {
        tileImage = GetComponent<Image>();
        if (tileImage == null)
        {
            Debug.LogError("Tile GameObject must have an Image component!", this);
        }
    }

    private void Start()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (sunImage != null)
        {
            sunImage.gameObject.SetActive(sun);
        }

        Color defaultColor = Color.white;
        tileImage.color = defaultColor;

        if (sun || grass)
        {
            if (ColorUtility.TryParseHtmlString("#427430", out Color greenColor))
            {
                tileImage.color = greenColor;
            }
        }
        else if (sand)
        {
            if (ColorUtility.TryParseHtmlString("#CBBE78", out Color sandColor))
            {
                tileImage.color = sandColor;
            }
        }
        else if (wet)
        {
            if (ColorUtility.TryParseHtmlString("#61A0B3", out Color waterColor))
            {
                tileImage.color = waterColor;
            }
        }
    }
}