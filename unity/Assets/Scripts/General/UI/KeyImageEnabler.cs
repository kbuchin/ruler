namespace General.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Enables a image when m_keyname is satisfied otherwise disables the image
    /// </summary>
    public class KeyImageEnabler : MonoBehaviour
    {
        // keyname of variable in Player Prefs
        public string m_keyName = "score_beat";

        // Use this for initialization
        void Awake()
        {
            if (PlayerPrefs.GetInt(m_keyName, 0) == 0)
            {
                GetComponent<Image>().enabled = false;
            }
        }
    }
}