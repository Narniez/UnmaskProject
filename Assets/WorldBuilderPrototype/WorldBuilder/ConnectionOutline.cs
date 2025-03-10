using UnityEngine;
using TMPro;
using System.Collections;

public class ConnectionOutline : MonoBehaviour
{
    private HexaTileManager hexTileManager;  // Reference to HexTileManager
   [SerializeField] private HexTile hexTile;  // Reference to HexTile

    private DiceRollUIManager diceRollUIManager;  // Reference to DiceRollUIManager

    private void OnEnable()
    {
        hexTile = GetComponentInParent<HexTile>();
        hexTileManager = FindFirstObjectByType<HexaTileManager>();
        diceRollUIManager = FindFirstObjectByType<DiceRollUIManager>();
    }

    private void OnMouseDown()
    {
        // Trigger dice roll when the outline is clicked
        if (diceRollUIManager != null)
        {
            diceRollUIManager.StartDiceRoll(hexTile);  // Pass tile's world position
        }
        else
        {
            Debug.LogError("DiceRollUIManager not found!");
        }
    }

}
