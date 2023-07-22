using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Game Items/Clue", order = 1)]
public class Clue : ScriptableObject
{
    public string key;
    public string value;
}