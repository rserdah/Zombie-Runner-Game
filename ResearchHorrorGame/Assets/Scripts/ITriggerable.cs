using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface ITriggerable
{
    List<IExecutable> listeners { get; set; }


    void BindListeners();

    UnityAction<ITriggerable> TriggerAction { get; set; }

    void Invoke();
}

public class ITriggerableHelper
{
    public static List<IExecutable> GetListenersAsList(MonoBehaviour[] allListeners)
    {
        List<IExecutable> listeners = new List<IExecutable>();

        IExecutable e;
        foreach(MonoBehaviour m in allListeners)
        {
            e = (IExecutable)m;
            if(e != null)
                listeners.Add(e);
        }

        return listeners;
    }
}
