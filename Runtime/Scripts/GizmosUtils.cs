using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class GizmosUtils
    {
        public static void DrawBoxBetweenPoints(Vector2 firstPoint, Vector2 secondPoint, float height)
        {
            var boxAngle = Vector2.SignedAngle(Vector2.right, firstPoint - secondPoint);
            var matrix = UnityEngine.Gizmos.matrix;
            UnityEngine.Gizmos.matrix = Matrix4x4.TRS((firstPoint + secondPoint) * 0.5f, Quaternion.Euler(0, 0, boxAngle), Vector3.one);
            UnityEngine.Gizmos.DrawCube(Vector3.zero, new Vector3((firstPoint - secondPoint).magnitude, height, 0));
            UnityEngine.Gizmos.matrix = matrix;
        }

        public static void DrawArrowLine(Vector2 firstPoint, Vector2 secondPoint, float size = 0.2f, float padding = 0f)
        {
            var displacement = secondPoint - firstPoint;
            var direction = displacement.normalized;

            firstPoint += direction * padding;
            secondPoint -= direction * padding;

            Gizmos.DrawLine(firstPoint, secondPoint);
            var perpendicular = Vector2.Perpendicular(direction);

            var firstArrowPoint = secondPoint - direction * size + perpendicular * size;
            var secondArrowPoint = secondPoint - direction * size - perpendicular * size;

            Gizmos.DrawLine(secondPoint, firstArrowPoint);
            Gizmos.DrawLine(secondPoint, secondArrowPoint);
        }
    }
}