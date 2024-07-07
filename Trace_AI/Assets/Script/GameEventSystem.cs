using System;
using UnityEngine;

public class GameEventArgs : EventArgs
{
    public Transform Source { get; } // ��ü ������ ����

    public GameEventArgs(Transform source)
    {
        Source = source;
    }
}

public static class GameEventSystem
{
    public static event EventHandler<GameEventArgs> OnGameEvent;

    public static void RaiseEvent(Transform source)
    {
        OnGameEvent?.Invoke(null, new GameEventArgs(source));
    }
}
