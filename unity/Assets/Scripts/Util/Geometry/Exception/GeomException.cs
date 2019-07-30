namespace Util.Geometry
{
    using System;

    public class GeomException : Exception
    {
        public GeomException()
        { }

        public GeomException(string a_message) : base(a_message)
        { }

        public GeomException(string a_message, Exception a_inner) : base(a_message, a_inner)
        { }
    }
}