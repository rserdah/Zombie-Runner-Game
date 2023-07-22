using UnityEngine;
using UnityEngine.UI;

public class ComputerScreen : MonoBehaviour
{
    public Text screenText;
    private string initialText;
    public float blinkSpeed = 0.5f;
    private int count = 0;


    private void Start()
    {
        //Assumes initial text does not include the cursor
        initialText = screenText.text;
        InvokeRepeating(nameof(Blink), 0f, blinkSpeed);
    }

    private void Blink()
    {
        if(count % 2 == 0)
            screenText.text = initialText;
        else
            screenText.text = initialText + "█";

        count++;
    }
}
