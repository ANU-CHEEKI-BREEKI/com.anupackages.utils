using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ANU.Utils.Physics
{
    public static class PhysicUtil
    {
        public struct ExtremumHitResult
        {
            public RaycastHit2D extremumHit;
            public float extremumDistance;
            public float extremumDistanceSqr;
            public int hitsCount;
        }

        /// <summary>
        /// May includes call of GetHitsAround!
        /// </summary>
        /// <param name="center">Raycast origin</param>
        /// <param name="from">Direction of first cast</param>
        /// <param name="angle">Cast sector angle in degrees</param>
        /// <param name="radius">Rays length</param>        
        /// <param name="hits">Array of all raycast hit results. even if it not hits. Also array length defines count of ray casts.</param>
        /// <param name="layers"></param>
        /// <param name="usePassedHits">if false this call will includes call of methot GetHitsAround</param>
        /// <returns></returns>
        public static ExtremumHitResult GetClosestHit(Vector2 center, Vector2 from, float angle, float radius, LayerMask layers, RaycastHit2D[] hits, bool usePassedHits = false)
        {
            ExtremumHitResult result = new ExtremumHitResult();

            if (!usePassedHits)
                GetHitsAround(center, from, angle, radius, layers, hits);

            RaycastHit2D? closestHit = null;
            result.hitsCount = 0;
            result.extremumDistanceSqr = 0;
            foreach (var hit in hits)
            {
                if (hit)
                {
                    result.hitsCount++;
                    var dist = (hit.point - center).sqrMagnitude;
                    if (!closestHit.HasValue || dist < result.extremumDistanceSqr)
                    {
                        closestHit = hit;
                        result.extremumDistanceSqr = dist;
                    }
                }
            }

            if (closestHit.HasValue)
                result.extremumHit = closestHit.Value;

            return result;
        }

        /// <summary>
        /// May includes call of GetHitsAround!
        /// </summary>
        /// <param name="center">Raycast origin</param>
        /// <param name="from">Direction of first cast</param>
        /// <param name="angle">Cast sector angle in degrees</param>
        /// <param name="radius">Rays length</param>        
        /// <param name="hits">Array of all raycast hit results. even if it not hits. Also array length defines count of ray casts.</param>
        /// <param name="layers"></param>
        /// <param name="usePassedHits">if false this call will includes call of methot GetHitsAround</param>
        /// <returns></returns>
        public static ExtremumHitResult GetFurtherHit(Vector2 center, Vector2 from, float angle, float radius, LayerMask layers, RaycastHit2D[] hits, bool usePassedHits = false)
        {
            ExtremumHitResult result = new ExtremumHitResult();

            if (!usePassedHits)
                GetHitsAround(center, from, angle, radius, layers, hits);

            RaycastHit2D? furtherHit = null;
            result.hitsCount = 0;
            result.extremumDistanceSqr = 0;
            foreach (var hit in hits)
            {
                if (hit)
                {
                    result.hitsCount++;
                    var dist = (hit.point - center).sqrMagnitude;
                    if (!furtherHit.HasValue || dist > result.extremumDistanceSqr)
                    {
                        furtherHit = hit;
                        result.extremumDistanceSqr = dist;
                    }
                }
            }

            if (furtherHit.HasValue)
                result.extremumHit = furtherHit.Value;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center">Raycast origin</param>
        /// <param name="from">Direction of first cast</param>
        /// <param name="angle">Cast sector angle in degrees</param>
        /// <param name="radius">Rays length</param>        
        /// <param name="hits">Array of all raycast hit results. even if it not hits. Also array length defines count of ray casts.</param>
        public static void GetHitsAround(Vector2 center, Vector2 from, float angle, float radius, LayerMask layers, RaycastHit2D[] hits, bool debugLines = false)
        {
            var raysCount = hits.Length;
            var sectorAngle = angle / raysCount;

            for (int i = 0; i < raysCount; i++)
            {
                var direction = (Vector2)(Quaternion.Euler(0, 0, sectorAngle * i) * from);
                hits[i] = Physics2D.Raycast(center, direction, radius, layers);

                if (debugLines)
                    Debug.DrawLine(center, center + direction.normalized * radius, Color.magenta, 1f);
            }
        }

        private struct Physic2DQueries
        {
            public bool queriesHitTriggers;
            public bool queriesStartInColliders;
            public int scopeId;
        }

        private static Stack<Physic2DQueries> _physics2DQueries = new Stack<Physic2DQueries>();
        private static HashSet<int> _scopes = new HashSet<int>();
        private static int _lastScopeId = -1;
        private const int NONE_SCOPE_ID = -1;

        [RuntimeInitializeOnLoadMethod]
        private static void ClearQueriesStack()
        {
            _physics2DQueries.Clear();
            _scopes.Clear();
        }

        /// <summary>
        /// push current queries in stack
        /// </summary>
        public static void PushQueries() => PhysicUtil.PushQueries(NONE_SCOPE_ID);

        /// <summary>
        /// push current queries in stack and set new values as current
        /// </summary>
        /// <param name="queriesHitTriggers"></param>
        /// <param name="queriesStartInColliders"></param>
        public static void PushQueries(bool queriesHitTriggers, bool queriesStartInColliders) => PushQueries(queriesHitTriggers, queriesStartInColliders, NONE_SCOPE_ID);

        private static void PushQueries(int scopeId)
        {
            _physics2DQueries.Push(new Physic2DQueries()
            {
                queriesHitTriggers = Physics2D.queriesHitTriggers,
                queriesStartInColliders = Physics2D.queriesStartInColliders,
                scopeId = scopeId
            });
            _scopes.Add(scopeId);
        }

        private static void PushQueries(bool queriesHitTriggers, bool queriesStartInColliders, int scopeId)
        {
            PushQueries(scopeId);
            Physics2D.queriesHitTriggers = queriesHitTriggers;
            Physics2D.queriesStartInColliders = queriesStartInColliders;
        }

        /// <summary>
        /// pop last values from stack and apply them as current
        /// </summary>
        public static void PopQueries() => PhysicUtil.PopQueries(NONE_SCOPE_ID);

        private static void PopQueries(int scopeId)
        {
            if (_physics2DQueries.Count <= 0)
                return;

            var lastQueries = _physics2DQueries.Peek();
            if (lastQueries.scopeId != scopeId)
                return;
            _physics2DQueries.Pop();
            _scopes.Remove(scopeId);
            if (_scopes.Count <= 0)
                _lastScopeId = NONE_SCOPE_ID;

            Physics2D.queriesHitTriggers = lastQueries.queriesHitTriggers;
            Physics2D.queriesStartInColliders = lastQueries.queriesStartInColliders;
        }

        public class Physic2DQueriesScope : IDisposable
        {
            private bool _isInitialized;
            private readonly int _id;

            public Physic2DQueriesScope(bool queriesHitTriggers, bool queriesStartInColliders)
            {
                _isInitialized = true;
                _lastScopeId++;
                _id = _lastScopeId;
                PhysicUtil.PushQueries(_id);
            }

            public static Physic2DQueriesScope OlyTriggers => new Physic2DQueriesScope(true, false);
            public static Physic2DQueriesScope OlyInsigeColliders => new Physic2DQueriesScope(false, true);
            public static Physic2DQueriesScope All => new Physic2DQueriesScope(true, true);

            public void Dispose()
            {
                if (!_isInitialized)
                    return;
                _isInitialized = false;
                PhysicUtil.PopQueries(_id);
            }
        }
    }
}
