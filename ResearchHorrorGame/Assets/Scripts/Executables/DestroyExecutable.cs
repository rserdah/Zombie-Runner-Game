using UnityEngine;
using UnityEngine.Events;

public class DestroyExecutable : MonoBehaviour, IExecutable
{
    public float delay = 1f;


    public UnityAction<ITriggerable> ExecuteAction
    {
        get => (ITriggerable _) => { Destroy(gameObject, delay); };
        set { }
    }
}
