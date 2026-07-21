using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class WayPointLine
{
    public List<Transform> Points = new();
    public Color GizmoColor;
}

public class WayPointManager : MonoBehaviour
{
    public static WayPointManager Instance;
    public List<WayPointLine> wayPoints; //3종류의 길이 존재할 것임.

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (wayPoints == null)
            return;;

        for (int line = 0; line < wayPoints.Count; line++) {
            Gizmos.color = wayPoints[line].GizmoColor;

            for (int i = 0; i < wayPoints[line].Points.Count; i++) {
                Transform point = wayPoints[line].Points[i];

                if (point == null)
                    continue;

                Gizmos.DrawSphere(point.position, 0.1f);

                if (i < wayPoints[line].Points.Count - 1) {
                    Transform next = wayPoints[line].Points[i + 1];

                    if (next != null)
                        Gizmos.DrawLine(point.position, next.position);
                }
            }
        }
    }
}