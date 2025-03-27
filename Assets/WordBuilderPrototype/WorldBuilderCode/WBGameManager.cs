using System.ComponentModel;
using UnityEngine;

public class WBGameManager : MonoBehaviour
{


    private static WBGameManager _instance;

    public bool canInteractWithTiles = true;


    public static WBGameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<WBGameManager>();
            }

            return _instance;
        }
    }
}
