using UnityEngine;
using UnityEngine.SceneManagement;

public class RetartButton : MonoBehaviour
{
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reloads the current scene
    }
}
