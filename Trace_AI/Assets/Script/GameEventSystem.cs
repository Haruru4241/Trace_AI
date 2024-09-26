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
    public static event EventHandler<GameEventArgs> OnSoundDetected;
    public static event EventHandler<AggroSource> OnAggroDetected;
    public static event EventHandler<GameEventArgs> OnTargetDestroyed;
    public static event Action OnAiAdditionalEvent;

    public static void RaiseSoundDetected(Transform source)
    {
        OnSoundDetected?.Invoke(null, new GameEventArgs(source));
    }

    public static void RaiseAggroDetected(Transform source)
    {
        OnSoundDetected?.Invoke(null, new GameEventArgs(source));
    }

    public static void RaiseTargetDestroyed(Transform source) // �ı� �̺�Ʈ �߻� �޼��� �߰�
    {
        OnTargetDestroyed?.Invoke(null, new GameEventArgs(source));
    }

    public static void RaiseAiAdditionalEvent()
    {
        OnAiAdditionalEvent?.Invoke();
    }
}
