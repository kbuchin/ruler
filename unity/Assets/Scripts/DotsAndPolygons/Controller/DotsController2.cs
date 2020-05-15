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

    public class DotsController2 : DotsController
    {
        

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
                TryToPlaceLineSegment();
            }
        }

        private void TryToPlaceLineSegment()
        {
            if(EdgeIsPossible(FirstPoint, SecondPoint, Edges, Faces))
            {
                AddVisualEdge(FirstPoint, SecondPoint);

                bool faceCreated = AddEdge(FirstPoint, SecondPoint, CurrentPlayer, HalfEdges, Vertices,
                    GameMode.GameMode2, this, root) > 0.0f;

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

        public bool CheckArea() => Math.Abs(TotalAreaP1 + TotalAreaP2 - HullArea) < .001f;

        public override void CheckSolution()
        {
            if (CheckHull() && CheckArea())
            {
                FinishLevel();
            }
        }
        
        public override void InitLevel()
        {
            base.InitLevel();
            
            AddDotsInGeneralPosition();

            //faces.Add(frame);
            //LineSegment left = new LineSegment(new Vector2(-6, 3), new Vector2(-6, -3));
            //LineSegment upper = new LineSegment(new Vector2(-6, 3), new Vector2(6, 3));
            //LineSegment right = new LineSegment(new Vector2(6, 3), new Vector2(6, -3));
            //LineSegment lower = new LineSegment(new Vector2(6, -3), new Vector2(-6, -3));
            //root = new TrapDecomRoot(frame);
        }
    }
}