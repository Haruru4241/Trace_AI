using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    public float baseMoveSpeed = 5.0f; // 기본 이동 속도

    [SerializeField]
    protected float currentMoveSpeed; // 현재 이동 속도
    protected List<float> speedModifiers = new List<float> { };

    protected bool isMoving;
    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }
    public virtual void Initialize()
    {
        speedModifiers.Add(baseMoveSpeed);
        UpdateMoveSpeed();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying ) UpdateMoveSpeed();
    }

    private void UpdateMoveSpeed()
    {
        currentMoveSpeed = 1.0f;
        foreach (float modifier in speedModifiers)
        {
            currentMoveSpeed *= modifier;
        }
        UpdateSpeed(currentMoveSpeed);
    }

    public void AddSpeedModifier(float modifier)
    {
        speedModifiers.Add(modifier);
        UpdateMoveSpeed();
    }

    public void RemoveSpeedModifier(float modifier)
    {
        speedModifiers.Remove(modifier);
        UpdateMoveSpeed();
    }

    public void ResetSpeedModifiers()
    {
        speedModifiers.Clear();
        speedModifiers.Add(baseMoveSpeed);
        UpdateMoveSpeed();
    }

    public bool IsMoving()
    {
        return isMoving;
    }


    public abstract void UpdateSpeed(float value);
}
