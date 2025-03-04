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
        SceneManager.LoadScene("Puzzle 1");
    }

    public void Level2()
    {
        SceneManager.LoadScene("Puzzle 2");
        Debug.Log("SERBAN");
    }

    public void Level3()
    {
        SceneManager.LoadScene("Puzzle 3");
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
