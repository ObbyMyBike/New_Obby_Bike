using System;
using System.Collections.Generic;
using UnityEngine;

public class RacePath
{
    private readonly Vector3[] points;
    private readonly int count;
    private readonly float[] cumLen;
    private readonly float totalLen;
    private readonly bool uniformSegments;
    
    public Vector3 FinishPoint { get; private set; }
    public bool IsValid => points != null && points.Length >= 2;
    
    public RacePath(CheckPoints[] pointsArray)
    {
        uniformSegments = false;

        if (pointsArray == null || pointsArray.Length == 0)
        {
            points = null; cumLen = null; count = 0; totalLen = 0f;
            
            return;
        }
        
        Array.Sort(pointsArray, (a, b) => a.Number.CompareTo(b.Number));
        List<Vector3> list = new List<Vector3>(pointsArray.Length);
        
        foreach (CheckPoints checkpoint in pointsArray)
        {
            if (checkpoint != null)
                list.Add(checkpoint.transform.position);
        }

        if (list.Count < 2)
        {
            points = null; cumLen = null; count = 0; totalLen = 0f;
            
            return;
        }

        points = list.ToArray();
        count = points.Length;
        FinishPoint = points[count - 1];
        cumLen = new float[count];
        
        float distance = 0f;

        for (int i = 1; i < count; i++)
        {
            distance += Vector3.Distance(points[i - 1], points[i]);
            cumLen[i] = distance;
        }

        totalLen = distance;
    }
    
    public RacePath(Vector3[] worldPoints, bool uniformSegments)
    {
        this.uniformSegments = uniformSegments;

        if (worldPoints == null || worldPoints.Length < 2)
        {
            points = null; cumLen = null; count = 0; totalLen = 0f;
            
            return;
        }

        points = worldPoints;
        count = points.Length;
        FinishPoint = points[count - 1];

        cumLen = new float[count];
        float distance = 0f;

        for (int i = 1; i < count; i++)
        {
            distance += uniformSegments ? 1f : Vector3.Distance(points[i - 1], points[i]);
            cumLen[i] = distance;
        }

        totalLen = distance;
    }

    public float ComputeProgress(Vector3 position)
    {
        if (!IsValid || totalLen <= 1e-4f)
            return 0f;

        float bestSqr = float.PositiveInfinity;
        float bestDistanceAlong = 0f;

        for (int i = 1; i < count; i++)
        {
            Vector3 pointA = points[i - 1];
            Vector3 pointB = points[i];
            Vector3 distance = pointB - pointA;
            float abLen2 = distance.sqrMagnitude;
            
            if (abLen2 < 1e-6f)
                continue;

            float time = Mathf.Clamp01(Vector3.Dot(position - pointA, distance) / abLen2);
            Vector3 proj = pointA + distance * time;
            float sqr = (position - proj).sqrMagnitude;

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                
                if (uniformSegments)
                {
                    bestDistanceAlong = cumLen[i - 1] + 1f * time;
                }
                else
                {
                    float segLen = Mathf.Sqrt(abLen2);
                    
                    bestDistanceAlong = cumLen[i - 1] + segLen * time;
                }
            }
        }

        return Mathf.Clamp01(bestDistanceAlong / totalLen);
    }
}