using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public abstract class Detection : MonoBehaviour
{
    [Tooltip("감지할 오브젝트 레이어 설정")]
    public LayerMask detectionLayerMask;

    public virtual List<Transform> Detect() { return null; }
}
