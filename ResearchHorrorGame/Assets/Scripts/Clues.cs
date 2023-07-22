using System.Collections.Generic;
using UnityEngine;

public class Clues : MonoBehaviour
{
    public static Dictionary<string, string> clues = new Dictionary<string, string>();
    public static Clues Instance { get; private set; }
    private static readonly string folder = "Clues/";


    private void Start()
    {
        if(Instance != null)
            gameObject.SetActive(false);

        InitClues();

        Instance = this;
    }

    private static void InitClues()
    {
        Clue[] allClues = Resources.LoadAll<Clue>(folder);

        foreach(Clue c in allClues)
        {
            clues.Add(c.key, c.value);
        }
    }

    private static void PrintAllClues()
    {
        foreach(KeyValuePair<string, string> k in clues)
        {
            Debug.LogError(k.Key + ": " + k.Value);
        }
    }
}
