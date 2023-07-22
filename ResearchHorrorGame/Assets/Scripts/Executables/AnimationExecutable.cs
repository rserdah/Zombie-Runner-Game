using UnityEngine;
using UnityEngine.Events;

public class AnimationExecutable : MonoBehaviour, IExecutable
{
    private Animator anim;

    public string onExecuteAnimation;


    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public UnityAction<ITriggerable> ExecuteAction
    {
        get => Execute;
        set { }
    }

    private void Execute(ITriggerable triggerable)
    {
        anim.Play(onExecuteAnimation);
    }
}
