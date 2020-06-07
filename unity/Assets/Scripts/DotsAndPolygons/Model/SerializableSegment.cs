using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util.Geometry;

namespace DotsAndPolygons
{
    [Serializable]
    public class SerializableSegment
    {
        private SerializableVector2 point1;
        private SerializableVector2 point2;

        public SerializableSegment(LineSegment segment)
        {
            point1 = new SerializableVector2(segment.Point1);
            point2 = new SerializableVector2(segment.Point2);
        }

        public LineSegment LineSegment
        {
            get
            {
                return new LineSegment(point1.Vector2, point2.Vector2);
            }
        }
    }
}
