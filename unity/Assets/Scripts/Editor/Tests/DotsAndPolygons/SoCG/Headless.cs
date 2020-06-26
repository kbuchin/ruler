using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static DotsAndPolygons.HelperFunctions;

namespace DotsAndPolygons.Tests.SoCG
{
    public class Headless
    {
        [Test]
        public void Test1()
        {
            List<int> numberOfThreadsToTest = new List<int>() { 2 };
            List<GameMode> gameModes = new List<GameMode>() { GameMode.GameMode2 };
            List<float> thresholds = new List<float>() {0.5f };
            List<int> depths = new List<int>() { 4 };
            List<int> numberOfDotsToTest = new List<int>() { 10 };
            List<PlayerType> playerTypes = new List<PlayerType> { PlayerType.GreedyAi, PlayerType.MinMaxAi };
            List<TestResult> results = new List<TestResult>();
            
            foreach (GameMode mode in gameModes)
            {
                foreach (int numberOfDots in numberOfDotsToTest)
                {
                    // greedy init
                    foreach (float threshold1 in thresholds)
                    {
                        foreach (int deptsp1 in depths)
                        {
                            foreach(int numberOfThreadsp1 in numberOfThreadsToTest)
                            {
                                foreach (int deptsp2 in depths)
                                {
                                    foreach (int numberOfThreadsp2 in numberOfThreadsToTest)
                                    {
                                        foreach (float threshold2 in thresholds)
                                        {
                                            for(int i = 0; i < 3; i++)
                                            {
                                                TestResult result = new TestResult()
                                                {
                                                    NumberOfDots = numberOfDots,
                                                    P1AIDepth = deptsp1,
                                                    P2AIDepth = deptsp2,
                                                    P1Threshold = threshold1,
                                                    P2Threshold = threshold2,
                                                    P1NumberOfThreads = numberOfThreadsp1,
                                                    P2NumberOfThreads = numberOfThreadsp2,
                                                    GameMode = mode,
                                                    P1AItype = PlayerType.MinMaxAi,
                                                    P2AItype = PlayerType.MinMaxAi
                                                };
                                                HeadlessDotsController controller = null;
                                                switch (mode)
                                                {
                                                    case GameMode.GameMode1:
                                                        controller = new HeadlessDotsController1(new MinMaxAi(PlayerNumber.Player1, mode, deptsp1, threshold1, numberOfThreadsp1),
                                                                                                new MinMaxAi(PlayerNumber.Player2, mode, deptsp2, threshold2, numberOfThreadsp2), numberOfDots);
                                                        break;
                                                    case GameMode.GameMode2:
                                                        controller = new HeadlessDotsController2(new MinMaxAi(PlayerNumber.Player1, mode, deptsp1, threshold1, numberOfThreadsp1),
                                                                                                new MinMaxAi(PlayerNumber.Player2, mode, deptsp2, threshold2, numberOfThreadsp2), numberOfDots);
                                                        break;
                                                    case GameMode.GameMode3:
                                                        controller = new HeadlessDotsController2(new MinMaxAi(PlayerNumber.Player1, mode, deptsp1, threshold1, numberOfThreadsp1),
                                                                                                new MinMaxAi(PlayerNumber.Player2, mode, deptsp2, threshold2, numberOfThreadsp2), numberOfDots);
                                                        break;
                                                }
                                                controller.Start();
                                                result.P1AVGRunningTime = controller.AvgAIRunningTime1.Average();
                                                result.P2AVGRunningTime = controller.AvgAIRunningTime2.Average();
                                                result.P1FinalScore = controller.TotalAreaP1;
                                                result.P2FinalScore = controller.TotalAreaP2;
                                                result.Winner = controller.TotalAreaP1 > controller.TotalAreaP2 ? 1 : 2;
                                                results.Add(result);
                                            }
                                             
                                            HelperFunctions.print("Finished with case",true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            using (FileStream writer2 = new FileStream($"D:/repos/ruler/testresults/results-{DateTime.Now:dd-MM-yyyy-HH.mm}.csv", FileMode.Create, FileAccess.Write))
            {
                using(StreamWriter writer = new StreamWriter(writer2))
                {
                    writer.WriteLine("P1AItype;P2AIType;GameMode;NumberOfDots;P1NumerOfThreads;P2NumberOfThreads;P1AIDepth;P2AIDepth;P1Threshold;P2Threshold;P1AVGRunningTime;P2AVGRunningTime;P1FinalScore;P2FinalScore;Winner");
                    foreach (TestResult result in results)
                    {
                        writer.Write(result.ToString());
                    }
                    writer.Close();
                }
            }
            
            Assert.True(true);
        }
    }
}