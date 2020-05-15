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
                if(EdgeIsPossible(FirstPoint, SecondPoint, Edges, Faces))
                {
                    AddVisualEdge(FirstPoint, SecondPoint);

                    bool faceCreated = AddEdge(FirstPoint, SecondPoint, CurrentPlayer, HalfEdges, Vertices,
                        GameMode.GameMode1, this, root) > 0.0f;

                    RemoveTrapDecomLines();
                    ShowTrapDecomLines();

                    if (!faceCreated)
                    {
                        CurrentPlayer = CurrentPlayer == 1 ? 2 : 1;
                        currentPlayerText.text = $"Go Player {CurrentPlayer}!";
                        currentPlayerText.gameObject.GetComponentInParent<Image>().color =
                            CurrentPlayer == 2 ? Color.blue : Color.red;
                    }

                    CheckSolution();
                }

                FirstPoint = null;
                SecondPoint = null;
                p1Line.enabled = false;
                p2Line.enabled = false;
            }
        }

        public override void CheckSolution()
        {
            if (CheckHull())
            {
                FinishLevel();
            }
        }

        public override void InitLevel()
        {
            base.InitLevel();
            
            AddDotsInGeneralPosition();
        }

    }
}