using UnityEngine;

public class PlantSpotVar : MonoBehaviour
{
    public bool normal, shade, sun, wet, dry, barrier; // Plot environment properties
    [SerializeField] private GameObject shadeImage, sunImage, wetImage, dryImage, stoneImage; // Serialized for inspector assignment

    private void Start()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // Deactivate all images
        shadeImage?.SetActive(false);
        sunImage?.SetActive(false);
        wetImage?.SetActive(false);
        dryImage?.SetActive(false);
        stoneImage?.SetActive(false);

        // Activate the correct image based on the assigned environment
        if (shade && shadeImage) shadeImage.SetActive(true);
        else if (sun && sunImage) sunImage.SetActive(true);
        else if (wet && wetImage) wetImage.SetActive(true);
        else if (dry && dryImage) dryImage.SetActive(true);
        else if (barrier && stoneImage) stoneImage.SetActive(true);
    }
}