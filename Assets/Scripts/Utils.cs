using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    public static float DistanceFromPoint2Line(Vector3 p, Vector3 p1, Vector3 p2)
    {
        float p2pDistance = Vector3.Distance(p2, p);
        Vector3 p2p1 = p2 - p1;
        Vector3 p2p = p2 - p;
        float dotResult = Vector3.Dot(p2p1, p2p);
        float seitaRad = Mathf.Acos(dotResult / (p2p1.magnitude * p2pDistance));
        float distance = p2pDistance * Mathf.Sin(seitaRad);
        return distance;
    }
    public static float CoTangentFormPoint2Line(Vector3 p,Vector3 p1,Vector3 p2)
    {
        float angle = Vector3.Angle(p1 - p, p2 - p);
        return 1 / Mathf.Tan(angle * Mathf.Deg2Rad);
    }
}
