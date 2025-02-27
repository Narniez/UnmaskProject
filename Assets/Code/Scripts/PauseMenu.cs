using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject PausePane1;

    private void Update()
    {
        
    }

    public void Pause()
    {
        PausePane1.SetActive(true);
        Time.timeScale = 0;
        Debug.Log("Paused");
    }

    public void Continue()
    {
        PausePane1.SetActive(false);
        Time.timeScale = 1;
        Debug.Log("Continued");
    }
}
