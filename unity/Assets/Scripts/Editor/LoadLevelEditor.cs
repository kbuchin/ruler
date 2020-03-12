using ArtGallery;
using Divide;
using KingsTaxes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using Util.Math;
using Util.Geometry;
using ConvexHull;
using Util.Geometry.Polygon;
using General.Model;

[ScriptedImporter(1, "ipe")]
public class LoadLevelEditor : ScriptedImporter
{
    private readonly float agSIZE = 9f;
    private readonly float ktSIZE = 6f;
    private readonly float divSIZE = 5f;

    /// <summary>
    /// Defines a custom method for importing .ipe files into unity.
    /// Currently used for importing levels into 
    /// </summary>
    /// <param name="ctx"></param>
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var path = ctx.assetPath;
        var name = Path.GetFileNameWithoutExtension(path);

        var fileSelected = XElement.Load(path);

        // switch between which level to generate based on file name
        UnityEngine.Object obj;
        if (name.StartsWith("agLevel"))
        {
            obj = LoadArtGalleryLevel(fileSelected, name);
        }
        else if (name.StartsWith("ktLevel"))
        {
            obj = LoadKingsTaxesLevel(fileSelected, name);
        }
        else if (name.StartsWith("divLevel"))
        {
            obj = LoadDivideLevel(fileSelected, name);
        } 
        else if (name.StartsWith("hullLevel"))
        {
            obj = LoadHullLevel(fileSelected, name);
        }
        else
        {
            // no file name match
            EditorUtility.DisplayDialog("Error", "Level name not in an expected format", "OK");
            ctx.SetMainObject(null);
            return;
        }

