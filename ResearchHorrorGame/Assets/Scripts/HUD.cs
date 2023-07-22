using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public enum PointerType
    {
        NORMAL,
        HAND,
        STRING,
        UNAVAILABLE
    }

    private static Image pointerImage { get; set; }
    private static Text pointerText { get; set; }
    public static PointerType pointer { get => m_pointer; set { m_pointer = value; SetPointerType(); } }
    private static PointerType m_pointer;
    public static string pointerTextTip { get => m_pointerTextTip; set { m_pointerTextTip = value; pointerText.text = m_pointerTextTip; if(!value.Equals("")) pointer = PointerType.STRING; } }
    private static string m_pointerTextTip;
    private static Dictionary<string, Sprite> pointerSprites = new Dictionary<string, Sprite>();
    public static HUD Instance { get; private set; }

    public Text health;
    public Text stamina;
    public Text energy;
    public Text water;
    public Text clues;


    private void Start()
    {
        if(Instance)
            gameObject.SetActive(false);

        Instance = this;

        pointerImage = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        pointerText = transform.GetChild(0).GetChild(1).GetComponent<Text>();

        GetPointerSprites();

        Player.hudCallback += UpdateHUD;
    }

    public void UpdateHUD(Player.Stats stats)
    {
        health.text = "Health: " + Mathf.CeilToInt(stats.health);
        stamina.text = "Stamina: " + Mathf.CeilToInt(stats.stamina);
        energy.text = "Energy: " + Mathf.CeilToInt(stats.energy);
        water.text = "Water: " + Mathf.CeilToInt(stats.water);

        clues.text = "";
        foreach(string s in Player.player.activeClues.Values)
            clues.text += "\n" + s;
    }

    private static void GetPointerSprites()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("Textures/Sprites/Pointer");

        foreach(Sprite s in sprites)
            pointerSprites.Add(s.name, s);
    }

    private static void SetPointerType()
    {
        switch(m_pointer)
        {
            case PointerType.NORMAL:
            case PointerType.HAND:
            case PointerType.UNAVAILABLE:
                Sprite sprite;
                if(pointerSprites.TryGetValue(m_pointer.ToString(), out sprite))
                {
                    pointerImage.sprite = sprite;
                    pointerImage.SetNativeSize();
                }

                SetPointerAsImage();
                break;

            case PointerType.STRING:
                SetPointerAsImage(false);
                break;
        }
    }

    /// <summary>
    /// Should the pointer image be used or the pointer text, defaults to using the pointer image if not specified
    /// </summary>
    /// <param name="useImage"></param>
    private static void SetPointerAsImage(bool useImage = true)
    {
        pointerImage.gameObject.SetActive(useImage);
        pointerText.gameObject.SetActive(!useImage);
    }
}
