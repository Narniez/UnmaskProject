using UnityEngine;
using UnityEngine.SceneManagement;

public class RetartButton : MonoBehaviour
{
    [SerializeField] GameObject CheckButton;
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reloads the current scene
    }

    public void MakeButtonInactive()
    {
        CheckButton.SetActive(false);
    }

    public void PuzzleFinished()
    {
        //Debug.Log(">?");
        //if (Pests.PuzzleCompleated)
        //{
        //   Debug.Log("Good Job");
        //}
        
    }
}
