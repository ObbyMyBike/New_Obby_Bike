using System;
using System.Collections.Generic;
using UnityEngine;

public class RacePath
{
    private readonly Vector3[] points;
    private readonly float[] cumLen;
    private readonly int count;
    private readonly float totalLen;

    public bool IsValid => points != null && points.Length >= 2;
    public float TotalLength => totalLen;
    
    public RacePath(CheckPoints[] pointsArray)
    {
        if (pointsArray == null || pointsArray.Length == 0)
        {
            points = null;
            cumLen = null;
            count = 0;
            totalLen = 0f;
            
            return;
        }

        Array.Sort(pointsArray, (checkPoints, checkPointsNext) => checkPoints.Number.CompareTo(checkPointsNext.Number));

        List<Vector3> list = new List<Vector3>(pointsArray.Length);
        
        for (int i = 0; i < pointsArray.Length; i++)
        {
            if (pointsArray[i] != null)
                list.Add(pointsArray[i].transform.position);
        }

        if (list.Count < 2)
        {
            points = null;
            cumLen = null;
            count = 0;
            totalLen = 0f;
            
            return;
        }

        points = list.ToArray();
        count = points.Length;

        cumLen = new float[count];
        float distance = 0f;
        
        for (int i = 1; i < count; i++)
        {
            distance += Vector3.Distance(points[i - 1], points[i]);
            cumLen[i] = distance;
        }
        
        totalLen = distance;
    }
    
    public float ComputeProgress(Vector3 pos)
    {
        if (!IsValid || totalLen <= 1e-4f)
            return 0f;

        float bestSqr = float.PositiveInfinity;
        float bestDistAlong = 0f;

        for (int i = 1; i < count; i++)
        {
            Vector3 pointA = points[i - 1];
            Vector3 pointB = points[i];
            Vector3 ab = pointB - pointA;
            float abLen2 = ab.sqrMagnitude;
            
            if (abLen2 < 1e-6f)
                continue;

            float clamp01 = Mathf.Clamp01(Vector3.Dot(pos - pointA, ab) / abLen2);
            Vector3 proj = pointA + ab * clamp01;

            float sqr = (pos - proj).sqrMagnitude;
            
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                float segLen = Mathf.Sqrt(abLen2);
                bestDistAlong = cumLen[i - 1] + segLen * clamp01;
            }
        }

        return Mathf.Clamp01(bestDistAlong / totalLen);
    }
}