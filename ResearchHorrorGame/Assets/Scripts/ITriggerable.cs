using System.Collections.Generic;
using UnityEngine.Events;

public interface ITriggerable
{
    List<IExecutable> listeners { get; set; }


    void BindListeners();

    UnityAction<ITriggerable> TriggerAction { get; set; }

    void Invoke();
}
