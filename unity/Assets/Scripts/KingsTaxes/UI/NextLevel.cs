namespace KingsTaxes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    class NextLevel : MonoBehaviour
    {
        public string m_nextLevel = "";

        public void Advance()
        {
            SceneManager.LoadScene(m_nextLevel);
        }
    }
}
