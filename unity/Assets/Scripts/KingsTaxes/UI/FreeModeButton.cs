using UnityEngine;
using UnityEngine.UI;

namespace KingsTaxes.UI
{
    class FreeModeButton:MonoBehaviour
    {
        public string m_beatkey = "beat";
        public string m_numberOfLevelsKey = "free";

        void Start()
        {
            if (0 == PlayerPrefs.GetInt(m_beatkey, 0))
            {
                Debug.Log("Disabling");
                gameObject.SetActive(false);
            } else
            {
                var score = PlayerPrefs.GetInt(m_numberOfLevelsKey, 0);
                this.transform.FindChild("Panel").FindChild("Text").GetComponent<Text>().text = score.ToString();
            }
        }
    }
}
