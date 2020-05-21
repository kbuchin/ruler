using DotsAndPolygons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotsAndPolygons
{
    public abstract class PotentialMove
    {
        public IDotsVertex A { get; set; }
        public IDotsVertex B { get; set; }
        public PotentialMove(IDotsVertex A, IDotsVertex B)
        {
            this.A = A;
            this.B = B;
        }

        public abstract float GetValue();
    }

    public sealed class AreaMove : PotentialMove
    {
        public float MaxArea { get; set; }
        public AreaMove(float maxArea, IDotsVertex A, IDotsVertex B) : base(A, B)
        {
            MaxArea = maxArea;
        }
        public override float GetValue() => MaxArea;

        public override string ToString() => $"AreaMove(MaxArea = {MaxArea}, A = {A}, B = {B})";
    }

    public sealed class WeightMove : PotentialMove
    {
        public float MinWeight { get; set; }
        public WeightMove(float minWeight, IDotsVertex A, IDotsVertex B) : base (A, B)
        {
            MinWeight = minWeight;
        }

        public override float GetValue() => MinWeight;
        
        public override string ToString() => $"WeightMove(MaxArea = {MinWeight}, A = {A}, B = {B})";
    }
}
