namespace Divide.UI
{
    using General.Drawing;
    using UnityEngine;

    class DivideLineDrawer : LineDrawer
    {
        public Color ArcherColor = new Color(102f/255,194f/255,165f/255);
        public Color SwordsmenColor = new Color(252f/255,141f/255,98f/255);
        public Color MageColor = new Color(141f/255,160f/255,203f/255);
        public Color AllColor = Color.black;

        private bool m_displayArcherLines;
        private bool m_displaySoldierLines;
        private bool m_displayMageLines;
        private bool m_displayAllLines;

        private DivideSolution m_solution;

        internal void ToggleArchers()
        {
            m_displayArcherLines = !m_displayArcherLines;
            UpdateLines();
        }

        internal void ToggleSoldier()
        {
            m_displaySoldierLines = !m_displaySoldierLines;
            UpdateLines();
        }

        internal void ToggleMages()
        {
            m_displayMageLines = !m_displayMageLines;
            UpdateLines();
        }

        internal void ToggleAll()
        {
            m_displayAllLines = !m_displayAllLines;
            UpdateLines();
        }

        internal void NewSolution(DivideSolution a_solution)
        {
            m_solution = a_solution;
            UpdateLines();
        }

        private void UpdateLines()
        {
            ClearLines();
            
            
            if (m_displayArcherLines)
            {
                AddLines(m_solution.Archers, ArcherColor);
            }
            if (m_displaySoldierLines)
            {
                AddLines(m_solution.Soldiers, SwordsmenColor);
            }
            if (m_displayMageLines)
            {
                AddLines(m_solution.Mages, MageColor); 
            }
            if (m_displayAllLines)
            {
                AddLines(m_solution.All, AllColor); 
            }
        }
    }
}
