namespace KingsTaxes
{
    using UnityEngine;
    using System.Collections;
    using UnityEngine.UI;

    /// <summary>
    /// Enables a image when m_keyname is satisfied otherwise disables the image
    /// </summary>
    public class LevelCompleteMarker : MonoBehaviour {

        public string m_keyName = "beat";
    
        // Use this for initialization
	    void Start () {
	        if (PlayerPrefs.GetInt(m_keyName, 0) == 0)
            {
                GetComponent<Image>().enabled = false;
            }
	    }
    }
}
