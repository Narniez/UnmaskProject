using UnityEngine;
using UnityEngine.SceneManagement;

public class RetartButton : MonoBehaviour
{
    [SerializeField] GameObject CheckButton;
    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // reloads the current scene
    }

    private void MakeButtonInactive()
    {
        CheckButton.SetActive(false);
    }

    public void Tutorial1()
    {
        SceneManager.LoadScene("Tutorial1");
    }

    public void Tutorial2()
    {
        SceneManager.LoadScene("Tutorial2");
    }

    public void Tutorial3()
    {
        SceneManager.LoadScene("Tutorial3");
    }

    public void Tutorial4()
    {
        SceneManager.LoadScene("Tutorial4");
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
