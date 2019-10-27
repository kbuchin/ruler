namespace Util.Geometry
{
    using System;

    /// <summary>
    /// Generic catch-all class for exceptions related to geometry.
    /// Used for most exceptions thrown in this geometry library.
    /// </summary>
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