using ArtGallery;
using Divide;
using KingsTaxes;
using General.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using Util.Math;
using Util.Geometry;

[ScriptedImporter(1, "ipe")]
public class LoadLevelEditor : ScriptedImporter
{
    private readonly float agSIZE = 8f;
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
        Object obj;
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

    public Object LoadArtGalleryLevel(XElement fileSelected, string name)
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
        else if (items.Count > 1)
        {
            EditorUtility.DisplayDialog("Error", "File contains too many paths (lines/polygons).", "OK");
            return asset;
        }

        // retrieve coordinates from .ipe file
        var points = new List<Vector2>();
        foreach (var item in items[0].Value.Split('\n'))
        {
            var coords = item.Split(' ').ToList();

            if (coords.Count < 2) continue;

            var x = float.Parse(coords[0]);
            var y = float.Parse(coords[1]);

            if (!MathUtil.IsFinite(x) || !MathUtil.IsFinite(y)) continue;

            points.Add(new Vector2(x, y));
        }

        // normalize coordinates
        var rect = BoundingBoxComputer.FromPoints(points);
        Normalize(rect, agSIZE, ref points);

        // create relevant Vector2Array
        asset.Outer = new Vector2Array(points.ToArray());

        if (!asset.Polygon.IsClockwise())
        {
            points.Reverse();
            asset.Outer = new Vector2Array(points.ToArray());
        }

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

    private Object LoadKingsTaxesLevel(XElement fileSelected, string name)
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
        Normalize(rect, ktSIZE, ref asset.Villages);
        Normalize(rect, ktSIZE, ref asset.Castles);

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

    private Object LoadDivideLevel(XElement fileSelected, string name)
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
        Normalize(rect, divSIZE, ref asset.Spearmen);
        Normalize(rect, divSIZE, ref asset.Archers);
        Normalize(rect, divSIZE, ref asset.Mages);

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
    /// Normalizes the coordinate vector to fall within bounds specified by rect.
    /// Also adds random perturbations to create general positions.
    /// </summary>
    /// <param name="rect">Bounding box</param>
    /// <param name="coords"></param>
    private void Normalize(Rect rect, float SIZE, ref List<Vector2> coords)
    {
        var scale = SIZE / Mathf.Max(rect.width, rect.height);
        var rnd = 0.0001f; // for general positions

        coords = coords
            .Select(p => new Vector2(
                (p[0] - (rect.xMin + rect.width / 2f) + Random.Range(-rnd, rnd)) * scale,
                (p[1] - (rect.yMin + rect.height / 2f) + Random.Range(-rnd, rnd)) * scale))
            .ToList();
    }
}