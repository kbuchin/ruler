namespace KingsTaxes.Model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ktLevelNew", menuName = "Levels/Kings Taxes Level")]
    public class KingsTaxesLevel : ScriptableObject {

        [Header("Level Parameters")]
        public float TSpannerRatio = 1f;

        [Header("Settlements")]
        public List<Vector2> Villages = new List<Vector2>();
        public List<Vector2> Castles = new List<Vector2>();

    }
}