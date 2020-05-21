using System;
using General.Model;
using UnityEngine;
using Util.Geometry;
using static DotsAndPolygons.HelperFunctions;

namespace DotsAndPolygons
{
    public class UnityTrapDecomLine : MonoBehaviour
    {
        public static GameObject CreateUnityTrapDecomLine(LineSegment line, DotsController gameController) =>
            CreateUnityTrapDecomLine(line.Point1, line.Point2, gameController);


        public static GameObject CreateUnityTrapDecomLine(Vector2 point1, Vector2 point2,
            DotsController gameController)
        {
            if (Math.Abs(point1.x - point2.x) < BIETJE && Math.Abs(point1.y - point2.y) < BIETJE)
            {
                return null;
            }
            GameObject segmentMesh = Instantiate(
                gameController.trapDecompEdgeMeshPrefab,
                Vector3.forward,
                Quaternion.identity);
            segmentMesh.transform.parent = gameController.transform;
            segmentMesh.transform.Translate(0, 0, 1);
            gameController.InstantObjects.Add(segmentMesh);

            var unityTrapDecomLine = segmentMesh.GetComponent<UnityTrapDecomLine>();
            unityTrapDecomLine.Segment = new LineSegment(point1, point2);
            unityTrapDecomLine._mGameController = gameController;

            var edgeMeshScript = segmentMesh.GetComponent<ReshapingMesh>();
            edgeMeshScript.CreateNewMesh(point1, point2);

            return segmentMesh;
        }

        public LineSegment Segment { get; set; }

        private DotsController _mGameController;

        private void Awake()
        {
            _mGameController = FindObjectOfType<DotsController>();
        }
    }
}