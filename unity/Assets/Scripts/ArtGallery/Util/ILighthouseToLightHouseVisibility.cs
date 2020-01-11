using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Util.Geometry.Polygon;

namespace Assets.Scripts.ArtGallery.Util
{
    public interface ILighthouseToLightHouseVisibility
    {
        /// <summary>Checks if tho vertices are visible to each other</summary>
        /// <param name="vertex1"> The first vertex </param>
        /// <param name="vertex2"> The second vertex </param>
        /// <param name="polygon"> The polygon containing the vertices</param>
        bool VisibleToOtherVertex(
            Vector2 vertex1,
            Vector2 vertex2,
            Polygon2D polygon);

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
        bool VisibleToOtherVertex(
            Vector2 vertex,
            List<Vector2> otherVerteces,
            Polygon2D polygon);

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
        bool VisibleToOtherVertex(
            List<Vector2> vertexes,
            Polygon2D polygon);

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
        ICollection<Vector2> VisibleToOtherVertices(
            Vector2 vertex,
            List<Vector2> otherVertices,
            Polygon2D polygon);

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
        IDictionary<Vector2, ICollection<Vector2>>
            VisibleToOtherVertices(
                List<Vector2> vertices,
                Polygon2D polygon);
    }
}