using System;
using UnityEngine;

public class GameEventArgs : EventArgs
{
    public Transform Source { get; } // 객체 정보를 전달

    public GameEventArgs(Transform source)
    {
        Source = source;
    }
}

public static class GameEventSystem
{
    public static event EventHandler<GameEventArgs> OnSoundDetected;
    public static event Action OnAiAdditionalEvent;

    public static void RaiseSoundDetected(Transform source)
    {
        OnSoundDetected?.Invoke(null, new GameEventArgs(source));
    }

    public static void RaiseAiAdditionalEvent()
    {
        OnAiAdditionalEvent?.Invoke();
    }
}
