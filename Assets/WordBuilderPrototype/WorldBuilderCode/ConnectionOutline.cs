using UnityEngine;
using TMPro;
using System.Collections;

public class ConnectionOutline : MonoBehaviour
{
    private HexaTileManager hexTileManager;  
   [SerializeField] private HexTile hexTile;  

    private DiceRollUIManager diceRollUIManager;


    private void OnEnable()
    {
        hexTile = GetComponentInParent<HexTile>();
        hexTileManager = FindFirstObjectByType<HexaTileManager>();
        diceRollUIManager = FindFirstObjectByType<DiceRollUIManager>();
    }

    private void OnMouseDown()
    {
        if (!hexTile.canBeClicked || !WBGameManager.Instance.canInteractWithTiles) return;
        //Debug.Log("Clicking on tile!");
        // Trigger dice roll when the outline is clicked
        if (diceRollUIManager != null && hexTile != null)
        {
           
            diceRollUIManager.StartDiceRoll(hexTile); 
           //Debug.Log("Should start dice roll!");
        }
        else
        {
            Debug.LogError("DiceRollUIManager or HexTile not found!");
        }
    }

}
