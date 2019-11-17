namespace ConvexHull
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Data container for convex hull level, containing point set.
    /// </summary>
    [CreateAssetMenu(fileName = "hullLevelNew", menuName = "Levels/Convex Hull Level")]
    public class HullLevel : ScriptableObject
    {
        [Header("Hull Points")]
        public List<Vector2> Points = new List<Vector2>();
    }
}