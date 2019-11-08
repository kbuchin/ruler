namespace Divide
{
    using System.Collections.Generic;
    using System.Linq;
    using Util.Geometry;

    /// <summary>
    /// Data storage for Ham sandwich cuts that divide the three kinds of soldiers as well as all soldiers.
    /// Used for passing line information to line drawer.
    /// </summary>
    public struct DivideSolution
    {
        public List<Line> Archers { get; private set; }
        public List<Line> Spearmen { get; private set; }
        public List<Line> Mages { get; private set; }
        public List<Line> All { get; private set; }

        public DivideSolution(IEnumerable<Line> a_archers,
            IEnumerable<Line> a_spearmen,
            IEnumerable<Line> a_mages,
            IEnumerable<Line> a_all)
        {
            Archers = a_archers.ToList();
            Spearmen = a_spearmen.ToList();
            Mages = a_mages.ToList();
            All = a_all.ToList();
        }
    }
}
