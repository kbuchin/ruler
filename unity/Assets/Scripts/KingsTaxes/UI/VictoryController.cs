using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingsTaxes.UI
{
    class VictoryController : MonoBehaviour
    {
        /// <summary>
        /// Where to save the victory
        /// </summary>
        public string m_victorykey = "beat";
        /// <summary>
        /// The next scene we will advance to
        /// </summary>
        public string m_nextScene = "mainMenu";

        void Start()
        {
            PlayerPrefs.SetInt(m_victorykey, 1);
        }

        /// <summary>
        /// Advances to the main menu
        /// </summary>
        public void Advance()
        {
            SceneManager.LoadScene(m_nextScene);
        }
    }
}
