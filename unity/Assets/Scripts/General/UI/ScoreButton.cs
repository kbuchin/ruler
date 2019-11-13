namespace General.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Enable/disables a panel depending if a game has been beaten (given by beat key in Player Prefs).
    /// If enabled, sets a number in the panel to the number of levels beaten, given by another key.
    /// </summary>
    public class ScoreButton : MonoBehaviour
    {
        // key in Player Prefs which stores if a game has been beat
        public string m_beatkey = "score_beat";

        // key in Player Prefs storing the amount of levels beaten
        public string m_numberOfLevelsKey = "levels_score";

        // Use this for initialization
        void Awake()
        {
            if (0 == PlayerPrefs.GetInt(m_beatkey, 0))
            {
                // disable panel
                gameObject.SetActive(false);
            }
            else
            {
                // enable panel and set text to the score
                gameObject.SetActive(true);
                var score = PlayerPrefs.GetInt(m_numberOfLevelsKey, 0);
                transform.Find("Panel").Find("Text").GetComponent<Text>().text = score.ToString();
            }
        }
    }
}