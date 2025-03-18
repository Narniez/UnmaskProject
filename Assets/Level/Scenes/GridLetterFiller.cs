using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GridLetterFiller : MonoBehaviour
{
    public GridLayoutGroup grid;
    public TMP_Text sequence1Text;
    public TMP_Text sequence2Text;
    public TMP_Text sequence3Text;
    public TMP_Text playerInputsText;
    public Image selectionHighlight;
    public GameObject victoryPopup;
    public TMP_Text victoryText;
    public Button nextLevelButton; // Add this in the Inspector

    private Button[] buttons;
    private bool isRowMode = true;
    private int currentRow = 0;
    private int currentColumn = -1;
    private List<Button> selectedButtons = new List<Button>();
    private List<char> sequence1Letters = new List<char>();
    private List<char> sequence2Letters = new List<char>();
    private List<char> sequence3Letters = new List<char>();
    private Dictionary<char, List<int>> sequence1Positions = new Dictionary<char, List<int>>();
    private Dictionary<char, List<int>> sequence2Positions = new Dictionary<char, List<int>>();
    private Dictionary<char, List<int>> sequence3Positions = new Dictionary<char, List<int>>();
    private HashSet<int> selectedPositions1 = new HashSet<int>();
    private HashSet<int> selectedPositions2 = new HashSet<int>();
    private HashSet<int> selectedPositions3 = new HashSet<int>();
    private string playerInputString = "";

    void Start()
    {
        FillGridWithLetters();
        InitializeButtons();
        UpdateSelectionHighlight();
        victoryPopup.SetActive(false);
        nextLevelButton.onClick.AddListener(OnNextLevelClick); // Hook up the button click
    }

    void InitializeButtons()
    {
        buttons = grid.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    void OnButtonClick(int buttonIndex)
    {
        Button clickedButton = buttons[buttonIndex];

        if (selectedButtons.Contains(clickedButton) || !IsValidSelection(buttonIndex))
            return;

        selectedButtons.Add(clickedButton);
        UpdateButtonAppearance(clickedButton);
        HighlightMatchingSequenceLetters(clickedButton);
        UpdatePlayerInputs(clickedButton);

        if (isRowMode)
        {
            currentColumn = buttonIndex % 5;
            isRowMode = false;
        }
        else
        {
            currentRow = buttonIndex / 5;
            isRowMode = true;
        }

        UpdateSelectionHighlight();
        CheckForVictory();
    }

    bool IsValidSelection(int index)
    {
        int row = index / 5;
        int col = index % 5;

        if (isRowMode)
            return row == currentRow;
        else
            return col == currentColumn;
    }

    void UpdateButtonAppearance(Button button)
    {
        button.image.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        button.interactable = false;
    }

    void UpdatePlayerInputs(Button button)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            char selectedLetter = buttonText.text[0];
            playerInputString += selectedLetter + " ";
            playerInputsText.text = playerInputString.TrimEnd();
        }
    }

    void HighlightMatchingSequenceLetters(Button button)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            char selectedLetter = buttonText.text[0];

            AddEarliestValidPosition(selectedLetter, sequence1Letters, sequence1Positions, selectedPositions1);
            AddEarliestValidPosition(selectedLetter, sequence2Letters, sequence2Positions, selectedPositions2);
            AddEarliestValidPosition(selectedLetter, sequence3Letters, sequence3Positions, selectedPositions3);

            UpdateSequenceTextHighlighting();
        }
    }

    void AddEarliestValidPosition(char letter, List<char> sequence, Dictionary<char, List<int>> positions, HashSet<int> selected)
    {
        if (positions.ContainsKey(letter))
        {
            foreach (int pos in positions[letter])
            {
                if (!selected.Contains(pos))
                {
                    bool allPreviousSelected = true;
                    for (int i = 0; i < pos; i++)
                    {
                        if (!selected.Contains(i))
                        {
                            allPreviousSelected = false;
                            break;
                        }
                    }

                    if (allPreviousSelected)
                    {
                        selected.Add(pos);
                        break;
                    }
                }
            }
        }
    }

    void UpdateSequenceTextHighlighting()
    {
        UpdateSequenceText(sequence1Text, sequence1Letters, selectedPositions1);
        UpdateSequenceText(sequence2Text, sequence2Letters, selectedPositions2);
        UpdateSequenceText(sequence3Text, sequence3Letters, selectedPositions3);
    }

    void UpdateSequenceText(TMP_Text textComponent, List<char> sequence, HashSet<int> selectedPositions)
    {
        string richText = "";

        for (int i = 0; i < sequence.Count; i++)
        {
            char c = sequence[i];
            if (selectedPositions.Contains(i))
            {
                richText += $"<color=yellow>{c}</color>";
            }
            else
            {
                richText += c;
            }
        }

        textComponent.text = richText;
    }

    void CheckForVictory()
    {
        bool allHighlighted =
            sequence1Letters.Count == selectedPositions1.Count &&
            sequence2Letters.Count == selectedPositions2.Count &&
            sequence3Letters.Count == selectedPositions3.Count;

        if (allHighlighted)
        {
            victoryPopup.SetActive(true);
            if (victoryText != null)
            {
                victoryText.text = "Good job, you did it!";
            }
        }
    }

    void OnNextLevelClick()
    {
        GenerateNewSequences();
        FillGridWithLetters();
        victoryPopup.SetActive(false); // Hide popup after generating new level
    }

    void GenerateNewSequences()
    {
        // Choose number of unique letters (3-5)
        int numLetters = Random.Range(3, 6); // 3, 4, or 5
        List<char> letterPool = new List<char>();

        // Generate unique random letters
        while (letterPool.Count < numLetters)
        {
            char newLetter = (char)Random.Range(65, 91); // A-Z
            if (!letterPool.Contains(newLetter))
            {
                letterPool.Add(newLetter);
            }
        }

        // Generate sequences of random length (2-5) using these letters
        sequence1Text.text = GenerateRandomSequence(letterPool, Random.Range(2, 6));
        sequence2Text.text = GenerateRandomSequence(letterPool, Random.Range(2, 6));
        sequence3Text.text = GenerateRandomSequence(letterPool, Random.Range(2, 6));
    }

    string GenerateRandomSequence(List<char> letters, int length)
    {
        string sequence = "";
        for (int i = 0; i < length; i++)
        {
            sequence += letters[Random.Range(0, letters.Count)];
        }
        return sequence;
    }

    void UpdateSelectionHighlight()
    {
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        float cellWidth = gridRect.rect.width / 5;
        float cellHeight = gridRect.rect.height / 5;

        RectTransform highlightRect = selectionHighlight.GetComponent<RectTransform>();

        highlightRect.anchorMin = new Vector2(0, 1);
        highlightRect.anchorMax = new Vector2(0, 1);
        highlightRect.pivot = new Vector2(0, 1);

        if (isRowMode)
        {
            highlightRect.sizeDelta = new Vector2(gridRect.rect.width, cellHeight);
            highlightRect.anchoredPosition = new Vector2(0, -cellHeight * currentRow);
        }
        else
        {
            highlightRect.sizeDelta = new Vector2(cellWidth, gridRect.rect.height);
            highlightRect.anchoredPosition = new Vector2(cellWidth * currentColumn, 0);
        }
    }

    public void FillGridWithLetters()
    {
        sequence1Letters.Clear();
        sequence2Letters.Clear();
        sequence3Letters.Clear();
        sequence1Positions.Clear();
        sequence2Positions.Clear();
        sequence3Positions.Clear();
        selectedPositions1.Clear();
        selectedPositions2.Clear();
        selectedPositions3.Clear();
        playerInputString = "";
        playerInputsText.text = "";
        victoryPopup.SetActive(false);

        PopulateSequence(sequence1Text.text, sequence1Letters, sequence1Positions);
        PopulateSequence(sequence2Text.text, sequence2Letters, sequence2Positions);
        PopulateSequence(sequence3Text.text, sequence3Letters, sequence3Positions);

        List<char> allLetters = new List<char>();
        allLetters.AddRange(sequence1Letters);
        allLetters.AddRange(sequence2Letters);
        allLetters.AddRange(sequence3Letters);
        List<char> availableLetters = allLetters.Distinct().ToList();

        if (availableLetters.Count == 0)
        {
            Debug.LogError("No letters found in sequence text boxes!");
            return;
        }

        char randomLetter = (char)Random.Range(65, 91);
        availableLetters.Add(randomLetter);

        int totalSlots = 25;
        int letterCount = availableLetters.Count;
        int baseFrequency = totalSlots / letterCount;
        int remainder = totalSlots % letterCount;

        List<char> letterPool = new List<char>();
        foreach (char letter in availableLetters)
        {
            for (int i = 0; i < baseFrequency; i++)
            {
                letterPool.Add(letter);
            }
        }

        for (int i = 0; i < remainder; i++)
        {
            char extraLetter = availableLetters[Random.Range(0, availableLetters.Count)];
            letterPool.Add(extraLetter);
        }

        ShuffleList(letterPool);

        buttons = grid.GetComponentsInChildren<Button>();
        if (buttons.Length != 25)
        {
            Debug.LogError("Grid should contain exactly 25 buttons!");
            return;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            TMP_Text buttonText = buttons[i].GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = letterPool[i].ToString();
            }
            buttons[i].image.color = Color.white;
            buttons[i].interactable = true;
        }

        selectedButtons.Clear();
        isRowMode = true;
        currentRow = 0;
        currentColumn = -1;

        sequence1Text.text = string.Join("", sequence1Letters);
        sequence2Text.text = string.Join("", sequence2Letters);
        sequence3Text.text = string.Join("", sequence3Letters);
    }

    void PopulateSequence(string text, List<char> sequenceLetters, Dictionary<char, List<int>> positions)
    {
        sequenceLetters.AddRange(text.ToCharArray());
        for (int i = 0; i < sequenceLetters.Count; i++)
        {
            char c = sequenceLetters[i];
            if (!positions.ContainsKey(c))
            {
                positions[c] = new List<int>();
            }
            positions[c].Add(i);
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}