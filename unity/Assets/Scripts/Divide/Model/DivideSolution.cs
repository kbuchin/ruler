namespace Divide {
    using System.Collections.Generic;
    using Util.Geometry;

    public struct DivideSolution
    {
        public ICollection<Line> Archers { get; private set; }
        public ICollection<Line> Soldiers { get; private set; }
        public ICollection<Line> Mages { get; private set; }
        public ICollection<Line> All { get; private set; }

        public DivideSolution(List<Line> a_archers, List<Line> a_soldiers, List<Line> a_mages, List<Line> a_all)
        {
            Archers = a_archers;
            Soldiers = a_soldiers;
            Mages = a_mages;
            All = a_all;
        }
    }
}
