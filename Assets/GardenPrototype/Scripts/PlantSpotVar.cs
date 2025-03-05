using UnityEngine;

public class PlantSpotVar : MonoBehaviour
{
    public bool normal, shade, sun, wet, dry, barrier; // plot environment properties
    public bool isOccupied = false;
    [SerializeField] private GameObject shadeImage, sunImage, wetImage, dryImage, stoneImage;

    private void Start()
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // deactivate all images
        shadeImage?.SetActive(false);
        sunImage?.SetActive(false);
        wetImage?.SetActive(false);
        dryImage?.SetActive(false);
        stoneImage?.SetActive(false);

        // activate the correct image based on the assigned environment
        if (shade && shadeImage) shadeImage.SetActive(true);
        else if (sun && sunImage) sunImage.SetActive(true);
        else if (wet && wetImage) wetImage.SetActive(true);
        else if (dry && dryImage) dryImage.SetActive(true);
        else if (barrier && stoneImage) stoneImage.SetActive(true);
    }
}