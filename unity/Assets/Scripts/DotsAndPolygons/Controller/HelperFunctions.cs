using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ClipperLib;
using JetBrains.Annotations;
using UnityEngine;
using Util.Geometry;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DotsAndPolygons
{
    using Path = List<IntPoint>;
    using Paths = List<List<IntPoint>>;

    public static class HelperFunctions
    {
        public const float BIETJE = .0001f;
        public static void print(object message, bool debug = true) // If you want print statements set debug = true
        {
            if (debug) MonoBehaviour.print(message);
        }

        public static float GetAreaOfAllInnerComponents(this DotsFace dotsFace)
        {
            // for safety
            if (dotsFace.InnerComponents.Count == 0) return 0f;

            float innerComponentsArea = dotsFace.InnerComponents.Select(it => it.IncidentFace?.Area ?? 0f).Sum();
            if (innerComponentsArea > dotsFace.Area)
                print(
                    $"ERROR: innerComponents are larger than face itself\nFace: {string.Join(", ", dotsFace.OuterComponentVertices.Select(it => it.Coordinates))}\nInnerComponentsFaces: \n    [{string.Join("],\n    [", dotsFace.InnerComponents.Select(it => string.Join(", ", it.IncidentFace.OuterComponentVertices.Select(it2 => it2.Coordinates))).Distinct())}]");
            return innerComponentsArea;
        }

        public static float Distance(Vector2 point1, Vector2 point2) =>
            Mathf.Sqrt(
                Mathf.Pow(Mathf.Abs(point1.x - point2.x), 2)
                + Mathf.Pow(Mathf.Abs(point1.y - point2.y), 2)
            );

        public static float ShortestPointDistance(Vector2 point, IEnumerable<Vector2> set)
        {
            float min = float.MaxValue;
            foreach (Vector2 setPoint in set)
            {
                float dist = Distance(setPoint, point);
                if (dist < min) min = dist;
            }

            return min;
        }

        public static bool IsOnLine(Vector2 a_Point, Line line, float diff = .2f) =>
            line.DistanceToPoint(a_Point) < diff;

        public static bool Colinear(Vector2 a, Vector2 b, Vector2 c, float diff = .2f) =>
            IsOnLine(c, new Line(a, b), diff);

        // Return whether adding the new Vector2 point in the HashSet set would result in a General Position
        // set is assumed to be general already
        public static bool IsInGeneralPosition(
            Vector2 point,
            IEnumerable<Vector2> set,
            float colinearDiff = .14f // .065f
        ) =>
            set.All(setPoint =>
                // first check if the x and y coordinates differ enough
//                Math.Abs(setPoint.x - point.x) >= coordinateDiff
//                && Math.Abs(setPoint.y - point.y) >= coordinateDiff
                // then check if the distance between the line between this point and the current setPoint has a great enough distance for all points in the set
//                && 
                set.All(otherSetPoint =>
                    point == setPoint || point == otherSetPoint || setPoint == otherSetPoint ||
                    !Colinear(point, setPoint, otherSetPoint, colinearDiff)
                )
            );

        public static IEnumerable<DotsHalfEdge> LeavingHalfEdges(this DotsVertex vertex)
        {
            var leavingEdges = new List<DotsHalfEdge>();
            DotsHalfEdge current = vertex.IncidentEdge;
            if (current == null) return leavingEdges; // vertex does not have any edges
            do
            {
                // print($"current leaving half edge {current}");
                leavingEdges.Add(current);
                if (current.Twin.Next == current) break;
                current = current.Twin.Next;
            } while (current != vertex.IncidentEdge /* && current != null*/);

            return leavingEdges;
        }

        // Returns the faces and Inner Components of those faces that connect to this vertex
        public static Dictionary<DotsFace, DotsHalfEdge> GetNeighbouringFaces(this DotsVertex vertex)
        {
            var result = new Dictionary<DotsFace, DotsHalfEdge>();
            foreach (DotsHalfEdge dotsHalfEdge in vertex.LeavingHalfEdges()
                .Where(it => it.IncidentFace != null && !result.ContainsKey(it.IncidentFace)))
            {
                result[dotsHalfEdge.IncidentFace] = dotsHalfEdge;
            }

            return result;
        }

        /** Returns the angle between abc */
        public static double AngleVertices(Vector2 a, Vector2 b, Vector2 c)
        {
            Vector2 endAlpha = a - b;
            Vector2 endBeta = c - b;
            double angle = Math.Atan2(endAlpha.x * endBeta.y - endBeta.x * endAlpha.y,
                endAlpha.x * endBeta.x + endAlpha.y * endBeta.y) * 180.0 / Math.PI;

            double result = (angle + 360.0) % 360.0;
            return Math.Abs(result) < BIETJE ? 360.0 : result;
        }

        /** returns angle between three vertices with the overlapping vertex between alpha and beta as the middle */
        public static double AngleVertices(DotsHalfEdge alpha, DotsHalfEdge beta)
        {
            Vector2 alpha1 = alpha.Origin.Coordinates;
            Vector2 alpha2 = alpha.Twin.Origin.Coordinates;

            Vector2 beta1 = beta.Origin.Coordinates;
            Vector2 beta2 = beta.Twin.Origin.Coordinates;

            Vector2 endAlpha;
            Vector2 endBeta;

            if (alpha1 == beta1)
            {
                endAlpha = alpha2 - alpha1;
                endBeta = beta2 - beta1;
            }
            else if (alpha1 == beta2)
            {
                endAlpha = alpha2 - alpha1;
                endBeta = beta1 - beta2;
            }
            else if (alpha2 == beta1)
            {
                endAlpha = alpha1 - alpha2;
                endBeta = beta2 - beta1;
            }
            else if (alpha2 == beta2)
            {
                endAlpha = alpha1 - alpha2;
                endBeta = beta1 - beta2;
            }
            else
            {
                throw new Exception($"your two half edges {alpha} and {beta} do not overlap in any of their vertices");
            }

            double angle = Math.Atan2(endAlpha.x * endBeta.y - endBeta.x * endAlpha.y,
                endAlpha.x * endBeta.x + endAlpha.y * endBeta.y) * 180.0 / Math.PI;

            double result = (angle + 360.0) % 360.0;
            return Math.Abs(result) < BIETJE ? 360.0 : result;
        }

        public static bool EdgeAlreadyExists(IEnumerable<DotsEdge> edges, DotsVertex point1, DotsVertex point2) =>
            edges.Where(it => it != null).Any(edge =>
                edge.Segment.Point1 == point1.Coordinates && edge.Segment.Point2 == point2.Coordinates
                || edge.Segment.Point2 == point1.Coordinates && edge.Segment.Point1 == point2.Coordinates
            );


        public static void RemoveFromDCEL(DotsHalfEdge halfEdgeToRemove)
        {
            DotsHalfEdge twinHalfEdgeToRemove = halfEdgeToRemove.Twin;

            List<DotsHalfEdge> destinationLeavingHalfEdges = twinHalfEdgeToRemove.Origin.LeavingHalfEdges()
                .Where(it => !it.Equals(twinHalfEdgeToRemove))
                .ToList();

            if (destinationLeavingHalfEdges.Any())
            {
                twinHalfEdgeToRemove.Prev.Next = halfEdgeToRemove.Next;
                halfEdgeToRemove.Next.Prev = twinHalfEdgeToRemove.Prev;
            }

            DotsVertex dest = twinHalfEdgeToRemove.Origin;
            if (twinHalfEdgeToRemove.Equals(dest.IncidentEdge))
            {
                dest.IncidentEdge = destinationLeavingHalfEdges.FirstOrDefault();
            }
            
            
            List<DotsHalfEdge> originLeavingHalfEdges = halfEdgeToRemove.Origin.LeavingHalfEdges()
                .Where(it => !it.Equals(halfEdgeToRemove))
                .ToList();

            if (originLeavingHalfEdges.Any())
            {
                halfEdgeToRemove.Prev.Next = twinHalfEdgeToRemove.Next;
                twinHalfEdgeToRemove.Next.Prev = halfEdgeToRemove.Prev;
            }

            DotsVertex orig = halfEdgeToRemove.Origin;
            if (halfEdgeToRemove.Equals(orig.IncidentEdge))
            {
                orig.IncidentEdge = originLeavingHalfEdges.FirstOrDefault();
            }

        }

        // by jolan
        public static void AssignNextAndPrev(DotsHalfEdge newHalfEdge)
        {
            DotsVertex origin = newHalfEdge.Origin;
            DotsVertex destination = newHalfEdge.Destination;

            DotsHalfEdge newHalfEdgeTwin = newHalfEdge.Twin;

            List<DotsHalfEdge> destinationLeavingHalfEdges = destination.LeavingHalfEdges().ToList();
            List<DotsHalfEdge> originLeavingHalfEdges = origin.LeavingHalfEdges().ToList();

            if (destinationLeavingHalfEdges.Count == 0)
            {
                newHalfEdge.Next = newHalfEdgeTwin;
                newHalfEdgeTwin.Prev = newHalfEdge;
            }
            else
            {
                // fit in new half edge
                destinationLeavingHalfEdges.Sort((a, b) =>
                    AngleVertices(origin.Coordinates, destination.Coordinates /* == a.Origin*/,
                            a.Destination.Coordinates)
                        .CompareTo(
                            AngleVertices(origin.Coordinates, destination.Coordinates, b.Destination.Coordinates)
                        )
                );
                DotsHalfEdge newNext = destinationLeavingHalfEdges.First(); // take the one with the smallest angle

                newNext.Prev = newHalfEdge;
                newHalfEdge.Next = newNext;

                // fit in new half edge twin
                destinationLeavingHalfEdges.Sort((a, b) =>
                    (AngleVertices(origin.Coordinates, destination.Coordinates /* == a.Origin*/,
                        a.Destination.Coordinates) % 360)
                    .CompareTo(
                        AngleVertices(origin.Coordinates, destination.Coordinates, b.Destination.Coordinates) % 360
                    )
                );
                DotsHalfEdge
                    newTwinPrev =
                        destinationLeavingHalfEdges.Last()
                            .Twin; // take the one with largest angle excluding 360 degrees
                newTwinPrev.Next = newHalfEdgeTwin;
                newHalfEdgeTwin.Prev = newTwinPrev;
            }

            if (originLeavingHalfEdges.Count == 0)
            {
                newHalfEdge.Prev = newHalfEdgeTwin;
                newHalfEdgeTwin.Next = newHalfEdge;
            }
            else
            {
                // fit in new half edge
                originLeavingHalfEdges.Sort((a, b) =>
                    AngleVertices(a.Destination.Coordinates, origin.Coordinates /* == a.Destination*/,
                            destination.Coordinates)
                        .CompareTo(
                            AngleVertices(b.Destination.Coordinates, newHalfEdge.Origin.Coordinates,
                                destination.Coordinates)
                        )
                );
                DotsHalfEdge newPrevious = originLeavingHalfEdges.First().Twin;

                newPrevious.Next = newHalfEdge;
                newHalfEdge.Prev = newPrevious;

                // fit in new half edge twin
                originLeavingHalfEdges.Sort((a, b) =>
                    (AngleVertices(a.Destination.Coordinates, origin.Coordinates /* == a.Destination*/,
                        destination.Coordinates) % 360)
                    .CompareTo(
                        AngleVertices(b.Destination.Coordinates, newHalfEdge.Origin.Coordinates,
                            destination.Coordinates) % 360
                    )
                );
                DotsHalfEdge
                    newTwinNext =
                        originLeavingHalfEdges.Last(); // take the one with largest angle excluding 360 degrees

                newTwinNext.Prev = newHalfEdgeTwin;
                newHalfEdgeTwin.Next = newTwinNext;
            }
        }

        // Kotlin: fun <T, R> T.let(block: (T) -> R): R
        public static TR Let<T, TR>(this T self, Func<T, TR> block)
        {
            return block(self);
        }

        // Kotlin: fun <T> T.also(block: (T) -> Unit): T
        public static T Also<T>(this T self, Action<T> block)
        {
            block(self);
            return self;
        }

        public enum GameMode
        {
            GameMode1,
            GameMode2,
            GameMode3
        }

        public static HashSet<DotsHalfEdge> HalfEdges(this DotsEdge dotsEdge) =>
            new HashSet<DotsHalfEdge> {dotsEdge.LeftPointingHalfEdge, dotsEdge.RightPointingHalfEdge};

        /** returns true if adding edge created a face */
        public static (DotsFace, DotsFace) AddEdge(
            DotsVertex a,
            DotsVertex b,
            int currentPlayer,
            HashSet<DotsHalfEdge> m_halfEdges,
            IEnumerable<DotsVertex> allVertices,
            GameMode gameMode,
            [CanBeNull] DotsController mGameController = null,
            [CanBeNull] TrapDecomRoot root = null,
            List<DotsVertex> newlyDisabled = null
        )
        {
            // Add edge for current player and check if new face is created
            // This will create two half-edges and correctly set their attributes
            DotsHalfEdge incident = new DotsHalfEdge().Constructor(
                player: currentPlayer, origin: a, twin: null);
            DotsHalfEdge twin = new DotsHalfEdge().Constructor( player: currentPlayer,
                origin: b, twin: incident);
            incident.Twin = twin;

            // if running any gamemode, insert the new edge into the trap decomp
            DotsHalfEdge leftPointing;
            DotsHalfEdge rightPointing;
            if (incident.Origin.Coordinates.x < incident.Destination.Coordinates.x)
            {
                rightPointing = incident;
                leftPointing = twin;
            }
            else
            {
                rightPointing = twin;
                leftPointing = incident;
            }

            if (root != null) TrapDecomHelper.Insert(root, new DotsEdge(leftPointing, rightPointing));

            AssignNextAndPrev(incident); // Also assigns twin

            if (a.IncidentEdge == null)
            {
                a.IncidentEdge = incident;
            }

            if (b.IncidentEdge == null)
            {
                b.IncidentEdge = twin;
            }

            m_halfEdges.Add(incident);
            m_halfEdges.Add(twin);

            IEnumerable<DotsVertex> allVerticesNotInAFace = allVertices.Where(it => !it.InFace).ToList();

            DotsFace newFace = CreateFaceLoop(incident, gameMode, allVerticesNotInAFace);
            DotsFace secondNewFace = CreateFaceLoop(twin, gameMode, allVerticesNotInAFace);

            if (newFace == null && secondNewFace == null) return (null, null);

            if (newFace != null)
            {
                newFace.Player = currentPlayer;
                // when in gamemode 1, find all newly created trap faces that lie inside the new dotsFace
                List<TrapFace> trapFacesInsideNewFace = null;
                if (gameMode == GameMode.GameMode1 && root != null)
                {
                    List<TrapFace> allTrapFaces = root.FindAllFaces();
                    trapFacesInsideNewFace = allTrapFaces.Where(it =>
                        it?.Upper?.DotsEdge?.RightPointingHalfEdge?.IncidentFace == newFace
                        || it?.Downer?.DotsEdge?.LeftPointingHalfEdge?.IncidentFace == newFace
                    ).ToList();
                    print($"Found {allTrapFaces.Count} trapfaces");
                    print($"Found {trapFacesInsideNewFace.Count} trapfaces inside new face");
                }

                Dictionary<DotsFace, DotsHalfEdge> innerFaces =
                    UpdateVerticesInFace(allVertices, newFace, trapFacesInsideNewFace, root, newlyDisabled);
                // remove new face if it happened to be inside the inner faces (this should never happen)
                innerFaces.Remove(newFace);

                // print("Inner faces");
                // foreach (KeyValuePair<IDotsFace, IDotsHalfEdge> entry in innerFaces)
                // {
                //     print(
                //         $"Face area: {entry.Key.Area}, inner component edge: [{entry.Value.Origin.Coordinates}, {entry.Value.Destination.Coordinates}]");
                // }

                newFace.InnerComponents = innerFaces.Values.ToList();
                if (mGameController != null)
                {
                    // Update the current player's total area
                    mGameController.AddToPlayerArea(currentPlayer, newFace.AreaMinusInner);
                    print(
                        $"Area of new face = {newFace.Area}, with inner faces subtracted = {newFace.AreaMinusInner}");
                    GameObject faceObject = Object.Instantiate(
                        mGameController.facePrefab,
                        new Vector3(0, 0, 0),
                        Quaternion.identity);
                    faceObject.transform.parent = mGameController.transform;
                    mGameController.InstantObjects.Add(faceObject);
                    UnityDotsFace face = faceObject.gameObject.GetComponent<UnityDotsFace>();
                    face.Constructor(newFace);
                    mGameController.Faces.Add(face);
                }
            }

            if (secondNewFace != null)
            {
                secondNewFace.Player = currentPlayer;
                // when in gamemode 1, find all newly created trap faces that lie inside the new dotsFace
                List<TrapFace> trapFacesInsideNewFace = null;
                if (gameMode == GameMode.GameMode1 && root != null)
                {
                    List<TrapFace> allTrapFaces = root.FindAllFaces();
                    trapFacesInsideNewFace = allTrapFaces.Where(it =>
                        it?.Upper?.DotsEdge?.RightPointingHalfEdge?.IncidentFace == secondNewFace
                        || it?.Downer?.DotsEdge?.LeftPointingHalfEdge?.IncidentFace == secondNewFace
                    ).ToList();
                    print($"Found {allTrapFaces.Count} trapfaces");
                    print($"Found {trapFacesInsideNewFace.Count} trapfaces inside new face");
                }

                Dictionary<DotsFace, DotsHalfEdge> innerFaces =
                    UpdateVerticesInFace(allVertices, secondNewFace, trapFacesInsideNewFace, root, newlyDisabled);
                // remove new face if it happened to be inside the inner faces (this should never happen)
                innerFaces.Remove(secondNewFace);

                // print("Inner faces");
                // foreach (KeyValuePair<IDotsFace, IDotsHalfEdge> entry in innerFaces)
                // {
                //     print(
                //         $"Face area: {entry.Key.Area}, inner component edge: [{entry.Value.Origin.Coordinates}, {entry.Value.Destination.Coordinates}]");
                // }

                secondNewFace.InnerComponents = innerFaces.Values.ToList();
                if (mGameController != null)
                {
                    // Update the current player's total area
                    mGameController.AddToPlayerArea(currentPlayer, secondNewFace.AreaMinusInner);
                    print(
                        $"Area of new face = {secondNewFace.Area}, with inner faces subtracted = {secondNewFace.AreaMinusInner}");

                    GameObject faceObject = Object.Instantiate(
                        mGameController.facePrefab,
                        new Vector3(0, 0, 0),
                        Quaternion.identity);
                    faceObject.transform.parent = mGameController.transform;
                    mGameController.InstantObjects.Add(faceObject);
                    UnityDotsFace face = faceObject.gameObject.GetComponent<UnityDotsFace>();
                    face.Constructor(secondNewFace);
                    mGameController.Faces.Add(face);
                }
            }
            return (newFace, secondNewFace);
        }

        // Given three colinear vertices u, v and w, this method checks whether vertex v is on line segment (u,w)
        public static bool OnSeg(Vector2 u, Vector2 v, Vector2 w)
        {
            if (v.Equals(u) || v.Equals(w)) return false;
            return v.x <= Math.Max(u.x, w.x) &&
                   v.x >= Math.Min(u.x, w.x) &&
                   v.y <= Math.Max(u.y, w.y) &&
                   v.y >= Math.Min(u.y, w.y);
        }

        public static bool OnSeg(Vector2 vertex, LineSegment segment)
        {
            return OnSeg(segment.Point1, vertex, segment.Point2);
        }

        // Given three vertices u, v and w in static order, this method checks in what direction the line segments
        // uv and vw make a turn/
        // 0 : u, v and w are colinear 
        // 1 : segments uv and vw make a clockwise (right) turn
        // 2 : segments uv and vw make a counterclockwise (left) turn
        public static float TurnDirection(Vector2 u, Vector2 v, Vector2 w)
        {
            float slopes = (v.y - u.y) * (w.x - v.x) - (v.x - u.x) * (w.y - v.y);

            if (Math.Abs(slopes) < BIETJE) return 0f;
            return slopes > 0 ? 1 : 2;
        }

        // Given two line segments v1u1 and v2u2, this method checks whether they intersect
        // interSEGting want SEGments die intersecten haha leuk grappig woordgrapje grts
        public static bool InterSEGting(Vector2 v1, Vector2 u1, Vector2 v2, Vector2 u2)
        {
            // Check for points starting or ending in the same vertex
            if (v1 == v2 || v1 == u2 || u1 == v2 || u1 == u2)
                return false; // As generality is assumed, so v1 cannot lie on v2u2 for instance

            // Find the four necessary directions
            float TD1 = TurnDirection(v1, u1, v2);
            float TD2 = TurnDirection(v1, u1, u2);
            float TD3 = TurnDirection(v2, u2, v1);
            float TD4 = TurnDirection(v2, u2, u1);

            // General check
            if (Math.Abs(TD1 - TD2) > BIETJE && Math.Abs(TD3 - TD4) > BIETJE) return true;

            // Colinear check 
            // v1, u1 and v2 are colinear and v2 lies on segment v1u1 
            if (Math.Abs(TD1) < BIETJE && OnSeg(v1, v2, u1)) return true;

            // v1, u1 and v2 are colinear and u2 lies on segment v1u1 
            if (Math.Abs(TD2) < BIETJE && OnSeg(v1, u2, u1)) return true;

            // v2, u2 and v1 are colinear and v1 lies on segment v2u2 
            if (Math.Abs(TD3) < BIETJE && OnSeg(v2, v1, u2)) return true;

            // v2, u2 and u1 are colinear and u1 lies on segment v2u2 
            if (Math.Abs(TD4) < BIETJE && OnSeg(v2, u1, u2)) return true;

            // Not interSEGting
            return false;
        }

        public static bool InterSEGting(LineSegment s1, LineSegment s2)
        {
            bool result = InterSEGting(s1.Point1, s1.Point2, s2.Point1, s2.Point2);
            // if (result)
            // {
            //     print(s1 + " intersects " + s2);
            // }

            return result;
        }

        public static bool InterSEGtsAny(LineSegment segment, IEnumerable<LineSegment> collection) =>
            collection.Any(otherSegment => InterSEGting(segment, otherSegment));


        // Given a list of vertices representing a face on the plane, and a vertex on the plane,
        // this method checks whether the vertex lies inside of the face.
        public static bool IsInside(List<Vector2> face, Vector2 vertex)
        {
            // vertex is on the border, so not inside the face
            if (face.Contains(vertex)) return false;
            int n = face.Count;

            var horizontalInf = new Vector2(int.MaxValue, vertex.y);

            // Count intersections of the above line  
            // with sides of polygon 
            int intersects = 0, iter = 0;
            do
            {
                int next = (iter + 1) % n;

                // Check if the line segment from vertex to horizontalInf intersects with the line  
                // segment from face[i] to face[next] 
                if (InterSEGting(face[iter], face[next], vertex, horizontalInf))
                {
                    // If vertex is colinear with line  
                    // segment 'iter-next', then check if it lies  
                    // on segment. If it does, return true, otherwise false 
                    if (Math.Abs(TurnDirection(face[iter], vertex, face[next])) < BIETJE)
                    {
                        return OnSeg(face[iter], vertex, face[next]);
                    }

                    intersects++;
                }

                iter = next;
            } while (iter != 0);

            // Return true if intersects is odd, false otherwise 
            return intersects % 2 == 1;
        }

        public static bool LineCanBeDrawnFrom(DotsVertex v) => v.LeavingHalfEdges().Any(it => it.IncidentFace == null);

        // Given a newly created half edge hEdge, this method updates the inFace values of all vertices, returns any faces and their InnerComponents inside newly created face
        public static Dictionary<DotsFace, DotsHalfEdge> UpdateVerticesInFace(IEnumerable<DotsVertex> allVertices,
            DotsFace newFace, IEnumerable<TrapFace> trapFacesInsideNewFace = null, ITrapDecomNode root = null, List<DotsVertex> newlyDisabled = null)
        {
            // Initialize an empty list of vertices on the border of the new face
            var verticesOnBorder = new List<Vector2>();

            DotsHalfEdge hEdge = newFace.OuterComponent;

            // Initialize and iteration half edge for the following while loop
            DotsHalfEdge tempEdge = hEdge;

            // Loop over half edges to find vertices on the border of the face
            do
            {
                // If a half edge does not have another half edge following it, a new face has not been created
                if (tempEdge.Next == null)
                    return new Dictionary<DotsFace, DotsHalfEdge>();
                // Add the origin of the current iterator half edge to the list of vertices on the border of the face
                verticesOnBorder.Add(tempEdge.Origin.Coordinates);
                // Update the iterator half edge
                tempEdge = tempEdge.Next;

                // When the iterator half edge has looped around we know a new face has been created
            } while (tempEdge != hEdge);

            // If the number of vertices on the border of the face is less than 3, vertices can not lie inside of it
            // For robustness, should not be of importance
            if (verticesOnBorder.Count <= 3) return new Dictionary<DotsFace, DotsHalfEdge>();

            var innerComponents = new Dictionary<DotsFace, DotsHalfEdge>();

            // Update the inFace values of all vertices that do not yet lie inside of a face
            foreach (DotsVertex v in allVertices.Where(it => !it.InFace))
            {
                if (trapFacesInsideNewFace == null || root == null) // gamemode 2
                {
                    bool isInside = IsInside(verticesOnBorder, v.Coordinates); // excludes vertices on border
                    if (verticesOnBorder.Contains(v.Coordinates) && !LineCanBeDrawnFrom(v) || isInside)
                    {
                        if (!v.OnHull)
                        {
                            v.InFace = true;
                            newlyDisabled?.Add(v);
                        }
                        if (isInside)
                            foreach (KeyValuePair<DotsFace, DotsHalfEdge> entry in v.GetNeighbouringFaces())
                                if (!innerComponents.ContainsKey(entry.Key))
                                    innerComponents[entry.Key] = entry.Value;
                    }
                }
                else // gamemode 1, 3
                {
                    bool isInside = !verticesOnBorder.Contains(v.Coordinates) &&
                                    trapFacesInsideNewFace.Any(it => it == root.query(v));
                    if (verticesOnBorder.Contains(v.Coordinates) && !LineCanBeDrawnFrom(v) || isInside)
                    {
                        if (!v.OnHull) v.InFace = true;
                        if (isInside)
                            foreach (KeyValuePair<DotsFace, DotsHalfEdge> entry in v.GetNeighbouringFaces())
                                if (!innerComponents.ContainsKey(entry.Key))
                                    innerComponents[entry.Key] = entry.Value;
                    }
                }
            }

            return innerComponents;
        }

        public static bool IsClockwise(this IEnumerable<DotsHalfEdge> halfEdges) =>
            halfEdges.Select(it =>
                (it.Destination.Coordinates.x - it.Origin.Coordinates.x) *
                (it.Destination.Coordinates.y + it.Origin.Coordinates.y)
            ).Sum() > 0;

        public static bool IsValidGM1(this List<DotsHalfEdge> halfEdges, bool debug = false)
        {
            if (halfEdges.Count <= 2)
            {
                if (debug) print(
                    $"face consisting of [{string.Join(", ", halfEdges)}] is not valid because halfEdges.count <= 2");
                return false;
            }

            if (halfEdges.All(he => halfEdges.Contains(he.Twin)))
            {
                if (debug) print($"face consisting of [{string.Join(", ", halfEdges)}] is not valid because it's a path");
                return false;
            }

            if (!halfEdges.IsClockwise())
            {
                if (debug) print(
                    $"face consisting of [{string.Join(", ", halfEdges)}] is not valid because it's not clockwise");
                return false;
            }

            return true;
        }

        public static bool IsValidGM2(this List<DotsHalfEdge> halfEdges,
            IEnumerable<DotsVertex> allVerticesNotInAFace, bool debug = false)
        {
            if (!halfEdges.IsValidGM1()) return false;

            List<Vector2> originCoordinates = halfEdges.Select(it => it.Origin.Coordinates).ToList();

            if (allVerticesNotInAFace.Any(v =>
                IsInside(originCoordinates, v.Coordinates)
            ))
            {
                if (debug) print(
                    $"face consisting of [{string.Join(", ", halfEdges)}] is not valid because it has a vertex inside");
                return false;
            }

            // Check whether face would be simple or not
            if (originCoordinates.Any(it => originCoordinates.Count(a => a == it) > 1))
            {
                if (debug) print($"face consisting of [{string.Join(", ", halfEdges)}] is not valid because it is not simple");
                return false;
            }

            return true;
        }

        // Will return null if no loop exists, else it will return the face and update all edges surrounding it
        public static DotsFace CreateFaceLoop(DotsHalfEdge halfEdge, GameMode gameMode,
            IEnumerable<DotsVertex> allVerticesNotInAFace)
        {
            DotsHalfEdge currentHalfEdge = halfEdge.Next;
            var visitedHalfEdges = new List<DotsHalfEdge>();
            var counter = 0;
            while (counter < 10000)
            {
                counter++;
                // print($"checking faceloop... Starting at [{halfEdge.Origin.Coordinates}, {halfEdge.Destination.Coordinates}] CurrentHalfEdge: [{currentHalfEdge.Origin.Coordinates}, {currentHalfEdge.Destination.Coordinates}]");
                if (halfEdge == currentHalfEdge)
                {
                    visitedHalfEdges.Add(currentHalfEdge);

                    if (gameMode == GameMode.GameMode1)
                    {
                        if (!visitedHalfEdges.IsValidGM1()) return null;
                    }
                    else
                    {
                        if (!visitedHalfEdges.IsValidGM2(allVerticesNotInAFace))
                            return null;
                    }

                    // print($"Face created! face consists of: {string.Join("\n", visitedHalfEdges.Select(it => $"[{it.Origin.Coordinates} -> {it.Destination.Coordinates}]"))}");

                    DotsFace face = new DotsFace();

                    // assuming the face is requested only on the last created edge
                    try
                    {
                        face.Constructor(outerComponent: halfEdge);
                    }
                    catch (Exception)
                    {
                        return null;
                    }

                    foreach (DotsHalfEdge edge in visitedHalfEdges)
                    {
                        edge.IncidentFace = face;
                    }

                    return face;
                }

                if (currentHalfEdge.Next != null)
                {
                    visitedHalfEdges.Add(currentHalfEdge);
                    currentHalfEdge = currentHalfEdge.Next;
                }
                else return null;
            }

            throw new Exception("Face too large or infinite path");
        }

        public static IEnumerable<DotsVertex> GetVerticesInConvexPosition(int amount, bool sameDistance,
            Vector2? center = null, float radius = 1f)
        {
            Vector2 _center = center ?? Vector2.zero;
            float angleBetweenVertices = 2f * Mathf.PI / amount;
            var vertices = new List<DotsVertex>();

            for (var i = 0; i < amount; i++)
            {
                float angle = i * angleBetweenVertices;
                float adjustmentAngle = Random.Range(-angleBetweenVertices / 3f, +angleBetweenVertices / 3f);
                if (!sameDistance) angle += adjustmentAngle;
                Vector2 pos = _center + new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
                vertices.Add(new DotsVertex(pos));
            }

            return vertices;
        }

        public static Tuple<DotsVertex, DotsVertex> FindAMiddleLine(
            IEnumerable<DotsVertex> vertices)
        {
            DotsVertex vertexA = vertices.First();

            DotsVertex vertexB = vertices.OrderBy(
                it => Mathf.Abs(Distance(vertexA.Coordinates, it.Coordinates))
            ).Last();

            return new Tuple<DotsVertex, DotsVertex>(vertexA, vertexB);
        }

        private const long CLIPPER_ACCURACY = 1000000000000000;
        public static long toLongForClipper(this float number) => Convert.ToInt64(number * CLIPPER_ACCURACY);
        public static float toFloatForClipper(this long number) => number / Convert.ToSingle(CLIPPER_ACCURACY);


        public static string ToString(this PolyNode polyNode, string indent = "", bool toFloat = true) =>
            $"{indent}Contour = ({string.Join(", ", polyNode.Contour.Select(it => $"({(toFloat ? it.X.toFloatForClipper() : it.X)},{(toFloat ? it.Y.toFloatForClipper() : it.Y)})"))})\n" +
            $"{indent}IsHole = {polyNode.IsHole}\n" +
            $"{indent}IsPolygon = {!polyNode.IsOpen}\n" +
            $"{indent}ChildCount = {polyNode.ChildCount}" +
            (polyNode.ChildCount > 0 ? "\n" : "") +
            string.Join(
                "\n",
                polyNode.Childs.Select((child, i) =>
                    $"{indent}Children[{i}]:\n" +
                    child.ToString(indent + "    ")
                ));

        public static string toString(this PolyTree polyTree) => ToString(polyTree.GetFirst());

        public static float GenerateRandomFloat(float bound1, float bound2)
        {
            var random = new System.Random();
            return bound1 < bound2
                ? (float) random.NextDouble() * (bound2 - bound1) + bound1
                : (float) random.NextDouble() * (bound1 - bound2) + bound2;
        }

        public static int GenerateRandomInt(int bound1, int bound2)
        {
            var random = new System.Random();
            return bound1 < bound2 ? random.Next(bound1, bound2) : random.Next(bound2, bound1);
        }

        public static long GenerateRandomLong(long bound1, long bound2)
        {
            if (bound1 == bound2) return bound1;

            var buf = new byte[8];
            new System.Random().NextBytes(buf);
            var longRand = BitConverter.ToInt64(buf, 0);

            long max = bound1 > bound2 ? bound1 : bound2;
            long min = bound1 > bound2 ? bound2 : bound1;

            return Math.Abs(longRand % (max - min)) + min;
        }

        public static T DrawRandomItem<T>(this IEnumerable<T> collection)
        {
            if (!collection.Any()) throw new Exception("Collection was empty");
            int randomPos = GenerateRandomInt(0, collection.Count());
            print($"Retrieving element at index {randomPos}");
            return collection.ElementAt(randomPos);
        }

        public static bool IsAbove(this LineSegment segment, LineSegment other) =>
            segment.Line.HeightAtYAxis > other.Line.HeightAtYAxis;

        public static float DiagonalLength(this Rect input) =>
            Mathf.Sqrt(Mathf.Pow(input.width, 2.0f) + Mathf.Pow(input.height, 2.0f));

        public static Path ToPathForClipper(this Rect rect) => new List<Vector2>
        {
            new Vector2(rect.xMin, rect.yMax),
            new Vector2(rect.xMax, rect.yMax),
            new Vector2(rect.xMax, rect.yMin),
            new Vector2(rect.xMin, rect.yMin)
        }.Select(coords =>
            new IntPoint(coords.x.toLongForClipper(), coords.y.toLongForClipper())
        ).ToList();

        public static PolyTree ToPolyTree(this Paths paths, Clipper clipper = null)
        {
            clipper = clipper ?? new Clipper();
            clipper.Clear();
            clipper.AddPaths(paths, PolyType.ptClip, true);
            clipper.AddPaths(paths, PolyType.ptSubject, true);
            var polyTree = new PolyTree();
            clipper.Execute(ClipType.ctUnion, polyTree, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
            return polyTree;
        }

        public static PolyTree ToPolyTree(this Path path, Clipper clipper = null)
        {
            clipper = clipper ?? new Clipper();
            clipper.Clear();
            clipper.AddPath(path, PolyType.ptClip, true);
            clipper.AddPath(path, PolyType.ptSubject, true);
            var polyTree = new PolyTree();
            clipper.Execute(ClipType.ctUnion, polyTree, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
            return polyTree;
        }

        public static void ForEach<T>(this IEnumerable<T> iEnumerable, Action<T> action)
        {
            foreach (T x in iEnumerable) action(x);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> iEnumerable ) 
        {
            HashSet<T> returner = new HashSet<T>();
            iEnumerable.ForEach(x => returner.Add(x));
            return returner;
        }

        public static bool EdgeIsPossible(DotsVertex p1, DotsVertex p2, IEnumerable<DotsEdge> edges,
            IEnumerable<DotsFace> faces)
        {
            if (p2 == null)
            {
                return false;
            }

            if (p1 == p2)
            {
                return false;
            }
            // use isInside method to see of middle of line lies in a face

            if (faces.Where(it => it?.OuterComponentHalfEdges != null).Any(face =>
                IsInside(
                    face.OuterComponentVertices.Select(it => it.Coordinates).ToList(),
                    new LineSegment(p1.Coordinates, p2.Coordinates).Midpoint
                )
            ))
            {
                return false;
            }

            if (EdgeAlreadyExists(edges, p1, p2))
            {
                return false;
            }

            if (InterSEGtsAny(
                new LineSegment(p1.Coordinates, p2.Coordinates),
                edges.Select(edge => edge.Segment)
            ))
            {
                return false;
            }

            return true;
        }
    }
}