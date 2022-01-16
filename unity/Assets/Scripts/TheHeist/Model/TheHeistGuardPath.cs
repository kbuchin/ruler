using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheHeistGuardPath : MonoBehaviour {

    // 1    up
    // 2    left
    // 3    down
    // 4    right

    private Dictionary<int, Dictionary<int, int[]>> data;

	// Use this for initialization
	void Start () {

        data = new Dictionary<int, Dictionary<int, int[]>>();

        // Level 1
        data.Add(1, new Dictionary<int, int[]>()
        {
            {0, new int[] { 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 1 }}
        });

        // Level 2
        data.Add(2, new Dictionary<int, int[]>()
        {
            {0, new int[] { 2, 2, 2, 2, 2, 3, 3, 2, 2, 2, 1, 1, 2, 2, 2, 3, 3, 3, 3, 3 }},
            {1, new int[] { 1, 1, 1, 1, 1, 1, 4, 4, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 2, 2, 2, 1, 1, 2, 2, 2, 3, 3, 3, 3, 3 }}
        });

        // Level 3
        data.Add(3, new Dictionary<int, int[]>()
        {
            {0, new int[] { 2, 2, 2, 2, 2, 2, 2, 2 }},
            { 1, new int[] { 3, 3, 3, 3, 3, 3 }},
            {2, new int[] { 4, 4, 4, 4, 4, 4 }}
        });
    }

    public int[] GetPath(int level, int guard)
    {
        if (data.ContainsKey(level))
        {
            var levelData = data[level];
            if (levelData.ContainsKey(guard)) return levelData[guard];
        }

        // else, return empty path
        return new int[0];
    } 
}
