using System;

public interface ISelectable
{
    Action OnClick { get; set; }
    Action OnSelect { get; set; }
    Action OnDeselect { get; set; }

    ITriggerable triggerable { get; set; }

    void SetLayer();

    //void Select();

    //void Deselect();
}
