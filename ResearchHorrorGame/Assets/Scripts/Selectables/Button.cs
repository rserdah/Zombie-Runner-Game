﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour, ISelectable, ITriggerable
{
    public List<IExecutable> listeners { get => ITriggerableHelper.GetListenersAsList(m_listeners); set { } }
    public MonoBehaviour[] m_listeners = new MonoBehaviour[0];
    public UnityAction<ITriggerable> TriggerAction { get; set; }
    public int targetSubMeshIndex = 0;

    private new Renderer renderer;
    private Material[] materials;
    private Material normalMaterial;
    private bool isSelected;

    public Action OnClick { get; set; }
    public Action OnSelect { get; set; }
    public Action OnDeselect { get; set; }

    public ITriggerable triggerable { get => this; set { } }


    private void Start()
    {
        SetLayer();

        renderer = GetComponentInChildren<Renderer>();
        materials = renderer.materials;

        if(targetSubMeshIndex > -1)
            normalMaterial = materials[targetSubMeshIndex];

        BindListeners();

        OnSelect += Select;
        OnDeselect += Deselect;
    }

    public void Deselect()
    {
        isSelected = false;

        if(targetSubMeshIndex > -1)
        {
            materials[targetSubMeshIndex] = normalMaterial;
            renderer.materials = materials;
        }
    }

    public void Select()
    {
        isSelected = true;

        if(targetSubMeshIndex > -1)
        {
            materials[targetSubMeshIndex] = Player.selectedMaterial;
            renderer.materials = materials;
        }
    }

    public void SetLayer() => gameObject.layer = LayerMask.NameToLayer("Selectable");

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

    public void Invoke() => TriggerAction?.Invoke(this);
}
