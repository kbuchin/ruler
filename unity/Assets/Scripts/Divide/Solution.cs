using System.Collections.Generic;
using Algo;

namespace Divide {
    public class Solution
    {
        List<Line> m_archers;
        List<Line> m_soldiers;
        List<Line> m_mages;
        List<Line> m_all;

        public List<Line> Archers { get { return m_archers; } }
        public List<Line> Soldiers { get { return m_soldiers; } }
        public List<Line> Mages { get { return m_mages; } }
        public List<Line> All { get { return m_all; } }

        public Solution(List<Line> a_archers, List<Line> a_soldiers, List<Line> a_mages, List<Line> a_all)
        {
            m_archers = a_archers;
            m_soldiers = a_soldiers;
            m_mages = a_mages;
            m_all = a_all;
        }

    }
}
