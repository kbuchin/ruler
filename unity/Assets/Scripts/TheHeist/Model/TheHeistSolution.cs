namespace TheHeist
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Algorithms.Polygon;

    /// <summary>
    /// Stores a list of lighthouses and can calculate their combined visibility area.
    /// Handles destruction of lighthouse game objects.
    /// </summary>
    class TheHeistSolution : ScriptableObject
    {
        /// <summary>
        /// The number of lighthouses placed
        /// </summary>
        public int Count { get { return m_guards.Count; } }

        /// <summary>
        /// Total area visible by all lighthouses
        /// </summary>
        public float Area
        {
            get
            {
                if (Count <= 0) return 0f;

                
                // create multi polygon of visibility area
                /*
                var visiblePolygon = new MultiPolygon2D(m_guards[0].VisionPoly);

                
                // add visibility polygons, cutting out the overlap
                foreach (TheHeistGuard lighthouse in m_guards.Skip(1))
                {
                    visiblePolygon = Clipper.CutOut(visiblePolygon, lighthouse.VisionPoly);
                    visiblePolygon.AddPolygon(lighthouse.VisionPoly);
                }
                */

                var visiblePolygon = new UnionSweepLine().Union(m_guards.Select(lh => lh.VisionPoly).ToList());

                // return total area
                return visiblePolygon.Area;
            }
        }

        // collection of lighthouses
        public List<TheHeistGuard> m_guards;

        // stores lighthouse objects for easy destroyal
        private List<GameObject> m_objects;

        public TheHeistSolution()
        {
            m_objects = new List<GameObject>();
            m_guards = new List<TheHeistGuard>();
        }

        /// <summary>
        /// Add lighthouse to solution.
        /// </summary>
        /// <param name="m_guard"></param>
        public void AddGuard(TheHeistGuard m_guard)
        {
            m_guards.Add(m_guard);
        }

        /// <summary>
        /// Create a player object for given game object and add to solution.
        /// </summary>
        /// <param name="obj"></param>
        public void AddGuard(GameObject obj)
        {
            // remember object for removal
            m_objects.Add(obj);

            // add the lighthouse component of game object to solution
            AddGuard(obj.GetComponent<TheHeistGuard>());
        }

        /// <summary>
        /// Add lighthouse to solution.
        /// </summary>
        /// <param name="m_player"></param>
        public void PlacePlayer(TheHeistLightHouse m_player)
        {
            //m_guards.Add(m_guard); // add player somewhere
        }

        /// <summary>
        /// Create a player object for given game object and add to solution.
        /// </summary>
        /// <param name="obj"></param>
        public void PlacePlayer(GameObject obj)
        {
            // remember object for removal
            m_objects.Add(obj);

            // add the lighthouse component of game object to solution
            PlacePlayer(obj.GetComponent<TheHeistLightHouse>());
        }

        /// <summary>
        /// Remove the given lighthouse from the solution
        /// </summary>
        /// <param name="m_guard"></param>
        public void RemoveLighthouse(TheHeistGuard m_guard)
        {
            m_guards.Remove(m_guard);
            Destroy(m_guard);
        }

        /// <summary>
        /// Clears the lighthouse lists and destroys all corresponding game objects
        /// </summary>
        public void Clear()
        {
            foreach (var lh in m_guards) Destroy(lh);
            foreach (var obj in m_objects) Destroy(obj);
            m_guards.Clear();
        }

        public void OnDestroy()
        {
            Clear();
        }
    }
}
