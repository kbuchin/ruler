using UnityEngine.UI;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable ConvertToAutoProperty

namespace DotsAndPolygons
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using Util.Geometry.Polygon;
    using Util.Geometry;
    using static HelperFunctions;

    public class DotsController1 : DotsController
    {
        
        public override GameMode CurrentGamemode => GameMode.GameMode1;
        
        // Update is called once per frame
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                _showTrapDecomLines = !_showTrapDecomLines;
                if (_showTrapDecomLines)
                    ShowTrapDecomLines();
                else
                    RemoveTrapDecomLines();
            }

            // User clicked a point and is drawing line from starting point
            if (FirstPoint == null) return;
            // User is holding mouse button
            if (Input.GetMouseButton(0))
            {
                // update edge endpont
                Camera mainCamera = Camera.main;
                if (mainCamera == null) return;
                Vector3 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition + 10 * Vector3.forward);

                SetDrawingLinePosition(1, pos);
            }
            else // User let go of mouse button
            {
                if (EdgeIsPossible(FirstPoint?.dotsVertex, SecondPoint?.dotsVertex, Edges.Select(x => x.DotsEdge), Faces.Select(x => x.DotsFace).ToHashSet()))
                {
                    DoMove(FirstPoint?.dotsVertex, SecondPoint?.dotsVertex);
                }

                FirstPoint = null;
                SecondPoint = null;
                p1Line.enabled = false;
                p2Line.enabled = false;
            }
        }

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

            AddDotsInGeneralPosition();
        }
    }
}