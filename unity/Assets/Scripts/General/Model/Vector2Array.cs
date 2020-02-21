namespace General.Model
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Encapsulates a standard Vector2 list, useful for making 2D lists editable in the Unity Inspector
    /// </summary>
    [Serializable]
    public class Vector2Array
    {
        [SerializeField]
        public Vector2[] Points;

        public Vector2Array()
        {
            Points = new Vector2[] { };
        }

        public Vector2Array(Vector2[] points)
        {
            Points = points;
        }
    }
}