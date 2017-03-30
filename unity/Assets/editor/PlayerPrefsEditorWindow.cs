using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

public class KeyEditorWindow : EditorWindow
{
    enum PlayerPrefsValueType
    {
        Unknown,
        String,
        Float,
        Int
    }

    // Search vars
    private string searchKey_m;
    private string searchNewVal_m;
    private PlayerPrefsValueType searchValType_m = PlayerPrefsValueType.String;
    private TrySetResponse searchResponse_m;

    // Add key vars
    private string addKey_m;
    private string addVal_m;
    private PlayerPrefsValueType addValType_m = PlayerPrefsValueType.String;
    private TrySetResponse addResponse_m;

    [MenuItem("PlayerPrefs/Open editor")]
    static void DeleteKey()
    {
        KeyEditorWindow editorWindow = (KeyEditorWindow)GetWindow(typeof(KeyEditorWindow), true, "Player prefs editor");
        editorWindow.Show();
    }

    void OnGUI()
    {
        DrawSearchKey();
        DrawAddKey();
    }

    /// <summary>
    /// Draw search for PlayerPrefs key controls
    /// </summary>
    private void DrawSearchKey()
    {
        GUILayout.Label("Search for key", EditorStyles.boldLabel);
        searchKey_m = EditorGUILayout.TextField("Key", searchKey_m);

        //Edit existing key
        if (PlayerPrefs.HasKey(searchKey_m))
        {

            PlayerPrefsValueType type = GetType(searchKey_m);
            
            // Delete
            if (GUILayout.Button("Delete"))
            {
                PlayerPrefs.DeleteKey(searchKey_m);
                Debug.Log("PlayerPrefs key: " + searchKey_m + ", deleted");
            }

            // Set value
            GUILayout.Label("Set new value", EditorStyles.boldLabel);
            searchNewVal_m = EditorGUILayout.TextField("New value", searchNewVal_m);
            if (type == PlayerPrefsValueType.Unknown)
            {
                searchValType_m = (PlayerPrefsValueType) EditorGUILayout.EnumPopup("Type", searchValType_m);
                EditorGUILayout.HelpBox("The value for the key is a default value so the type cannot be determined. It is your responsibility to set the value in correct type.", MessageType.Warning);
            }
            else
            {
                searchValType_m = type;
                GUILayout.Label("Value type: " + searchValType_m, EditorStyles.boldLabel);
                GUILayout.Label("Current value: " + GetValue(searchKey_m, searchValType_m), EditorStyles.boldLabel);
            }
            if (GUILayout.Button("Set"))
            {
                searchResponse_m = TrySetValue(searchKey_m, searchNewVal_m, searchValType_m);
            }
            if (searchResponse_m != null)
            {
                EditorGUILayout.HelpBox(searchResponse_m.message, searchResponse_m.messageType);
            }

            
        }
        else
        {
            EditorGUILayout.HelpBox("Key doesn't exist in player prefs", MessageType.Warning);
        }
    }

    /// <summary>
    /// Draw add key controls
    /// </summary>
    private void DrawAddKey()
    {
        GUILayout.Label("Add key", EditorStyles.boldLabel);
        addKey_m = EditorGUILayout.TextField("Key", addKey_m);
        addVal_m = EditorGUILayout.TextField("Value", addVal_m);
        addValType_m = (PlayerPrefsValueType)EditorGUILayout.EnumPopup("Type", addValType_m);
        if (GUILayout.Button("Add"))
        {
            addResponse_m = TrySetValue(addKey_m, addVal_m, addValType_m);
        }
        if (addResponse_m != null)
        {
            EditorGUILayout.HelpBox(addResponse_m.message, addResponse_m.messageType);
        }
    }

