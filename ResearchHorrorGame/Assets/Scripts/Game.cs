using System;
using UnityEngine;

public class Game : MonoBehaviour
{
    public enum GameEventType
    {
        DEFAULT,
        GAME_START,
        GAME_PAUSE,
        GAME_RESUME,
        GAME_END
    }

    public static Game Instance { get; private set; }

    public static bool isStarted { get; private set; }

    /// <summary>
    /// The event that is called when the game starts. Only Game can direcly invoke it, events invocations can be requested.
    /// </summary>
    public static event Action OnGameStart;
    public static event Action OnGamePause;
    public static event Action OnGameResume;
    public static event Action OnGameEnd;


    private void Awake()
    {
        if(Instance)
        {
            gameObject.SetActive(false);
            return;
        }

        Instance = this;

        OnGameStart += () => { isStarted = true; };
        OnGameEnd += () => { isStarted = false; };
    }

    public static bool RequestGameEventInvoke(GameEventType type)
    {
        switch(type)
        {
            case GameEventType.DEFAULT:
            default:
                return false;

            case GameEventType.GAME_START:
                OnGameStart?.Invoke();
                return OnGameStart != null && OnGameStart.GetInvocationList().Length > 0;

            case GameEventType.GAME_PAUSE:
                OnGamePause?.Invoke();
                return OnGamePause != null && OnGamePause.GetInvocationList().Length > 0;

            case GameEventType.GAME_RESUME:
                OnGameResume?.Invoke();
                return OnGameResume != null && OnGameResume.GetInvocationList().Length > 0;

            case GameEventType.GAME_END:
                OnGameEnd?.Invoke();
                return OnGameEnd != null && OnGameEnd.GetInvocationList().Length > 0;
        }
    }
}
