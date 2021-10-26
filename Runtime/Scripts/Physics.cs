using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Utils
{
    public static class Physics
    {

        /// <summary>
        /// Подсчитать перемещение
        /// </summary>
        /// <param name="v0">velocity</param>
        /// <param name="t">time</param>
        /// <param name="a">acceleration</param>
        /// <returns></returns>
        public static float GetDisplacement(float v0, float t, float a)
        {
            return v0 * t + (a * t * t) * 0.5f;
        }

        /// <summary>
        /// Подсчитать перемещение
        /// </summary>
        /// <param name="v0">velocity</param>
        /// <param name="t">time</param>
        /// <param name="a">acceleration</param>
        /// <returns></returns>
        public static Vector2 GetDisplacement(Vector2 v0, float t, Vector2 a)
        {
            return new Vector2
            (
                GetDisplacement(v0.x, t, a.x),
                GetDisplacement(v0.y, t, a.y)
            );
        }

        /// <summary>
        /// Подсчитать скорость с ускорением
        /// </summary>
        /// <param name="v0">velocity</param>
        /// <param name="t">time</param>
        /// <param name="a">acceleretion</param>
        /// <returns></returns>
        public static float GetVelocity(float v0, float t, float a)
        {
            return v0 + a * t;
        }

        public static Vector2 GetVelocity(Vector2 v0, float t, Vector2 a)
        {
            return new Vector2(
                GetVelocity(v0.x, t, a.x),
                GetVelocity(v0.y, t, a.y)
            );
        }


        /// <summary>
        /// За сколько времени можно переместить на s
        /// </summary>
        /// <param name="s">displacement (s >= 0)</param>
        /// <param name="v0">velocity (v0 >= 0)</param>
        /// <param name="a">acceleration</param>
        /// <returns></returns>
        public static float GetTime(float s, float v0, float a)
        {
            // s = v0*t + a*t^2/2
            // at^2 + 2*v0*t - 2*s = 0

            Vector2 roots;
            if (!Utils.Math.SolveQuadraticEquation(new Math.QuadraticEquation(a, 2 * v0, -2 * s), out roots))
            {
                return 0;
            }
            else
            {
                var res = Mathf.Min(roots.x, roots.y);
                if (res < 0)
                    res = res == roots.x ? roots.y : roots.x;
                if (res < 0)
                    res = 0;
                return res;
            }
        }

        /// <summary>
        /// Расчитать начальную скорость, чтобы переместить обьект с начала движения до самого конца.
        /// Перемещение будет расчитано с учетом горизонтального и вертикального движения. (как если кинуть камень или запустить стрелу)
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="targetPosition"></param>
        /// <param name="resistance">сопротивление движению (сопротивление воздухи или гравитация). например для полета камня в воздухе может быть равням (0f, 9.8f)</param>
        /// <returns></returns>
        public static bool TryGetVelocity(Vector2 startPosition, Vector2 targetPosition, float absoluteVelocity, Vector2 resistance, bool minimizeTime, out Vector2 velocity)
        {
            //изначально имеем систему:
            //    | delta.x = VxT + AxT^2/2     (1)
            //    < delta.y = VyT + AyT^2/2     (2)
            //    | V = Sqrt(Vx^2 + Vy^2)       (3)

            //сначала подсчитаем перемещение
            var delta = targetPosition - startPosition;

            //подсчитаем время, для выполнения перемещения...
            //из (2) получаем Vy и подставляем в (3)
            //получаем: |Vy| = Sqrt(V^2 - (delta.x/T - AxT/2)^2)
            //раскрывая модуль, отбросим отрицательное значение. т.к. нам надо положительное значение скорости Vy
            //подставляем Vy в (2) уравнение и получем уравнение с T^4...
            //считая что T^2 = N, находим N из квадратного уравнения:
            var a = -resistance.x * resistance.x - resistance.y * resistance.y;
            var b = 4 * (absoluteVelocity * absoluteVelocity + delta.x * resistance.x + delta.y * resistance.y);
            var c = 4 * (-delta.x * delta.x - delta.y * delta.y);
            Vector2 n2;
            var hasResult = Utils.Math.SolveQuadraticEquation(new Math.QuadraticEquation(a, b, c), out n2);

            //так как n - это T^2, то n > 0. Более того, нас интересует минимильное вермя.
            if (!hasResult || (n2.x <= 0 && n2.y <= 0))
            {
                //хз что делать... пускай под 45 грудусов будет...
                velocity = new Vector2(absoluteVelocity * Mathf.Sign(delta.x), absoluteVelocity) / Mathf.Sqrt(2);
                return false;
            }

            var n = 0f;
            if (minimizeTime)
            {
                n = Mathf.Min(n2.x, n2.y);
                if (n <= 0) n = Mathf.Max(n2.x, n2.y);
            }
            else
            {
                n = Mathf.Max(n2.x, n2.y);
            }

            //|T| = Sqrt(n). Отрицательное значение отбрасываем. Т.к. нам надо положительное время...
            var time = Mathf.Sqrt(n);

            velocity = GetVelocity(startPosition, targetPosition, time, resistance);
            return true;
        }

        /// <summary>
        /// Расчитать начальную скорость, чтобы переместить обьект с начала движения до самого конца за указанное время.
        /// Перемещение будет расчитано с учетом горизонтального и вертикального движения. (как если кинуть камень или запустить стрелу)
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="targetPosition"></param>
        /// <param name="time"></param>
        /// <param name="resistance"></param>
        /// <returns></returns>
        public static Vector2 GetVelocity(Vector2 startPosition, Vector2 targetPosition, float time, Vector2 resistance)
        {
            //изначально имеем систему:
            //    | delta.x = VxT + AxT^2/2     (1)
            //    < 
            //    | delta.y = VyT + AyT^2/2     (2)

            //сначала подсчитаем перемещение
            var delta = targetPosition - startPosition;

            //ну и подставляем Т в формулы (1) и (2)
            var velocity = Vector2.zero;
            velocity.x = delta.x / time - resistance.x * time / 2;
            velocity.y = delta.y / time - resistance.y * time / 2;

            return velocity;
        }

        public static Vector2 CalculateLeadTargetPoint(Vector2 shooter, float bulletSpeed, Vector2 target, Vector2 targetVelocity)
        {
            var p1x = shooter.x; float p1y = shooter.y;
            var p2x = target.y;
            var p2y = target.y;
            var v2x = targetVelocity.x;
            var v2y = targetVelocity.y;
            var a = (v2x * v2x + v2y * v2y - bulletSpeed * bulletSpeed);
            var b = (2 * p2x * v2x - 2 * p1x * v2x + 2 * p2y * v2y - 2 * p1y * v2y);
            var c = (p2x * p2x - 2 * p2x * p1x + p1x * p1x + p2y * p2y - 2 * p2y * p1y + p1y * p1y);
            var result = (-b - Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
            return target + (result * targetVelocity);
        }
    }
}