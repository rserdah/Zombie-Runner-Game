using UnityEngine.Events;


public interface IExecutable
{
    UnityAction<ITriggerable> ExecuteAction { get; set; }
}
