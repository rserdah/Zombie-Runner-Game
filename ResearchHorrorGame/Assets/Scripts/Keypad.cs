using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// A Keypad is a general class that is an ITriggerable (including use for keyboards and doors with code locks) that sends a string to an Executable (if it is a keyboard it
/// could send something like "Hello, World!" and if it is a keypad it could send something like "1234" or "1-2-3-4" or even "TRIANGLE-SQUARE-CIRCLE" if it is a puzzle)
/// </summary>
public class Keypad : MonoBehaviour, ITriggerable, IExecutable
{
    public enum KeypadType
    {
        KEYBOARD, //Keyboard for typing
        KEYPAD //, //Passcode door entry, etc.
        //SYMBOLPAD //Visual pattern entry (think cryptic symbol puzzle, memory puzzle, color pattern, etc.)
    }

    public KeypadType keypadType;

    private class PadState
    {
        public event Action OnStateChange;

        protected void onStateChange() => OnStateChange?.Invoke();
    }

    private class KeyboardState : PadState
    {
        private bool shift;
        private bool capsLock;

        public string entry { get; private set; }


        public void Shift()
        {
            shift = true;

            onStateChange();
        }

        public void ToggleCapsLock()
        {
            capsLock = !capsLock;

            onStateChange();
        }

        /// <summary>
        /// Returns what would be appended to entry if the given char was sent with SendChar. Used for knowing whether the next character will be uppercase or lowercase, etc.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public char PeekInput(char c)
        {
            if(shift || capsLock)
                return char.ToUpper(c);
            else
                return c;
        }

        public void SendChar(char c)
        {
            if(shift || capsLock)
                entry += char.ToUpper(c);
            else
                entry += c;

            Debug.LogError(entry);

            shift = false;

            onStateChange();
        }

        public void Backspace()
        {
            if(entry.Length > 0)
            {
                entry = entry.Substring(0, entry.Length - 1);

                onStateChange();
            }
        }
    }

    private struct ActionStringPair
    {
        public UnityAction<ITriggerable> action;
        public string str;
    }