        // add generated level as the main imported file
        ctx.AddObjectToAsset(name, obj);
        ctx.SetMainObject(obj);
    }

    public UnityEngine.Object LoadArtGalleryLevel(XElement fileSelected, string name)
    {
        // create the output scriptable object
        var asset = ScriptableObject.CreateInstance<ArtGalleryLevel>();

        // retrieve page data from .ipe file
        var items = fileSelected.Descendants("page").First().Descendants("path").ToList();

        // check that .ipe file contains one and only one polygon
        if (items.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No paths (lines/polygons) found in ipe file.", "OK");
            return asset;
        }

        var outerPoints = new List<Vector2>();
        var holes = new List<List<Vector2>>();
        var checkPoints = new List<Vector2>();

        foreach (var poly in items)
        {
            List<float> transformation = null;
            if (poly.Attribute("matrix") != null)
            {
                transformation = poly.Attribute("matrix").Value
                    .Split(' ')
                    .Select(s => float.Parse(s))
                    .ToList();
            }

            // retrieve coordinates from .ipe file
            var points = new List<Vector2>();
            foreach (var coordString in poly.Value.Split('\n'))
            {
                var coords = coordString.Split(' ').ToList();

                if (coords.Count < 2) continue;

                var x = float.Parse(coords[0]);
                var y = float.Parse(coords[1]);

                if (!MathUtil.IsFinite(x) || !MathUtil.IsFinite(y)) continue;

                if (transformation != null)
                {
                    // apply transformation matrix (could be made into library function)
                    x = transformation[0] * x + transformation[2] * y + transformation[4];
                    y = transformation[1] * x + transformation[3] * y + transformation[5];
                }

                points.Add(new Vector2(x, y));
            }

            if (outerPoints.Count == 0 || new Polygon2D(outerPoints).Area < new Polygon2D(points).Area)
            {
                if (outerPoints.Count > 0) holes.Add(outerPoints);
                outerPoints = points;
            }
            else
            {
                holes.Add(points);
            }

            // Add all defining vertices to checkPoints
        }


        // normalize coordinates
        var rect = BoundingBoxComputer.FromPoints(outerPoints);
        outerPoints = Normalize(rect, agSIZE, outerPoints);
        checkPoints.AddRange(outerPoints);
        for (var i = 0; i < holes.Count; i++)
        {
            holes[i] = Normalize(rect, agSIZE, holes[i]);
            checkPoints.AddRange(holes[i]);
        }



        // reverse if not clockwise
        if (!(new Polygon2D(outerPoints).IsClockwise()))
        {
            outerPoints.Reverse();
        }

        for (var i = 0; i < holes.Count; i++)
        {
            // reverse if not clockwise
            if (!(new Polygon2D(holes[i]).IsClockwise()))
            {
                holes[i].Reverse();
            }
        }

        var gridPoints = ComputeGridPoints(rect, outerPoints, holes, 50);

        checkPoints.AddRange(gridPoints);

        asset.Outer = outerPoints;
        asset.Holes = holes.Select(h => new Vector2Array(h.ToArray())).ToList();
        asset.CheckPoints = checkPoints;

        Debug.Log(asset.CheckPoints);

        // get level arguments
        var args = name.Split('_').ToList();
        if (args.Count > 2)
        {
            EditorUtility.DisplayDialog("Error", "Too many level arguments given in path name", "OK");
            return asset;
        }
        else if (args.Count == 2)
        {
            asset.MaxNumberOfLighthouses = int.Parse(args[1]);
        }

        return asset;
    }

    private UnityEngine.Object LoadKingsTaxesLevel(XElement fileSelected, string name)
    {
        // create the output scriptable object
        var asset = ScriptableObject.CreateInstance<KingsTaxesLevel>();

        // retrieve page data from .ipe file
        var items = fileSelected.Descendants("page").First().Descendants("use");

        // get marker data into respective vector list
        asset.Villages.AddRange(GetMarkers(items, "disk"));
        asset.Castles.AddRange(GetMarkers(items, "square"));

        // normalize coordinates
        var total = new List<Vector2>();
        total.AddRange(asset.Villages);
        total.AddRange(asset.Castles);
        var rect = BoundingBoxComputer.FromPoints(total);
        asset.Villages = Normalize(rect, ktSIZE, asset.Villages);
        asset.Castles = Normalize(rect, ktSIZE, asset.Castles);

        // give warning if no relevant data found
        if (asset.Villages.Count + asset.Castles.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "File does not contain any villages/castles (disks and/or squares).", "OK");
        }

        // get level arguments
        var args = name.Split('_').ToList();
        if (args.Count > 2)
        {
            foreach (var item in args) Debug.Log(item);
            EditorUtility.DisplayDialog("Error", "Too many level arguments given in path name.", "OK");
            return asset;
        }
        else if (args.Count == 2)
        {
            if (!float.TryParse(args[1], out asset.TSpannerRatio))
            {
                EditorUtility.DisplayDialog("Error", "Could not parse the t-spanner ratio.", "OK");
                return asset;
            }
        }

        return asset;
    }

    private UnityEngine.Object LoadDivideLevel(XElement fileSelected, string name)
    {
        // create the output scriptable object
        var asset = ScriptableObject.CreateInstance<DivideLevel>();

        // retrieve page data from .ipe file
        var items = fileSelected.Descendants("page").First().Descendants("use");

        // get marker data into respective vector list
        asset.Spearmen.AddRange(GetMarkers(items, "disk"));
        asset.Archers.AddRange(GetMarkers(items, "square"));
        asset.Mages.AddRange(GetMarkers(items, "cross"));

        // normalize coordinates
        var total = new List<Vector2>();
        total.AddRange(asset.Spearmen);
        total.AddRange(asset.Archers);
        total.AddRange(asset.Mages);
        var rect = BoundingBoxComputer.FromPoints(total);
        asset.Spearmen = Normalize(rect, divSIZE, asset.Spearmen);
        asset.Archers = Normalize(rect, divSIZE, asset.Archers);
        asset.Mages = Normalize(rect, divSIZE, asset.Mages);

        // give warning if no relevant data found
        if (asset.Spearmen.Count + asset.Archers.Count + asset.Mages.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "File does not contain any spearmen, archers, mages (disks, squares and/or crosses).", "OK");
        }

        // check for even number of points
        if (asset.Spearmen.Count % 2 != 0)
        {
            EditorUtility.DisplayDialog("Error", "File contains uneven number of spearmen (disks).", "OK");
            return asset;
        }
        if (asset.Archers.Count % 2 != 0)
        {
            EditorUtility.DisplayDialog("Error", "File contains uneven number of archers (squares).", "OK");
            return asset;
        }
        if (asset.Mages.Count % 2 != 0)
        {
            EditorUtility.DisplayDialog("Error", "File contains uneven number of mages (crosses).", "OK");
            return asset;
        }

        // get level arguments
        var args = name.Split('_').ToList();
        if (args.Count > 2)
        {
            EditorUtility.DisplayDialog("Error", "Too many level arguments given in path name.", "OK");
            return asset;
        }
        else if (args.Count == 2)
        {
            if (!int.TryParse(args[1], out asset.NumberOfSwaps))
            {
                EditorUtility.DisplayDialog("Error", "Could not parse level argument number of swaps.", "OK");
                return asset;
            }
        }

        return asset;
    }
    /// <summary>
    /// Loads a convex hull level.
    /// </summary>
    /// <param name="fileSelected"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private UnityEngine.Object LoadHullLevel(XElement fileSelected, string name)
    {
        // create the output scriptable object
        var asset = ScriptableObject.CreateInstance<HullLevel>();

        // retrieve page data from .ipe file
        var items = fileSelected.Descendants("page").First().Descendants("use");

        // get marker data into respective vector list
        asset.Points.AddRange(GetMarkers(items, "disk"));

        // normalize coordinates
        var rect = BoundingBoxComputer.FromPoints(asset.Points);
        asset.Points = Normalize(rect, ktSIZE, asset.Points);

        // give warning if no relevant data found
        if (asset.Points.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "File does not contain any villages/castles (disks and/or squares).", "OK");
        }

        return asset;
    }

    /// <summary>
    /// Retrieve a vector list for all markers elements with given name
    /// </summary>
    /// <param name="items"></param>
    /// <param name="markerName"></param>
    /// <returns>list of positions</returns>
    private List<Vector2> GetMarkers(IEnumerable<XElement> items, string markerName)
    {
        var result = new List<Vector2>();
        var markers = items.Where(x => x.Attribute("name").Value.Contains(markerName));

        foreach (var marker in markers)
        {
            // retrieve (x, y) position from pos attribute
            var x = float.Parse(marker.Attribute("pos").Value.Split(' ')[0]);
            var y = float.Parse(marker.Attribute("pos").Value.Split(' ')[1]);

            if (marker.Attribute("matrix") != null)
            {
                var transformation = marker.Attribute("matrix").Value
                    .Split(' ')
                    .Select(s => float.Parse(s))
                    .ToList();

                // apply transformation matrix (could be made into library function)
                x = transformation[0] * x + transformation[2] * y + transformation[4];
                y = transformation[1] * x + transformation[3] * y + transformation[5];
            }

            // add to result
            result.Add(new Vector2(x, y));
        }

        return result;
    }

    /// <summary>
    /// Computes a list of gridpoints indside the given polygon defined by outerpoints and holes.
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="outerPoints"></param>
    /// <param name="holes"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    private List<Vector2> ComputeGridPoints(Rect rect, List<Vector2> outerPoints, List<List<Vector2>> holes, int n)
    {
        var gridPoints = new List<Vector2>();

        Debug.Log(rect.xMin);
        Debug.Log(rect.xMax);
        Debug.Log(rect.yMin);
        Debug.Log(rect.yMax);
        Debug.Log(rect.width);
        Debug.Log(rect.height);

        for (float x = rect.xMin; x < rect.xMax; x += rect.width / (float)n)
        {
            for (float y = rect.yMin; y < rect.yMax; y += rect.height / (float)n)
            {
                gridPoints.Add(new Vector2(x, y));
            }
        }

        gridPoints = Normalize(rect, agSIZE, gridPoints);

        var tempPoly = new Polygon2DWithHoles(new Polygon2D(outerPoints), holes.Select(h => new Polygon2D(h)));

        for (int i = gridPoints.Count - 1; i >= 0; i--)
        {
            var point = gridPoints[i];
            if (!tempPoly.ContainsInside(point))
            {
                gridPoints.Remove(point);
            }
        }

        return gridPoints;
    }

    /// <summary>
    /// Normalizes the coordinate vector to fall within bounds specified by rect.
    /// Also adds random perturbations to create general positions.
    /// </summary>
    /// <param name="rect">Bounding box</param>
    /// <param name="coords"></param>
    private List<Vector2> Normalize(Rect rect, float SIZE, List<Vector2> coords)
    {
        var scale = SIZE / Mathf.Max(rect.width, rect.height);

        return coords
            .Select(p => new Vector2(
                (p[0] - (rect.xMin + rect.width / 2f)) * scale,
                (p[1] - (rect.yMin + rect.height / 2f)) * scale))
            .ToList();
    }
}