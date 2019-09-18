namespace ArtGallery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Algorithms.Polygon;
    using Util.Geometry.Polygon;

    class ArtGallerySolution : ScriptableObject
    {
        private List<GameObject> m_objects;

        private List<ArtGalleryLightHouse> m_lighthouses;

        public int Count { get { return m_lighthouses.Count; } }

        public float Area
        {
            get
            {
                if (Count <= 0) return 0f;

                var visiblePolygon = new MultiPolygon2D(m_lighthouses[0].VisionArea);
                foreach (ArtGalleryLightHouse lighthouse in m_lighthouses.Skip(1))
                {
                    Clipper.CutOut(visiblePolygon, lighthouse.VisionArea);
                    visiblePolygon.AddPolygon(lighthouse.VisionArea);
                }

                return visiblePolygon.Area;
            }
        }

        public ArtGallerySolution()
        {
            m_objects = new List<GameObject>();
            m_lighthouses = new List<ArtGalleryLightHouse>();
        }

        public void AddLighthouse(ArtGalleryLightHouse m_lighthouse)
        {
            m_lighthouses.Add(m_lighthouse);
        }

        public void AddLighthouse(GameObject obj)
        {
            // remember object for removal
            m_objects.Add(obj);

            var m_lighthouse = obj.GetComponent<ArtGalleryLightHouse>();
            m_lighthouses.Add(m_lighthouse);
        }

        public void RemoveLighthouse(ArtGalleryLightHouse m_lighthouse)
        {
            m_lighthouses.Remove(m_lighthouse);
            Destroy(m_lighthouse);
        }

        public void Clear()
        {
            foreach (var lh in m_lighthouses) Destroy(lh);
            foreach (var obj in m_objects) Destroy(obj);
            m_lighthouses.Clear();
        }

        public void OnDestroy()
        {
            Clear();
        }
    }
}
