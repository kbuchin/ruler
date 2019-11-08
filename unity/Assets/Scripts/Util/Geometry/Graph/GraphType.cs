namespace Util.Geometry.Graph
{
    /// <summary>
    /// Struct that captures some basic graph properties.
    /// Currently, whether it is directed and/or simple.
    /// </summary>
    public struct GraphType
    {
        public bool DIRECTED { get; set; }
        public bool SIMPLE { get; set; }

        public GraphType(bool dir, bool simple)
        {
            DIRECTED = dir;
            SIMPLE = simple;
        }
    }
}
