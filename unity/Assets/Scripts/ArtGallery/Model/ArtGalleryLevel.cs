namespace ArtGallery
{
    using General.Model;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry.Polygon;

    [CreateAssetMenu(fileName = "agLevelNew", menuName = "Levels/Art Gallery Level")]
    public class ArtGalleryLevel : ScriptableObject
    {
        [Header("Level Parameters")]
        public int MaxNumberOfLighthouses = 1;

        [Header("Polygon")]
        public Vector2Array Outer = new Vector2Array(new Vector2[]
        {
            // default triangle
            new Vector2(1, 1), new Vector2(2, 0), new Vector2(0, 0)
        });


        public Polygon2D Polygon
        {
            get { return new Polygon2D(Outer.Points); }
        }
    }
}
