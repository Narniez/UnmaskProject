using UnityEngine;

public class InstantiateOncePersistentBase<T> : MonoBehaviour where T: MonoBehaviour
{
    private void Awake()
    {
        if (FindObjectsByType<T>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
