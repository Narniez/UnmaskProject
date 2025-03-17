using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static void AddPuzzleCondition(Func<bool> method)
    {
        if (!TryGetGameManager(out GameManager gm))
        {
            Debug.LogWarning("No GameManager found in the scene!");
            return;
        }

        gm._onCheckPuzzle += method;
    }

    private static bool TryGetGameManager(out GameManager gm)
    {
        gm = FindAnyObjectByType<GameManager>();
        return gm != null;
    }

    public event Action OnPuzzleRestarted;

    public event Action OnPuzzleSuccessful;
    public event Action OnPuzzleUnsuccessful;

    private event Func<bool> _onCheckPuzzle;

    private void Start()
    {
        OnPuzzleSuccessful += () => Debug.Log("Puzzle successful!");
        OnPuzzleUnsuccessful += () => Debug.Log("Puzzle unsuccessful!");

        Debug.Log("GAME MANAGER STARTED!!!");
    }

    public void CheckPuzzle()
    {
        Delegate[] delegates = _onCheckPuzzle?.GetInvocationList();

        if (delegates.Length == 0)
        {
            Debug.LogWarning("No methods subscribed to OnCheckPuzzle event!");
            return;
        }

        IEnumerable<Func<bool>> functions = delegates.Cast<Func<bool>>();

        bool willSucceed = true;

        foreach (Func<bool> method in functions)
        {
            willSucceed &= method();
        }

        if (willSucceed)
            OnPuzzleSuccessful.Invoke();
        else
            OnPuzzleUnsuccessful.Invoke();
    }

    public void RestartPuzzle() => OnPuzzleRestarted?.Invoke();
}
