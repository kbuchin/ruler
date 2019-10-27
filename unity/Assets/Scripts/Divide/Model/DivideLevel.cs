namespace Divide.Model
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Data container for the divide level.
    /// Stores position vectors for the three kinds of soldiers, plus the maximum number of swaps
    /// </summary>
    [CreateAssetMenu(fileName = "divideLevelNew", menuName = "Levels/Divide Level")]
    public class DivideLevel : ScriptableObject
    {
        [Header("Level Parameters")]
        public int NumberOfSwaps = 0;

        [Header("Soldiers")]
        public List<Vector2> Spearmen;
        public List<Vector2> Archers;
        public List<Vector2> Mages;
    }
}
