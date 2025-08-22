using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Saved
{
    public Transform ObjectTransform;
    public Transform Parent;
    public Rigidbody ObjectRigidbody;
    public Vector3 LocalPosition;
    public Quaternion LocalRotation;
    public Vector3 LocalScale;
    
    public List<Collider> AddedColliders = new List<Collider>();
    public List<DisabledColliderState> DisabledColliders = new List<DisabledColliderState>();
    
    public bool HadRigidbody;
}