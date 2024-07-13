using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public abstract class Detection : MonoBehaviour
{
    public LayerMask detectionLayerMask;


    public virtual List<Transform> Detect() { return null; }
}
