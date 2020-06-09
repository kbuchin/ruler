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
        public DotsVertex A { get; set; }
        public DotsVertex B { get; set; }
        public List<ValueMove> Path { get; set; } = new List<ValueMove>();
        public PlayerNumber playerNumber { get; set; }
        public PotentialMove(DotsVertex A, DotsVertex B)
        {
            this.A = A;
            this.B = B;
        }

        public abstract float GetValue();
    }
    
    public sealed class ValueMove : PotentialMove
    {
        public float BestValue { get; set; }        

        //private float ThisPlayerArea { get; set; }

        public ValueMove(float bestValue, DotsVertex A, DotsVertex B) : base(A, B)
        {
            BestValue = bestValue;
        }
        public override float GetValue() => BestValue;

        public override string ToString() => $"ValueMove(Value = {BestValue}, A = {A}, B = {B})";
    }
    

    public sealed class AreaMove : PotentialMove
    {
        public float MaxArea { get; set; }
        public AreaMove(float maxArea, DotsVertex A, DotsVertex B) : base(A, B)
        {
            MaxArea = maxArea;
        }
        public override float GetValue() => MaxArea;

        public override string ToString() => $"AreaMove(MaxArea = {MaxArea}, A = {A}, B = {B})";
    }

    public sealed class WeightMove : PotentialMove
    {
        public float MinWeight { get; set; }
        public WeightMove(float minWeight, DotsVertex A, DotsVertex B) : base (A, B)
        {
            MinWeight = minWeight;
        }

        public override float GetValue() => MinWeight;
        
        public override string ToString() => $"WeightMove(MaxArea = {MinWeight}, A = {A}, B = {B})";
    }
}