    //Keyboard
    private static readonly KeyCode[] keyboardKeyCodes = 
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0,
        KeyCode.Minus, KeyCode.Equals, KeyCode.Backspace, 
        KeyCode.Tab, KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, KeyCode.LeftBracket, KeyCode.RightBracket, 
        KeyCode.CapsLock, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon, KeyCode.Quote, KeyCode.Return, 
        KeyCode.LeftShift, KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M, KeyCode.Comma, KeyCode.Period, KeyCode.Slash, KeyCode.RightShift,
        KeyCode.LeftControl, KeyCode.Space, KeyCode.RightControl
    };
    private static Dictionary<string, ActionStringPair> actionsAndChars = new Dictionary<string, ActionStringPair>();

    //Keypad
    private static readonly KeyCode[] keypadKeyCodes =
    {
        KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4, KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9,
        KeyCode.KeypadPeriod, KeyCode.KeypadDivide, KeyCode.KeypadMultiply, KeyCode.KeypadMinus, KeyCode.KeypadPlus, KeyCode.KeypadEnter, KeyCode.KeypadEquals
    };

    //Symbol pad
    //TODO: Implement things for symbol pad. Can define symbols for use and make enum for it possibly

    //All
    private Dictionary<KeyCode, ISelectable> keys;

    public List<IExecutable> listeners { get => GetListenersAsList(); set { } }
    public MonoBehaviour[] m_listeners;

    /// <summary>
    /// The action that this Keypad triggers when the player presses enter/submit
    /// </summary>
    public UnityAction<ITriggerable> TriggerAction { get; set; }

    /// <summary>
    /// The action that each key triggers when pressed (this will usually be just sending the char/number/symbol from the pressed key to this Keypad)
    /// </summary>
    public UnityAction<ITriggerable> ExecuteAction { get; set; }

    private PadState padState;
    public UnityEngine.UI.Text text;


    private void Start()
    {
        BindListeners();

        Debug.LogError(keyboardKeyCodes.Length);

        padState.OnStateChange += () => { text.text = ((KeyboardState)padState).entry; };
    }

    private List<IExecutable> GetListenersAsList()
    {
        List<IExecutable> listeners = new List<IExecutable>();

        IExecutable e;
        foreach(MonoBehaviour m in m_listeners)
        {
            e = (IExecutable)m;
            if(e != null)
                listeners.Add(e);
        }

        return listeners;
    }

    public void BindListeners()
    {
        switch(keypadType)
        {
            default:
            case KeypadType.KEYBOARD:
                InitKeyboardKeys();
                break;

            case KeypadType.KEYPAD:

                break;
        }
    }

    /// <summary>
    /// Binds the keyboard keys to this Keypad AND also sets the OnSelect action of each key
    /// </summary>
    private void InitKeyboardKeys()
    {
        padState = new KeyboardState();

        Transform[] children = GetComponentsInChildren<Transform>();
        foreach(Transform t in children)
        {
            if(t.name.Contains("Char."))
            {
                Button b = t.gameObject.AddComponent<Button>();

                t.gameObject.AddComponent<BoxCollider>();
                //Debug.LogError(t.name + "->" + GetKeyCodeNameFromKey(b));
                InitKey(b);

                b.OnSelect += () =>
                {
                    ActionStringPair val;

                    HUD.pointer = HUD.PointerType.STRING;
                    actionsAndChars.TryGetValue(t.name, out val);

                    HUD.pointerTextTip = val.str.Length > 1 ? val.str : ((KeyboardState)padState).PeekInput(val.str[0]).ToString();
                };
            }
        }
    }

    public void Invoke()
    {
    }

    private void KeyboardInput(char input)
    {
    }

    private KeyCode GetKeyCodeNameFromKey(Button key)
    {
        string keyStr = key.transform.name.Remove(0, "Char.".Length);

        return (KeyCode)System.Enum.Parse(typeof(KeyCode), keyStr);
    }

    /// <summary>
    /// Does multiple things at the same time (to avoid having to copy/paste the huge switch statement):
    /// 1. Adds the keyboard key GameObject name as the key in the actionsAndChars Dictionary
    /// 2. Sets 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private ActionStringPair InitKey(Button key)
    {
        KeyboardState keyboardState = (KeyboardState)padState;
        KeyCode keyCode = GetKeyCodeNameFromKey(key);
        ActionStringPair actionStringPair = new ActionStringPair();
        string str;

        switch(keyCode)
        {
            //Letters
            case KeyCode.Q:
            case KeyCode.W:
            case KeyCode.E:
            case KeyCode.R:
            case KeyCode.T:
            case KeyCode.Y:
            case KeyCode.U:
            case KeyCode.I:
            case KeyCode.O:
            case KeyCode.P:
            case KeyCode.A:
            case KeyCode.S:
            case KeyCode.D:
            case KeyCode.F:
            case KeyCode.G:
            case KeyCode.H:
            case KeyCode.J:
            case KeyCode.K:
            case KeyCode.L:
            case KeyCode.Z:
            case KeyCode.X:
            case KeyCode.C:
            case KeyCode.V:
            case KeyCode.B:
            case KeyCode.N:
            case KeyCode.M:
                str = GetLetterCharFromKeyCode(keyCode).ToString();
                actionStringPair.action = (ITriggerable _) => { ((KeyboardState)padState).SendChar(str[0]); };
                actionStringPair.str = str;
                break;

            //Numbers
            case KeyCode.Alpha1:
            case KeyCode.Alpha2:
            case KeyCode.Alpha3:
            case KeyCode.Alpha4:
            case KeyCode.Alpha5:
            case KeyCode.Alpha6:
            case KeyCode.Alpha7:
            case KeyCode.Alpha8:
            case KeyCode.Alpha9:
            case KeyCode.Alpha0:
                str = GetNumberCharFromKeyCode(keyCode).ToString();
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar(str[0]); };
                actionStringPair.str = str;
                break;

            //Whitespace
            case KeyCode.Tab:
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar('\t'); };
                actionStringPair.str = "Tab";
                break;
            case KeyCode.Space:
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar(' '); };
                actionStringPair.str = "Space";
                break;

            //Special characters
            case KeyCode.LeftBracket:
                str = "[";
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar(str[0]); };
                actionStringPair.str = str;
                break;
            case KeyCode.RightBracket:
                str = "]";
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar(str[0]); };
                actionStringPair.str = str;
                break;
            case KeyCode.Semicolon:
                str = ";";
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar(str[0]); };
                actionStringPair.str = str;
                break;
            case KeyCode.Quote:
                str = "'";
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar(str[0]); };
                actionStringPair.str = str;
                break;
            case KeyCode.Comma:
                str = ",";
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar(str[0]); };
                actionStringPair.str = str;
                break;
            case KeyCode.Period:
                str = ".";
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar(str[0]); };
                actionStringPair.str = str;
                break;
            case KeyCode.Slash:
                str = "/";
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar(str[0]); };
                actionStringPair.str = str;
                break;
            case KeyCode.Minus:
                str = "-";
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar(str[0]); };
                actionStringPair.str = str;
                break;
            case KeyCode.Equals:
                str = "=";
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar(str[0]); };
                actionStringPair.str = str;
                break;

            //Modifiers
            case KeyCode.LeftShift:
            case KeyCode.RightShift:
                actionStringPair.action = (ITriggerable _) => { keyboardState.Shift(); };
                actionStringPair.str = "Shift";
                break;

            case KeyCode.CapsLock:
                actionStringPair.action = (ITriggerable _) => { keyboardState.ToggleCapsLock(); };
                actionStringPair.str = "Caps";
                break;

            case KeyCode.LeftControl:
            case KeyCode.RightControl:
                actionStringPair.action = (ITriggerable _) => {  };
                actionStringPair.str = "Ctrl";
                break;

            //Backspace
            case KeyCode.Backspace:
                actionStringPair.action = (ITriggerable _) => { keyboardState.Backspace(); };
                actionStringPair.str = "Del";
                break;

            //Enter
            case KeyCode.Return:
                actionStringPair.action = (ITriggerable _) => { };
                actionStringPair.str = "Ent";
                break;

            default:
                actionStringPair.action = (ITriggerable _) => { keyboardState.SendChar('\0'); };
                actionStringPair.str = "<null>";
                break;
        }

        key.TriggerAction += actionStringPair.action;
        actionsAndChars.Add(key.transform.name, actionStringPair);
        return actionStringPair;
    }

    private char GetLetterCharFromKeyCode(KeyCode keyCode) => Enum.GetName(typeof(KeyCode), keyCode).ToLower()[0];

    private char GetNumberCharFromKeyCode(KeyCode keyCode) => Enum.GetName(typeof(KeyCode), keyCode)[5]; //Gets the char after "Alpha"
}
