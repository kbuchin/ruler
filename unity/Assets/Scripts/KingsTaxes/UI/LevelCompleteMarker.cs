namespace KingsTaxes
{
    using UnityEngine;
    using System.Collections;
    using UnityEngine.UI;

    /// <summary>
    /// Eanbles a image when m_keyname is satisfied otherwise disables the image
    /// </summary>
    public class LevelCompleteMarker : MonoBehaviour {

        public string m_keyName = "beat";
    
        // Use this for initialization
	    void Start () {
	        if ( 0 == PlayerPrefs.GetInt(m_keyName, 0))
            {
                GetComponent<Image>().enabled = false;
            }
	    }
	
	    // Update is called once per frame
	    void Update () {
	
	    }
    }
}
