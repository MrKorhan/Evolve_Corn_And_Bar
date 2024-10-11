using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CornStruct
{
    [HideInInspector] public string LayerStr;
    public int CornPartCounter;
    public int MaxCornPart;
    public List<Transform> FloorsList;
    public List<CornPartStruct> CornPartStructList;
}

[Serializable]
public class CornPartStruct
{
    [HideInInspector] public string PartStr;
    public Transform Parent;
    public Transform Transform;
    public Rigidbody Rb;
    public Renderer Renderer;
    public Vector3 LocalPosition;
    public Vector3 Rotation;
}
