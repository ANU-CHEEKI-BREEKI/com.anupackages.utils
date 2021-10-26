using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class DebugUtils
    {
        public static void DrawCircle(Vector2 position, float radius, int halfDotCnt = 6)
        {
#if UNITY_EDITOR
            DrawCircle(position, radius, Color.white, 0, halfDotCnt);
#endif
        }

        public static void DrawCircle(Vector2 position, float radius, Color color, float duration = 0, int halfDotCnt = 6)
        {
#if UNITY_EDITOR
            var angle = Mathf.PI / halfDotCnt;
            var prevPos1 = Vector2.zero;
            var prevPos2 = Vector2.zero;
            for (int i = 0; i <= halfDotCnt; i++)
            {
                var cos = Mathf.Cos(angle * i);
                var sin = Mathf.Sin(angle * i);
                var displacement = new Vector2(cos, sin) * radius;
                var pos1 = position + displacement;
                var pos2 = position - displacement;

                if (i != 0)
                {
                    UnityEngine.Debug.DrawLine(prevPos1, pos1, color, duration);
                    UnityEngine.Debug.DrawLine(prevPos2, pos2, color, duration);
                }

                prevPos1 = pos1;
                prevPos2 = pos2;
            }
#endif
        }

        public static void DrawCrosshair(Vector2 position, float size, Color color, float duration = 0)
        {
#if UNITY_EDITOR
            var rightShift = Vector2.right / 2f * size;
            var upShift = Vector2.up / 2f * size;
            UnityEngine.Debug.DrawLine(position + upShift, position - upShift, color, duration);
            UnityEngine.Debug.DrawLine(position - rightShift, position + rightShift, color, duration);
#endif
        }


        public static void DrawRectangle(Vector2 position, Vector2 size)
        {
#if UNITY_EDITOR
            DrawRectangle(new Bounds(position, size));
#endif
        }

        public static void DrawRectangle(Vector2 position, Vector2 size, Color color, float duration = 0)
        {
#if UNITY_EDITOR
            DrawRectangle(new Bounds(position, size), color, duration);
#endif
        }

        public static void DrawRectangle(Bounds bounds)
        {
#if UNITY_EDITOR
            DrawRectangle(bounds, Color.white, 0);
#endif
        }

        public static void DrawRectangle(Bounds bounds, Color color, float duration = 0)
        {
#if UNITY_EDITOR
            var halfUp = Vector2.up / 2f;
            var halhRight = Vector2.right / 2f;
            var center = (Vector2)bounds.center;
            var lt = center + halfUp * bounds.size.y - halhRight * bounds.size.x;
            var lb = center - halfUp * bounds.size.y - halhRight * bounds.size.x;
            var rt = center + halfUp * bounds.size.y + halhRight * bounds.size.x;
            var rb = center - halfUp * bounds.size.y + halhRight * bounds.size.x;

            UnityEngine.Debug.DrawLine(lb, lt, color, duration);
            UnityEngine.Debug.DrawLine(lt, rt, color, duration);
            UnityEngine.Debug.DrawLine(rt, rb, color, duration);
            UnityEngine.Debug.DrawLine(rb, lb, color, duration);
#endif
        }

        public static void DrawRectangle(Rect worldRect)
                => DrawRectangle(new Bounds(worldRect.center, worldRect.size), Color.white);

        public static void DrawRectangle(Rect worldRect, Color color, float duration = 0)
                => DrawRectangle(new Bounds(worldRect.center, worldRect.size), color, duration);

        public static void DrawArrowLine(Vector2 firstPoint, Vector2 secondPoint, Color color, float size = 0.2f, float padding = 0f, float duration = 0f)
        {
#if UNITY_EDITOR
            var displacement = secondPoint - firstPoint;
            var direction = displacement.normalized;

            firstPoint += direction * padding;
            secondPoint -= direction * padding;

            UnityEngine.Debug.DrawLine(firstPoint, secondPoint, color, duration);
            var perpendicular = Vector2.Perpendicular(direction);

            var firstArrowPoint = secondPoint - direction * size + perpendicular * size;
            var secondArrowPoint = secondPoint - direction * size - perpendicular * size;

            UnityEngine.Debug.DrawLine(secondPoint, firstArrowPoint, color, duration);
            UnityEngine.Debug.DrawLine(secondPoint, secondArrowPoint, color, duration);
#endif
        }
    }
}