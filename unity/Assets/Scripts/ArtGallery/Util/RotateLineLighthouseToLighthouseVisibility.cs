﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using Assets.Scripts.ArtGallery.Util;
using UnityEngine;
using Util.Algorithms.Polygon;
using Util.Geometry;
using Util.Geometry.Polygon;
using Util.Math;

namespace ArtGallery
{
    public class
        RotateLineLighthouseToLighthouseVisibility :
            ILighthouseToLightHouseVisibility
    {
        /// <summary>Checks if tho vertices are visible to each other</summary>
        /// <param name="vertex1"> The first vertex </param>
        /// <param name="vertex2"> The second vertex </param>
        /// <param name="polygon"> The polygon containing the vertices</param>
        /// <returns>Whether the vertexes can see each other</returns>
        public bool VisibleToOtherVertex(
            Vector2 vertex1,
            Vector2 vertex2,
            Polygon2D polygon)
        {
            // find all visible vertices form vertex1
            var visibleVertices = VisibleVertices(polygon, vertex1);

            // check if vertex2 belongs to the set of visible vertices
            // if so, return tue, else return false.
            foreach (var vertex in visibleVertices)
            {
                if (vertex2.EqualsEps(vertex))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public bool VisibleToOtherVertex(
            Vector2 vertex,
            List<Vector2> otherVerteces,
            Polygon2D polygon)
        {
            //For each of the other vertices check if at least one is
            // visible. If so, return true else return false.
            foreach (Vector2 otherVertece in otherVerteces)
            {
                if (VisibleToOtherVertex(vertex, otherVertece, polygon))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        ///     Checks if the vertexes in
        ///     <paramref name="vertexes" />
        ///     can be seen by any of the vertexes in
        ///     <paramref name="vertexes" />
        ///     in the context of
        ///     <paramref name="polygon" />
        /// </summary>
        /// <param name="vertexes">
        ///     The collection of vertexes that need to be seen by at least
        ///     one other vertex in the same collection
        /// </param>
        /// <param name="polygon">The polygon in which the vertexes exist.</param>
        /// <returns>
        ///     Whether all of the vertexes in
        ///     <paramref name="vertexes" />
        ///     can seen by one other vertex in
        ///     <paramref name="vertexes" />
        /// </returns>
        public bool VisibleToOtherVertex(
            List<Vector2> vertexes,
            Polygon2D polygon)
        {
            // For each of the vertexes check if they are visible to at least
            // one other vertex
            foreach (Vector2 vertex1 in vertexes)
            {
                var otherVertexes = vertexes.Where(i => i != vertex1).ToList();

                // If the vertex cannot be seen by one other vertex return 
                if (!VisibleToOtherVertex(vertex1, otherVertexes, polygon))
                {
                    return false;
                }
            }

            // if every vertex can be seen by one other vertex return true
            return true;
        }

        /// <summary>
        ///     Creates a dictionary containing an entry for each vertex in
        ///     <paramref name="vertexes" />
        ///     and a corresponding value with all the vertexes in
        ///     <paramref name="vertexes" />
        ///     each vertex can see
        /// </summary>
        /// <param name="vertexes">
        ///     The collection of vertexes for which visibility needs to be
        ///     calculated
        /// </param>
        /// <param name="polygon">The polygon in which the vertexes exist.</param>
        /// <returns>
        ///     A dictionary containing for each vertex the other visible
        ///     vertexes
        /// </returns>
        public IDictionary<Vector2, ICollection<Vector2>>
            VisibleToOtherVertices(
                List<Vector2> vertexes,
                Polygon2D polygon)
        {
            // Create dictionary to store the result
            IDictionary<Vector2, ICollection<Vector2>> result =
                new Dictionary<Vector2, ICollection<Vector2>>();

            // Iterate over all the vertexes and calculate for each vertex the
            // other vertexes it can see.
            foreach (Vector2 vertex1 in vertexes)
            {
                // Select all vertexes except the current vertex 
                var otherVertexes = vertexes.Where(i => i != vertex1).ToList();

                // Calculate the visible vertexes 
                var visibleVertices = VisibleToOtherVertices(
                    vertex1,
                    otherVertexes,
                    polygon);

                // Create a dictionary item. The key is the current vertex and
                // the value is the vertexes it can see.
                var dicItem =
                    new KeyValuePair<Vector2, ICollection<Vector2>>(
                        vertex1,
                        visibleVertices);

                // Add the dictionary item to the dictionary
                result.Add(dicItem);
            }

            // Return the dictionary containing the vertex to vertex visibility
            return result;
        }

        /// <summary>
        ///     Checks if the vertex
        ///     <paramref name="vertex" />
        ///     can be seen by any of the vertexes in
        ///     <paramref name="otherVerteces" />
        ///     in the context of
        ///     <paramref name="polygon" />
        ///     and creates a collection of vertexes that can see
        ///     <paramref name="vertex" />
        /// </summary>
        /// <param name="vertex">
        ///     The vertex that needs to be seen by any of the vertexes in
        ///     <paramref name="otherVerteces" />
        /// </param>
        /// <param name="otherVerteces">
        ///     The vertexes that need to see
        ///     <paramref name="vertex" />
        /// </param>
        /// <param name="polygon">The polygon in which the vertexes exist.</param>
        /// <returns>
        ///     A collection of all vertexes in
        ///     <paramref name="otherVerteces" />
        ///     that can see the vertex
        ///     <paramref name="vertex" />
        /// </returns>
        public ICollection<Vector2> VisibleToOtherVertices(
            Vector2 vertex,
            List<Vector2> otherVerteces,
            Polygon2D polygon)
        {
            List<Vector2> result = new List<Vector2>();

            // check if the vertex can be seen by any of the other vertexes
            foreach (Vector2 vertex2 in otherVerteces)
            {
                // If the vertex can be seen by one other vertex add it to the 
                // list
                if (VisibleToOtherVertex(vertex, vertex2, polygon))
                {
                    result.Add(vertex2);
                }
            }

            // Return the list with all vertexes that can see the vertex
            return result;
        }

        public List<Vector2> VisibleVertices(
            Polygon2D polygon,
            Vector2 vertex)
        {
            List<Vector2> result = new List<Vector2>();

            // calculate new visibility polygon
            var vision = Visibility.Vision(polygon, vertex);

            if (vision == null)
            {
                throw new Exception("Vision polygon cannot be null");
            }

            // Check if the polygon is in clockwise order, if not, make 
            // the polygon in clockwise order
            if (!polygon.IsClockwise())
            {
                polygon.Reverse();
            }

            // Check if the vision polygon is in clockwise order, if not, make 
            // the vision polygon in clockwise order
            if (!vision.IsClockwise())
            {
                vision.Reverse();
            }

            List<Vector2> polyVertexes = polygon.Vertices.ToList();

            List<Vector2> visibilityVertexes = vision.Vertices.ToList();

            polyVertexes = polyVertexes.StartAt(vertex).ToList();
            visibilityVertexes = visibilityVertexes.StartAt(vertex).ToList();

            // move all vertexes such that the vertex vertex is at the origin
            // and transform them to PolarPoint2D

            var polyPolarPoints = polyVertexes
                                  .Select(v => v - vertex)
                                  .Select(x => new PolarPoint2D(x))
                                  .ToList();

            var visPolarPoints = visibilityVertexes
                                 .Select(v => v - vertex)
                                 .Select(x => new PolarPoint2D(x))
                                 .ToList();

            var initAngle = polyPolarPoints[1].Theta;

            // rotate all points of the shifted polygon clockwise such that v0 lies
            // on the x axis
            foreach (var curr in polyPolarPoints)
            {
                if (!curr.IsOrigin())
                {
                    curr.RotateClockWise(initAngle);
                }
            }

            foreach (var curr in visPolarPoints)
            {
                if (!curr.IsOrigin())
                {
                    curr.RotateClockWise(initAngle);
                }
            }

            bool done = false;
            int polyIndex = 0;
            int visIndex = 0;
            int polyCount = polyVertexes.Count;
            int visCount = visibilityVertexes.Count;

            while (!done)
            {
                var polyCurrent = polyVertexes[polyIndex];
                var visCurrent = visibilityVertexes[visIndex];
                var polyNext = polyVertexes[(polyIndex + 1) % polyCount];

                var polyLast =
                    polyVertexes[(polyIndex - 1 + polyCount) % polyCount];

                var visNext = visibilityVertexes[(visIndex + 1) % visCount];
                var polyLineSegment = new LineSegment(polyCurrent, polyNext);

                var polyLineSegmentLast =
                    new LineSegment(polyLast, polyCurrent);

                var visLineSegment = new LineSegment(visCurrent, visNext);

                if (polyCurrent == visCurrent)
                {
                    // the current poly vertex is a visibility vertex. 
                    // Add it to the list and increase the counter of polyIndex
                    result.Add(polyCurrent);
                    polyIndex++;
                }
                else if (visLineSegment.IsEndpoint(polyCurrent))
                {
                    //If the current poly vertex is equal to the next vis vertex
                    // We know that we can increase the vis index as there cannot
                    // be any more poly vertexes on the line between the current
                    // and the next vis vertex
                    // We do not add the current poly vertex to the list as 
                    // this will be done in the next iteration
                    visIndex++;
                }
                else if (visLineSegment.IsOnSegment(polyCurrent))
                {
                    // If the current poly vertex is on the line segment between
                    // the current and the next vis vertexes add it to the list
                    // and increase the counter of the polyIndex
                    result.Add(polyCurrent);
                    polyIndex++;
                }
                else if (polyLineSegment.IsOnSegment(visNext))
                {
                    // if the next vis vertex is on the poly line segment 
                    // increase the vis index. 
                    visIndex++;
                }
                else if (polyLineSegmentLast.IsOnSegment(visNext))
                {
                    // if the next vis vertex is on the previous poly
                    // lineSegment then no more points can lie on the 
                    // lineSegment of the current and next vis vertexes.
                    visIndex++;
                }
                else
                {
                    // Continue to increase the poly index until the current
                    // poly vertex intersects with the vis lineSegment again
                    polyIndex++;
                }

//                if (!(polyIndex < polyVertexes.Count) && !(visIndex < visibilityVertexes.Count))
//                {
//                    done = true;
//                }
                if (!(polyIndex < polyVertexes.Count))
                {
                    done = true;
                }

                if (!(visIndex < visibilityVertexes.Count))
                {
                    done = true;
                }
            }

            return result;
        }


        public List<List<Vector2>> VisibleVertices(
            Polygon2D polygon,
            List<Vector2> vertices)
        {
            List<List<Vector2>> result = new List<List<Vector2>>();

            foreach (Vector2 vertex in vertices)
            {
                result.Add(VisibleVertices(polygon, vertex));
            }

            return result;
        }
    }
}