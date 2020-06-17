using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DotsAndPolygons
{
    class Timer
    {
        private long startTime;

        public Timer()
        {

        }
        public void StartTimer()
        {
            startTime = DateTime.Now.Ticks;
        }

        public void StopTimer()
        {
            long stopTime = DateTime.Now.Ticks;
            TimeSpan span = new TimeSpan(stopTime - startTime);
            HelperFunctions.print($"Code took {span.Seconds} s");
        }
    }
}
