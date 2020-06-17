using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace DotsAndPolygons
{
    [Serializable]
    public class Dcel
    {
        public DotsVertex[] Vertices { get; set; }

        public HashSet<DotsEdge> Edges { get; set; }
        
        public HashSet<DotsHalfEdge> HalfEdges { get; set; }

        public HashSet<DotsFace> DotsFaces { get; set; }

        public Dcel(DotsVertex[] vertices, HashSet<DotsEdge> edges, HashSet<DotsHalfEdge> halfEdges, HashSet<DotsFace> dotsFaces)
        {
            Vertices = vertices;
            Edges = edges;
            HalfEdges = halfEdges;
            DotsFaces = dotsFaces;
        }

        public Dcel Clone()
        {
            using(var stream = new MemoryStream())
            {
                var b = new BinaryFormatter();
                b.Serialize(stream, this);
                stream.Position = 0;
                var returner = (Dcel) b.Deserialize(stream);
                stream.Close();
                //returner.Edges.Select(x => new Edge());

                //set references to og vertices
                for (var i = 0; i < returner.Vertices.Length; i++)
                {
                    returner.Vertices[i].Original = Vertices[i];
                }
                return returner;
            }
            
        }
    }
}