    /// <summary>
    /// Get type for a key that exists in PlayerPrefs. If the key's value is default value, the type cannot be determined.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static PlayerPrefsValueType GetType(string key)
    {
        if (!PlayerPrefs.HasKey(key)) throw new ArgumentException("Key didn't exist in PlayerPrefs");
        PlayerPrefsValueType type = PlayerPrefsValueType.Unknown;

        float floatVal = PlayerPrefs.GetFloat(key);
        int intVal = PlayerPrefs.GetInt(key);
        string stringVal = PlayerPrefs.GetString(key);

        if (floatVal == (default(float)) && intVal == (default(int)) && !stringVal.Equals(string.Empty))
        {
            type = PlayerPrefsValueType.String;
        }
        else if (floatVal == (default(float)) && intVal != (default(int)) && stringVal.Equals(string.Empty))
        {
            type = PlayerPrefsValueType.Int;
        }
        else if (floatVal != (default(float)) && intVal == (default(int)) && stringVal.Equals(string.Empty))
        {
            type = PlayerPrefsValueType.Float;
        }
        return type;
    }

    /// <summary>
    /// Tries to set the value to player prefs. If the value is successfully set, PlayerPrefs are saved.
    /// </summary>
    /// <param name="key">Key of value</param>
    /// <param name="value">Value for the key</param>
    /// <param name="type">Type of the value. This determines whether PlayerPrefs.SetString(), PlayerPrefs.SetFloat(), or PlayerPrefs.SetInt() is used.</param>
    /// <returns>Response containing info telling if the set was successful or not.</returns>
    private static TrySetResponse TrySetValue(string key, string value, PlayerPrefsValueType type)
    {
        TrySetResponse respone = new TrySetResponse()
        {
            message = "Key: " + key + " with Value: " + value + " was successfully saved to PlayerPrefs as a " + type,
            success = true,
            messageType = MessageType.Info
        };
        switch (type)
        {
            case PlayerPrefsValueType.String:
                PlayerPrefs.SetString(key, value);
                PlayerPrefs.Save();
                break;
            case PlayerPrefsValueType.Float:
                float newValFloat;
                if (float.TryParse(value, out newValFloat))
                {
                    PlayerPrefs.SetFloat(key, newValFloat);
                    PlayerPrefs.Save();
                }
                else
                {
                    respone.SetValues("Couldn't parse input value:" + value + " to target type float. Input a valid float value.", false, MessageType.Error);
                }
                break;
            case PlayerPrefsValueType.Int:
                int newValInt;
                if (int.TryParse(value, out newValInt))
                {
                    PlayerPrefs.SetInt(key, newValInt);
                    PlayerPrefs.Save();
                }
                else
                {
                    respone.SetValues("Couldn't parse input value:" + value + " to target type int. Input a valid int value.", false, MessageType.Error);
                }
                break;
            default:
                respone.SetValues("Unknown PlayerPrefsValueType: " + type, false, MessageType.Error);
                break;
        }
        return respone;
    }

    /// <summary>
    /// Get existing PlayerPrefs key value as a string.
    /// </summary>
    /// <param name="key">Key of the value</param>
    /// <param name="type">Type of the value</param>
    /// <returns>Value of the key as a string</returns>
    private string GetValue(string key, PlayerPrefsValueType type)
    {
        if (!PlayerPrefs.HasKey(key)) throw new ArgumentException("Key didn't exist in PlayerPrefs");
        switch (type)
        {
            case PlayerPrefsValueType.String:
                return PlayerPrefs.GetString(key);
            case PlayerPrefsValueType.Float:
                return PlayerPrefs.GetFloat(key).ToString(CultureInfo.InvariantCulture);
            case PlayerPrefsValueType.Int:
                return PlayerPrefs.GetInt(key).ToString(CultureInfo.InvariantCulture);
            default:
                throw new ArgumentOutOfRangeException("type");
        }
    }
    
    /// <summary>
    /// Helper class to return values from TrySetValue function
    /// </summary>
    class TrySetResponse
    {
        /// <summary>
        /// True if the value was successfully set, false otherwise.
        /// </summary>
        public bool success { get; set; }

        /// <summary>
        /// Message of the value set. May contain error message or success message.
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// Message type for showing the message in UI
        /// </summary>
        public MessageType messageType { get; set; }

        public void SetValues(string message, bool success, MessageType messageType)
        {
            this.message = message;
            this.success = success;
            this.messageType = messageType;
        }
    }
}