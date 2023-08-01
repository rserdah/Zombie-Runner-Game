using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionTriggerable : MonoBehaviour, ITriggerable
{
    public List<IExecutable> listeners { get => ITriggerableHelper.GetListenersAsList(m_listeners); set { } }
    public MonoBehaviour[] m_listeners = new MonoBehaviour[0];
    public UnityAction<ITriggerable> TriggerAction { get; set; }

    public bool onTriggerEnter = true;
    public bool onCollisionEnter = false;
    public bool onCollisionExit = false;

    public LayerMask detectLayer = Physics.AllLayers;
    public string detectTag;
    public string detectName;
    public GameObject detectGameObject;


    private void Start()
    {
        BindListeners();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(onTriggerEnter)
            HandleCollision(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(onCollisionEnter)
            HandleCollision(collision.collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        if(onCollisionExit)
            HandleCollision(collision.collider);
    }

    private void HandleCollision(Collider other)
    {
        if(other)
            if(
                detectLayer != ~Physics.AllLayers && ContainsLayer(detectLayer, other.gameObject.layer) || 
                !string.IsNullOrEmpty(detectTag) && detectTag.Equals(other.gameObject.tag) || 
                !string.IsNullOrEmpty(detectName) && detectName.Equals(other.gameObject.name) ||
                detectGameObject != null && other.gameObject.Equals(detectGameObject))
            {
                TriggerAction?.Invoke(this);
            }
    }

    public void BindListeners()
    {
        IExecutable e;
        foreach(MonoBehaviour m in m_listeners)
        {
            e = (IExecutable)m;
            if(e != null)
                TriggerAction += e.ExecuteAction;
        }
    }

    private bool ContainsLayer(LayerMask layerMask, int layer)
    {
        int checkLayerMask = 1 << layer;
        return (detectLayer & checkLayerMask) == checkLayerMask;
    }

    //Other classes should not call Invoke on CollisionTriggerable's
    public void Invoke() { }
}
