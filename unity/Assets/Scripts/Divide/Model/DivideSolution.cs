namespace Divide
{
    using System.Collections.Generic;
    using Util.Geometry;

    /// <summary>
    /// Data storage for Ham sandwich cuts that divide the three kinds of soldiers as well as all soldiers.
    /// Used for passing line information to line drawer.
    /// </summary>
    public struct DivideSolution
    {
        public IEnumerable<Line> Archers { get; private set; }
        public IEnumerable<Line> Spearmen { get; private set; }
        public IEnumerable<Line> Mages { get; private set; }
        public IEnumerable<Line> All { get; private set; }

        public DivideSolution(IEnumerable<Line> a_archers,
            IEnumerable<Line> a_spearmen, 
            IEnumerable<Line> a_mages,
            IEnumerable<Line> a_all)
        {
            Archers = a_archers;
            Spearmen = a_spearmen;
            Mages = a_mages;
            All = a_all;
        }
    }
}
