using UnityEngine;
using UnityEngine.SceneManagement;

public class RetartButton : MonoBehaviour
{
    [SerializeField] GameObject CheckButton;
    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reloads the current scene
    }

    private void MakeButtonInactive()
    {
        CheckButton.SetActive(false);
    }

    public void Level1()
    {
        SceneManager.LoadScene("Puzzle1");
    }

    public void Level2()
    {
        SceneManager.LoadScene("Puzzle2");
    }

    public void Level3()
    {
        SceneManager.LoadScene("Puzzle3");
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
