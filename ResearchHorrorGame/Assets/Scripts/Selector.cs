using UnityEngine;

public class Selector : MonoBehaviour
{
    private Player player;
    private int selectableLayerMask;

    internal ISelectable Selected { get; private set; }
    private ISelectable tmp;
    public float rayDistance = 20f;

    private Ray ray;
    private RaycastHit hit;


    public void Init(Player newPlayer)
    {
        selectableLayerMask = 1 << LayerMask.NameToLayer("Selectable");
        player = newPlayer;
    }

    private void Update()
    {
        ray = new Ray(player.cam.transform.position, player.cam.transform.forward);

        if (Physics.Raycast(ray, out hit, rayDistance, selectableLayerMask, QueryTriggerInteraction.Ignore))
        {
            tmp = hit.collider.gameObject.GetComponent<ISelectable>();

            if(Selected != tmp)
            {
                if (Selected != null)
                    Deselect();

                Selected = tmp;
                Select();
            }
        }
        else
        {
            if (Selected != null)
                Deselect();
        }
    }

    private void Deselect()
    {
        //Set pointer before calling OnDeselect in case the ISelectable wants to override the pointer
        HUD.pointer = HUD.PointerType.NORMAL;

        Selected.OnDeselect();
        Selected = null;
    }

    private void Select()
    {
        //Set pointer before calling OnSelect in case the ISelectable wants to override the pointer
        HUD.pointer = HUD.PointerType.HAND;

        if (Selected != null)
            Selected.OnSelect?.Invoke();
    }
}
