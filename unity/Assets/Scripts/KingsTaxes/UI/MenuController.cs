using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KingsTaxes.UI
{
    class MenuController: MonoBehaviour
    {
        //Editable form unity
        public string m_MSTStart = "mststart";
        public string m_TSPStart = "tspstart";
        public string m_SpannerStart = "tspannerstart";
        public string m_TSPEndless = "tspendless";
        public string m_MSTEndless = "mstendless";
        public string m_spannerEndless = "tspannerendless";

        public void StartMST()
        {
            SceneManager.LoadScene(m_MSTStart);
        }

        public void StartTSP()
        {
            SceneManager.LoadScene(m_TSPStart);
        }

        public void StartSpanner()
        {
            SceneManager.LoadScene(m_SpannerStart);
        }

        public void StartMSTEndless()
        {
            SceneManager.LoadScene(m_MSTEndless);
        }

        public void StartTSPEndless()
        {
            SceneManager.LoadScene(m_TSPEndless);
        }

        public void StartSpannerEndless()
        {
            SceneManager.LoadScene(m_spannerEndless);
        }
    }
}
