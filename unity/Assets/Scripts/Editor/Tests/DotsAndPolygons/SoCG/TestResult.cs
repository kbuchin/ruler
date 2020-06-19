using DotsAndPolygons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DotsAndPolygons.HelperFunctions;

namespace DotsAndPolygons.Tests.SoCG
{
    public class TestResult
    {
        public PlayerType P1AItype { get; set; }
        public PlayerType P2AItype { get; set; }
        public GameMode GameMode { get; set; }
        public int NumberOfDots { get; set; }
        public int P1NumberOfThreads { get; set; }
        public int P2NumberOfThreads { get; set; }
        public int P1AIDepth { get; set; }
        public int P2AIDepth { get; set; }
        public float P1Threshold { get; set; }
        public float P2Threshold { get; set; }
        public float P1AVGRunningTime { get; set; }
        public float P2AVGRunningTime { get; set; }
        public float P1FinalScore { get; set; }
        public float P2FinalScore { get; set; }
        public int Winner { get; set; }

        public override string ToString()
        {
            return $"{P1AItype.ValueOf()};{P2AItype.ValueOf()};{Convert.ToInt32(GameMode)};{NumberOfDots};{P1NumberOfThreads};{P2NumberOfThreads};{P1AIDepth};{P2AIDepth};{P1Threshold};{P2Threshold};{P1AVGRunningTime};{P2AVGRunningTime};{P1FinalScore};{P2FinalScore};{Winner}\n";
        }

    }
}
