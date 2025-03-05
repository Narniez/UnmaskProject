using UnityEngine;

public class TutorialText : MonoBehaviour
{
    void Start()
    {
        Invoke("HideTutorial", 10f); // Call HideTutorial after 10 seconds
    }

    void HideTutorial()
    {
        gameObject.SetActive(false); // Disable the tutorial box
    }
}
