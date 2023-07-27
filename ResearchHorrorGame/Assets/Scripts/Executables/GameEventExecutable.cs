using UnityEngine;
using UnityEngine.Events;

public class GameEventExecutable : MonoBehaviour, IExecutable
{
    public Game.GameEventType gameEventType;

    public UnityAction<ITriggerable> ExecuteAction
    {
        get => Execute;
        set { }
    }

    public void Execute(ITriggerable triggerable)
    {
        Game.RequestGameEventInvoke(gameEventType);
    }
}
