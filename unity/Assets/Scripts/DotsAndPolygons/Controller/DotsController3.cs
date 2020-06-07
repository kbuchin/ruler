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

    public class DotsController3 : DotsController
    {
        public override GameMode CurrentGamemode => GameMode.GameMode3;

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
                if (SecondPoint == null)
                {
                    HelperFunctions.print("SecondPoint was null");
                }
                else if (FirstPoint == SecondPoint)
                {
                    HelperFunctions.print("FirstPoint was same as SecondPoint");
                }
                // use isInside method to see of middle of line lies in a face
                else if (Faces.Where(it => it?.DotsFace?.OuterComponentHalfEdges != null).Any(face =>
                    IsInside(
                        face.DotsFace.OuterComponentVertices.Select(it => it.Coordinates).ToList(),
                        new LineSegment(FirstPoint.dotsVertex.Coordinates, SecondPoint.dotsVertex.Coordinates).Midpoint
                    )
                ))
                {
                    HelperFunctions.print($"Line between {FirstPoint} and {SecondPoint} lies inside face");
                }
                else if (EdgeAlreadyExists(Edges, FirstPoint?.dotsVertex, SecondPoint?.dotsVertex))
                {
                    HelperFunctions.print("edge between first and second point already exists");
                }
                else if (InterSEGtsAny(
                    new LineSegment(FirstPoint.dotsVertex.Coordinates, SecondPoint.dotsVertex.Coordinates),
                    Edges.Select(edge => edge.Segment)
                ))
                {
                    HelperFunctions.print(
                        $"Edge between first and second point intersects something ({FirstPoint.dotsVertex.Coordinates.x}, {FirstPoint.dotsVertex.Coordinates.y}), ({SecondPoint.dotsVertex.Coordinates.x}, {SecondPoint.dotsVertex.Coordinates.y})");
                }
                else
                {
                    DoMove(FirstPoint.dotsVertex, SecondPoint.dotsVertex);
                }

                FirstPoint = null;
                SecondPoint = null;
                p1Line.enabled = false;
                p2Line.enabled = false;
            }
        }

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

            IEnumerable<DotsVertex> dots = GetVerticesInConvexPosition(numberOfDots, false, radius: 3.3f);

            foreach (DotsVertex dotsVertex in dots)
            {
                GameObject dot = Instantiate(
                    dotPrefab,
                    new Vector3(dotsVertex.Coordinates.x, dotsVertex.Coordinates.y, 0),
                    Quaternion.identity
                );
                dot.transform.parent = transform;
                InstantObjects.Add(dot);
            }


            
        }
    }
}