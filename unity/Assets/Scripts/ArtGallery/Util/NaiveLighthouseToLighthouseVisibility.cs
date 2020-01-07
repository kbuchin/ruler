using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util.Algorithms.Polygon;
using Util.Geometry;
using Util.Geometry.Polygon;

namespace ArtGallery
{
    public static class NaiveLighthouseToLighthouseVisibility
    {
       /// <summary>Checks if tho vertices are visible to each other</summary>
        /// <param name="vertex1"> The first vertex </param>
        /// <param name="vertex2"> The second vertex </param>
        /// <param name="polygon"> The polygon containing the vertices</param>
        /// <returns>Whether the vertexes can see each other</returns>
        public static bool VisibleToOtherVertex(
            Vector2 vertex1,
            Vector2 vertex2,
            Polygon2D polygon)
        {
            LineSegment seg1 = new LineSegment(vertex1, vertex2);

            foreach (LineSegment lineSegment in polygon.Segments)
            {
                // Check if any of the line segments intersect with the
                // line segments between the two vertices. If they so then 
                // the two vertices cannot see each other. With the exception
                // that an intersection with an endpoint does not count as 
                // blocking visibility
                Vector2? intersection = seg1.IntersectProper(lineSegment);

                if (intersection != null)
                {
                    // Check if the two lines are parallel. We allow two vertexes
                    // to see each other if they they are along a straight line
                    if (!seg1.IsParallel(lineSegment))
                    {
                        return false;
                    }
                }
            }

            // Non of the line segments of the polygon intersect with the 
            // visibility line between the two points. Next we need to check if 
            // the visibility line is inside of the polygon
            // We take the midpoint of the line. If this is inside the polygon 
            // then the two points can see each other
            return polygon.ContainsInside(seg1.Midpoint);
        }

        /// <summary>
        ///     Checks if the vertex
        ///     <paramref name="vertex" />
        ///     can be seen by any of the vertexes in
        ///     <paramref name="otherVerteces" />
        ///     in the context of
        ///     <paramref name="polygon" />
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
        ///     Whether one of the vertexes in
        ///     <paramref name="otherVerteces" />
        ///     can seen
        ///     <paramref name="vertex" />
        /// </returns>
        public static bool VisibleToOtherVertex(
            Vector2 vertex,
            List<Vector2> otherVerteces,
            Polygon2D polygon)
        {
            // Calculate all the visible vertexes
            int numberOfVisibleVertexes =
                VisibleToOtherVertices(vertex, otherVerteces, polygon)
                    .Count;

            // Check if there is at least 1 vertex visible
            return numberOfVisibleVertexes > 0;
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
        public static ICollection<Vector2> VisibleToOtherVertices(
            Vector2 vertex,
            List<Vector2> otherVertices,
            Polygon2D polygon)
        {
            List<Vector2> result = new List<Vector2>();

            // check if the vertex can be seen by any of the other vertexes
            foreach (Vector2 vertex2 in otherVertices)
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
        public static bool VisibleToOtherVertex(
            List<Vector2> vertexes,
            Polygon2D polygon)
        {
            // For each of the vertexes check if they are visible to at least
            // one other vertex
            foreach (Vector2 vertex in vertexes)
            {
                var otherVertexes = vertexes.Where(i => i != vertex).ToList();

                // If the vertex cannot be seen by one other vertex return 
                if (!VisibleToOtherVertex(vertex, otherVertexes, polygon))
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
        public static IDictionary<Vector2, ICollection<Vector2>>
            VisibleToOtherVertices(
                List<Vector2> vertices,
                Polygon2D polygon)
        {
            // Create dictionary to store the result
            IDictionary<Vector2, ICollection<Vector2>> result =
                new Dictionary<Vector2, ICollection<Vector2>>();

            // Iterate over all the vertexes and calculate for each vertex the
            // other vertexes it can see.
            foreach (Vector2 vertex in vertices)
            {
                // Select all vertexes except the current vertex 
                var otherVertexes = vertices.Where(i => i != vertex).ToList();

                // Calculate the visible vertexes 
                var visibleVertices = VisibleToOtherVertices(
                    vertex,
                    otherVertexes,
                    polygon);

                // Add a dictionary item. The key is the current vertex and
                // the value is the vertexes it can see.
                result.Add(vertex, visibleVertices);
            }

            // Return the dictionary containing the vertex to vertex visibility
            return result;
        }
    }
}
