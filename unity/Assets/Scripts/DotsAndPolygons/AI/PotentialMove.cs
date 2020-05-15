using DotsAndPolygons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DotsAndPolygons.AI
{
    public class PotentialMove
    {
        public IDotsVertex A { get; set; }
        public IDotsVertex B { get; set; }
        public PotentialMove(IDotsVertex A, IDotsVertex B)
        {
            this.A = A;
            this.B = B;
        }
    }

    public class AreaMove : PotentialMove
    {
        public float MaxArea { get; set; }
        public AreaMove(float maxArea, IDotsVertex A, IDotsVertex B) : base(A, B)
        {
            MaxArea = maxArea;
        }

        
    }

    public class WeightMove : PotentialMove
    {
        public float MinWeight { get; set; }
        public WeightMove(float minWeight, IDotsVertex A, IDotsVertex B) : base (A, B)
        {
            MinWeight = minWeight;
        }
    }
}
