using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace DotsAndPolygons
{
    [Serializable]
    public class DCEL
    {
        public DotsVertex[] Vertices { get; set; }

        public HashSet<DotsEdge> Edges { get; set; }
        
        public HashSet<DotsHalfEdge> HalfEdges { get; set; }

        public HashSet<DotsFace> DotsFaces { get; set; }

        public DCEL(DotsVertex[] vertices, HashSet<DotsEdge> edges, HashSet<DotsHalfEdge> halfEdges, HashSet<DotsFace> dotsFaces)
        {
            Vertices = vertices;
            Edges = edges;
            HalfEdges = halfEdges;
            DotsFaces = dotsFaces;
        }

        public DCEL Clone()
        {
            using(MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(stream, this);
                stream.Position = 0;
                var returner = (DCEL) b.Deserialize(stream);
                stream.Close();
                return returner;
            }
        }
    }
}
