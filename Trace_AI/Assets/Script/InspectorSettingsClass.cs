using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DetectionWeight
{
    public string detectionType;
    public float value;
}

[Serializable]
public class LayerValue
{
    public string layerName;
    public float value;
}

[System.Serializable]
public class LayerPenalty
{
    public LayerMask layer;
    public int penalty;
}