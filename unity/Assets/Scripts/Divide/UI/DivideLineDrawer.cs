namespace Divide
{
    using General.UI;
    using UnityEngine;

    class DivideLineDrawer : LineDrawer
    {
        // line colors for cut lines
        private Color ArcherColor = Color.red; //new Color(102f/255, 194f/255, 165f/255);
        private Color SpearmenColor = Color.green; //new Color(252f/255, 141f/255, 98f/255);
        private Color MageColor = Color.blue; //new Color(141f/255, 160f/255, 203f/255);
        private Color AllColor = Color.black;

        // Boolean for whether to display lines
        private bool m_displayArcherLines;
        private bool m_displaySpearmenLines;
        private bool m_displayMageLines;
        private bool m_displayAllLines;

        // solution storing the cut lines
        private DivideSolution m_solution;

        /// <summary>
        /// Update the solution to be drawn
        /// </summary>
        public DivideSolution Solution
        {
            get { return m_solution; }
            set { m_solution = value; UpdateLines(); }
        }

        /// <summary>
        /// Toggles the drawing of the archer cut lines
        /// </summary>
        public void ToggleArchers()
        {
            m_displayArcherLines = !m_displayArcherLines;
            UpdateLines();
        }

        /// <summary>
        /// Toggles the drawing of the spearmen cut lines
        /// </summary>
        public void ToggleSpearmen()
        {
            m_displaySpearmenLines = !m_displaySpearmenLines;
            UpdateLines();
        }

        /// <summary>
        /// Toggles the drawing of the mage cut lines
        /// </summary>
        public void ToggleMages()
        {
            m_displayMageLines = !m_displayMageLines;
            UpdateLines();
        }

        /// <summary>
        /// Toggles displaying the final cut line
        /// </summary>
        public void ToggleAll()
        {
            m_displayAllLines = !m_displayAllLines;
            UpdateLines();
        }

        /// <summary>
        /// Updates the line drawing given the divide solution cut lines and toggles.
        /// </summary>
        private void UpdateLines()
        {
            ClearLines();

            // draw lines that are enabled by adding lines to LineDrawer
            if (m_displayArcherLines)
            {
                AddLines(m_solution.Archers, ArcherColor);
            }
            if (m_displaySpearmenLines)
            {
                AddLines(m_solution.Spearmen, SpearmenColor);
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
