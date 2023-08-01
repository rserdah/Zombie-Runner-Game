using UnityEngine;
using UnityEngine.Events;

public class SendMessageExecutable : MonoBehaviour, IExecutable
{
    public UnityAction<ITriggerable> ExecuteAction
    {
        get => Execute;
        set { }
    }

    [Tooltip("Send the message to this MonoBehaviour and every ansestor of it or just this MonoBehaviour?")]
    public bool sendMessageUpwards = false;
    public string methodName;
    public Object paramObject;
    public SendMessageOptions sendMessageOptions;


    private void Execute(ITriggerable triggerable)
    {
        //if(paramObject)
        {
            if(sendMessageUpwards)
                gameObject.SendMessageUpwards(methodName, paramObject, sendMessageOptions);
            else
                gameObject.SendMessage(methodName, paramObject, sendMessageOptions);
        }

    }
}
