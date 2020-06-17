using System;
using System.Collections.Generic;
using System.Linq;
using DotsAndPolygons;
using General.Model;
using UnityEngine;
using UnityEngine.UI;
using Util.Geometry;
using Random = UnityEngine.Random;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty

namespace DotsAndPolygons
{
    using static HelperFunctions;

    public class HeadlessDotsController3 : HeadlessDotsController
    {
        public HeadlessDotsController3(
            DotsPlayer player1,
            DotsPlayer player2,
            int numberOfDots = 20
        ) : base(player1, player2, numberOfDots)
        {
        }
        
        public override GameMode CurrentGamemode => GameMode.GameMode3;
        
        public bool CheckArea() => Math.Abs(TotalAreaP1 + TotalAreaP2 - HullArea) < .001f;

        public override bool CheckSolutionOfGameState()
        {
            if (CheckHull())
            {
                FinishLevel();
                return true;
            }
            return false;
        }

        public override void InitLevel()
        {
            base.InitLevel();

            IEnumerable<DotsVertex> dots = GetVerticesInConvexPosition(NumberOfDots, false, radius: 3.3f);
            Vertices = dots.ToHashSet();
        }
    }
}