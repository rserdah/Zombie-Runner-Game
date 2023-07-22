using UnityEngine;
using UnityEngine.Events;

public class ClueExecutable : MonoBehaviour, IExecutable
{
    /// <summary>
    /// Does executing this add or remove a Clue(s) to the player's activeClues?
    /// </summary>
    public bool addClue = true;
    public Clue[] clues;


    public UnityAction<ITriggerable> ExecuteAction
    {
        get => (ITriggerable _) => { foreach(Clue c in clues) if(addClue) Player.player.AddClue(c.key); else Player.player.UseClue(c.key); };
        set { }
    }
}
