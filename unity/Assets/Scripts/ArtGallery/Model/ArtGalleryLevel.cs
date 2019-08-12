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

        public float Scale = 1f;

        public bool Centering = true;

        [Header("Polygon")]
        [SerializeField]
        private Vector2Array Outer = new Vector2Array(new Vector2[]
        {
            // default triangle
            new Vector2(1, 1), new Vector2(2, 0), new Vector2(0, 0)
        });

        [SerializeField]
        private Vector2Array[] Holes = new Vector2Array[] { };


        public Polygon2DWithHoles Polygon
        {
            get
            {
                var offset = new Vector2();
                if(Centering)
                {
                    offset = new Vector2(Outer.Points.Average(p => p.x), Outer.Points.Average(p => p.y));
                }

                // apply scale and offset
                var transformedVertices = Outer.Points.Select(p => (p - offset) * Scale);

                var poly = new Polygon2DWithHoles(new Polygon2D(transformedVertices));

                foreach (var hole in Holes)
                {
                    // apply scale and offset
                    transformedVertices = hole.Points.Select(p => (p - offset) * Scale);

                    poly.AddHole(new Polygon2D(transformedVertices));
                }

                return poly;
            }
        }
    }
}
