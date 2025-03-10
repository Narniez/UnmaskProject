using UnityEngine;
using TMPro;
using System.Collections;

public class DiceRollUIManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI diceText;  
    [SerializeField] private GameObject diceRollPannel;
    [SerializeField] private HexaTileManager tileManager;

    int diceRoll = 0;

    private void Start()
    {

    }

    public void StartDiceRoll(HexTile _tile)
    {
        diceRollPannel.SetActive(true);
        StartCoroutine(RollDice(_tile));
 
    }

    private IEnumerator RollDice(HexTile tile)
    {
        float rollDuration = 3f;
        float elapsedTime = 0f;

        // Start rolling
        while (elapsedTime < rollDuration)
        {
            int randomNumber = Random.Range(1, 7);
            diceText.text = randomNumber.ToString();
            yield return new WaitForSeconds(0.1f);  // Update every 0.1s
            elapsedTime += 0.1f;
        }

        // Final roll result
        int finalRoll = Random.Range(1, 7);
        diceRoll = finalRoll;
        diceText.text = finalRoll.ToString();

        diceText.text = "You rolled: " + finalRoll.ToString();
        // Optionally, you could add a small delay before hiding the dice text
        yield return new WaitForSeconds(4f);

        diceText.text = string.Empty;
        tileManager.CheckDiceRoll(diceRoll, tile);

    }
}
