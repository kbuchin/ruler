namespace Divide.Model
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

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
