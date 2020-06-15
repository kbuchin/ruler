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
        private static long startTime;
        public static void StartTimer()
        {
            startTime = DateTime.Now.Ticks;
        }

        public static void StopTimer()
        {
            long stopTime = DateTime.Now.Ticks;
            TimeSpan span = new TimeSpan(stopTime - startTime);
            HelperFunctions.print($"Code took {span.Seconds} s", debug: true);
        }
    }
}
