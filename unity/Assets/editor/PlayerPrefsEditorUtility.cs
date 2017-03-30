using UnityEngine;
using UnityEditor;


public class PlayerPrefsEditorUtility : MonoBehaviour
{

    [MenuItem("PlayerPrefs/Delete All")]
    static void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("All PlayerPrefs deleted");
    }

    [MenuItem("PlayerPrefs/Save All")]
    static void SavePlayerPrefs()
    {
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs saved");
    }
}
