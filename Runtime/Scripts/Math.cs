using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Utils
{
    public static class Math
    {
        [Serializable]
        public struct LineEquation
        {
            public float k;
            public float b;

            public LineEquation(float k = 0, float b = 0)
            {
                this.k = k;
                this.b = b;
            }

            public LineEquation(Vector2 linePoint1, Vector2 linePoint2)
            {
                if (linePoint1.x == linePoint2.x)
                {
                    k = float.NaN;
                    b = linePoint1.x;
                }
                else
                {
                    k = (linePoint1.y - linePoint2.y) / (linePoint1.x - linePoint2.x);
                    b = linePoint1.y - k * linePoint1.x;
                }
            }

            public LineEquation GetPerpendicular(Vector2 linePoint)
            {
                if (float.IsNaN(this.k))
                    return new LineEquation(0, linePoint.y);

                if (this.k == 0)
                    return new LineEquation(float.NaN, linePoint.x);

                var k = -1f / this.k;
                return new LineEquation(k, linePoint.y - k * linePoint.x);
            }
        }

        /// <summary>
        /// ax^2 + bx + c = 0
        /// </summary>
        [Serializable]
        public struct QuadraticEquation
        {
            public float a;
            public float b;
            public float c;

            /// <summary>
            /// ax^2 + bx + c = 0
            /// </summary>
            public QuadraticEquation(float a = 0, float b = 0, float c = 0)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            public float Solve(float x)
            {
                return a * x * x + b * x + c;
            }
        }

        public static float Descriminant(QuadraticEquation equation)
        {
            var a = equation.a;
            var b = equation.b;
            var c = equation.c;

            return b * b - 4 * a * c;
        }

        public static bool SolveQuadraticEquation(QuadraticEquation equation, out Vector2 res)
        {
            var a = equation.a;
            var b = equation.b;
            var c = equation.c;

            if (a != 0)
            {
                var d = Descriminant(equation);
                if (d < 0)
                {
                    res.x = 0;
                    res.y = res.x;
                    return false;
                }
                else if (d == 0)
                {
                    res.x = (-b) / (2 * a);
                    res.y = res.x;
                }
                else
                {
                    res.x = (-b - Mathf.Sqrt(d)) / (2 * a);
                    res.y = (-b + Mathf.Sqrt(d)) / (2 * a);
                }
            }
            else
            {
                if (b != 0)
                {
                    //bx + c = 0
                    //bx = -c
                    //x = -c/b
                    res.x = (-c) / (b);
                    res.y = res.x;
                }
                else
                {
                    res.x = 0;
                    res.y = res.x;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// if line equation is: [y = kx + b], returns [k] and [b] by passed (x1,y1) and (x2,y2). if line is vertical returns (NaN, x)
        /// </summary>
        public static LineEquation GetLineEquation(Vector2 linePoint1, Vector2 linePoint2)
        {
            if (linePoint1.x == linePoint2.x)
                return new LineEquation(float.NaN, linePoint1.x);

            var k = (linePoint1.y - linePoint2.y) / (linePoint1.x - linePoint2.x);
            return new LineEquation(k, linePoint1.y - k * linePoint1.x);
        }

        /// <summary>
        /// if line equation is: [y = kx + b]. if line is vertical returns (0, y), if horisontal returns (NaN,x)
        /// </summary>
        public static LineEquation GetPerpendicularLineEquation(Vector2 linePoint, LineEquation lineEquation)
        {
            if (float.IsNaN(lineEquation.k))
                return new LineEquation(0, linePoint.y);

            if (lineEquation.k == 0)
                return new LineEquation(float.NaN, linePoint.x);

            var k = -1f / lineEquation.k;
            return new LineEquation(k, linePoint.y - k * linePoint.x);
        }

        /// <summary>
        /// if line equation is: [y = kx + b]. if lines matches or parallel retrns (NaN,NaN)
        /// </summary>
        public static Vector2 GetLinesCrossPoint(LineEquation line1Equation, LineEquation line2Equation)
        {
            //если одна вертикальная а вторая нет, то считаем на прямую
            if (float.IsNaN(line1Equation.k) && !float.IsNaN(line2Equation.k))
            {
                return new Vector2(
                    line1Equation.b,
                    line2Equation.k * line1Equation.b + line2Equation.b
                );
            }
            else if (!float.IsNaN(line1Equation.k) && float.IsNaN(line2Equation.k))
            {
                return new Vector2(
                    line2Equation.b,
                    line1Equation.k * line2Equation.b + line1Equation.b
                );
            }
            else if (float.IsNaN(line1Equation.k) && float.IsNaN(line2Equation.k))
            {
                return new Vector2(float.NaN, float.NaN);
            }
            else
            {
                var cross = new Vector2();
                cross.x = (line2Equation.b - line1Equation.b) / (line1Equation.k - line2Equation.k);
                cross.y = line1Equation.k * cross.x + line1Equation.b;
                return cross;
            }
        }

        public static Vector2 GetClosestPoint(LineEquation lineEquation, Vector2 toPoint)
        {
            var perpendicular = GetPerpendicularLineEquation(toPoint, lineEquation);
            return GetLinesCrossPoint(lineEquation, perpendicular);
        }

        public static Vector2 GetLinesCrossPoint(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End)
        {
            var l1Eq = GetLineEquation(line1Start, line1End);
            var l2Eq = GetLineEquation(line2Start, line2End);
            return GetLinesCrossPoint(l1Eq, l2Eq);
        }

        public static bool PointInsideRect(Vector2 firstCorner, Vector2 secondCorner, Vector2 point)
        {
            //возможные варианты входных параметров
            //  f----     ----f    s----    ----s 
            //  | 1 |     | 2 |    | 3 |    | 4 |
            //  ----s     s----    ----f    f----

            Rect rect;

            //1 variant
            if (firstCorner.x <= secondCorner.x && firstCorner.y >= secondCorner.y)
                rect = new Rect(
                    new Vector2(firstCorner.x, secondCorner.y),
                    new Vector2(secondCorner.x - firstCorner.x, firstCorner.y - secondCorner.y)
                );
            //2 variant
            else if (firstCorner.x > secondCorner.x && firstCorner.y >= secondCorner.y)
                rect = new Rect(
                   secondCorner,
                   firstCorner - secondCorner
               );
            // 3 variant
            else if (firstCorner.x > secondCorner.x && firstCorner.y < secondCorner.y)
                rect = new Rect(
                   new Vector2(secondCorner.x, firstCorner.y),
                   new Vector2(firstCorner.x - secondCorner.x, secondCorner.y - firstCorner.y)
               );
            // 4 variant
            else
                rect = new Rect(
                    firstCorner,
                    secondCorner - firstCorner
                );

            return rect.Contains(point);
        }

        public static bool IsSegmentIntersectsPoint(Vector2 segmentStart, Vector2 segmentEnd, Vector2 point, float radius = 0)
        {
            //получить уравнеие прямой
            var lineEquation = GetLineEquation(segmentStart, segmentEnd);
            //получить уравнение перпендикулярной прямой
            var perpendicularLineEquation = GetPerpendicularLineEquation(point, lineEquation);
            //найти точку пересечения прямых
            var crossPoint = GetLinesCrossPoint(lineEquation, perpendicularLineEquation);//совпадать прямые не могут!
                                                                                         //если пересеччени нет, то это фалс
            if (float.IsNaN(crossPoint.x) || float.IsNaN(crossPoint.y))
                return false;
            //если точки совпадают, то эта тру
            if (point.Equals(crossPoint))
                return true;
            //определить находится ли пересечение в отрезке
            if (!PointInsideRect(segmentStart, segmentEnd, point))
                return false;
            //найти расстояние от точки пересечения до point
            var sqrRadius = radius * radius;
            var sqrDist = Vector2.SqrMagnitude(crossPoint - point);
            //если оно не больше radius, то норм
            return sqrDist <= sqrRadius;
        }

        /// <summary>
        /// under or left
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <param name="trueResultIfLineIncludesPoint"></param>
        /// <returns></returns>
        public static bool PointUnderOrLeftLine(Vector2 point, LineEquation line, bool trueResultIfLineIncludesPoint = true)
        {
            if (float.IsNaN(line.k))
            {
                return trueResultIfLineIncludesPoint ? point.x <= line.b : point.x < line.b;
            }
            else
            {
                var lineY = line.k * point.x + line.b;
                return trueResultIfLineIncludesPoint ? point.y <= lineY : point.y < lineY;
            }
        }

        public static Vector2 GetBezierPointQuadratic(Vector2 baseStart, Vector2 baseMiddle, Vector2 baseEnd, float t)
        {
            var a = Vector2.Lerp(baseStart, baseMiddle, t);
            var b = Vector2.Lerp(baseMiddle, baseEnd, t);
            var c = Vector2.Lerp(a, b, t);
            return c;
        }

        public static Vector2 GetBezierPointCubic(Vector2 baseA, Vector2 baseB, Vector2 baseC, Vector2 baseD, float t)
        {
            var a = Vector2.Lerp(baseA, baseB, t);
            var b = Vector2.Lerp(baseB, baseC, t);
            var c = Vector2.Lerp(baseC, baseD, t);

            var d = Vector2.Lerp(a, b, t);
            var e = Vector2.Lerp(b, c, t);

            var f = Vector2.Lerp(d, e, t);
            return f;
        }

        public static float Remap(float inMin, float inMax, float outMin, float outMax, float value)
        {
            var t = Mathf.InverseLerp(inMin, inMax, value);
            var v = Mathf.Lerp(outMin, outMax, t);
            return v;
        }

        /// <summary>
        /// is two 1D segments intersects - return overlay width, else retuen - 0;
        /// </summary>
        /// <param name="firstSegment">x - OX start, y - OX end</param>
        /// <param name="secondSegment">x - OX start, y - OX end</param>
        /// <returns>overlay width or 0; always >= 0</returns>
        public static float GetIntersectsWidth1D(Vector2 firstSegment, Vector2 secondSegment)
        {
            if (!IsIntersects1D(firstSegment, secondSegment))
                return 0;

            firstSegment = Sort1D(firstSegment);
            secondSegment = Sort1D(secondSegment);

            //swap
            if (firstSegment.x > secondSegment.x)
            {
                var t = firstSegment;
                firstSegment = secondSegment;
                secondSegment = firstSegment;
            }

            var points = new List<float>(4) { firstSegment.x, firstSegment.y, secondSegment.x, secondSegment.y };
            points.Sort();
            return points[2] - points[1];
        }

        /// <summary>
        /// is two 1D segments intersects
        /// </summary>
        /// <param name="firstSegment">x - OX start, y - OX end</param>
        /// <param name="secondSegment">x - OX start, y - OX end</param>
        /// <returns></returns>
        public static bool IsIntersects1D(Vector2 firstSegment, Vector2 secondSegment)
        {
            firstSegment = Sort1D(firstSegment);
            secondSegment = Sort1D(secondSegment);

            var rigthSeg = firstSegment;
            var leftSeg = secondSegment;
            if (firstSegment.x < secondSegment.x)
            {
                rigthSeg = secondSegment;
                leftSeg = firstSegment;
            }

            return (rigthSeg.y >= leftSeg.x && leftSeg.y >= rigthSeg.x);
        }

        private static Vector2 Sort1D(Vector2 segment)
        {
            //swap
            if (segment.x > segment.y)
            {
                var t = segment.x;
                segment.x = segment.y;
                segment.y = t;
            }
            return segment;
        }

        /// <summary>
        /// returns integer value -1 or 1
        /// </summary>
        public static float RandomSign => UnityEngine.Mathf.Sign(UnityEngine.Random.Range(1, 3) * 2 - 3);

        public static bool TryGetIntersection(Rect firstRect, Rect secondRect, bool includeZeroSize, out Rect intersection)
        {
            intersection = new Rect()
            {
                xMin = Mathf.Max(firstRect.xMin, secondRect.xMin),
                yMin = Mathf.Max(firstRect.yMin, secondRect.yMin),
                xMax = Mathf.Min(firstRect.xMax, secondRect.xMax),
                yMax = Mathf.Min(firstRect.yMax, secondRect.yMax)
            };

            if (includeZeroSize)
                return intersection.size.x >= 0 && intersection.size.y >= 0;
            else
                return intersection.size.x > 0 && intersection.size.y > 0;
        }

        public static bool IsInside(Rect outterRect, Rect innerRect)
        {
            if (!TryGetIntersection(outterRect, innerRect, includeZeroSize: false, out var intersection))
                return false;
            return intersection == innerRect;
        }
    }
}