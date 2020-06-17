using DotsAndPolygons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotsAndPolygons
{
    public class MoveCollection
    {
        public float Value { get; set; }
        public List<PotentialMove> PotentialMoves { get; set; } = new List<PotentialMove>();

        public MoveCollection(float value)
        {
            Value = value;
        }
    }
}
