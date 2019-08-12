namespace KingsTaxes
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    class VictoryController : MonoBehaviour
    {
        /// <summary>
        /// Where to save the victory
        /// </summary>
        public string m_victorykey = "beat";

        void Start()
        {
            PlayerPrefs.SetInt(m_victorykey, 1);
        }
    }
}
