using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    public float baseMoveSpeed = 5.0f; // �⺻ �̵� �ӵ�
    public float currentMoveSpeed; // ���� �̵� �ӵ�
    protected List<float> speedModifiers;

    protected bool isMoving;
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        speedModifiers = new List<float> { baseMoveSpeed };
        UpdateMoveSpeed();
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
