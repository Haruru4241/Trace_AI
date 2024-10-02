using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public abstract class Detection : MonoBehaviour
{
    [Tooltip("������ ������Ʈ ���̾� ����")]
    public LayerMask detectionLayerMask;
    public float Range = 5;

    public virtual List<Transform> Detect() { return null; }
}
