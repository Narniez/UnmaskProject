using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    public void scene_changer(int scene_number)
    {
        SceneManager.LoadScene(scene_number);
    }
}
