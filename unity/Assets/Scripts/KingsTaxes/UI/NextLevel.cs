using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace KingsTaxes.UI
{
    class NextLevel : MonoBehaviour
    {
        public string m_nextLevel = "";

        public void Advance()
        {
            SceneManager.LoadScene(m_nextLevel);
        }
    }
}
