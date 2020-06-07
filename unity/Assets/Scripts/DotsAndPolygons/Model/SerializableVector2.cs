using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DotsAndPolygons
{
    [Serializable]
    public class SerializableVector2
    {
        private float X;
        private float Y;

        public SerializableVector2(Vector2 vector2)
        {
            X = vector2.x;
            Y = vector2.y;
        }

        public Vector2 Vector2 
        {
            get
            {
                return new Vector2(X, Y);
            }
        }
    }
}
