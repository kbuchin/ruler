using System;

namespace Algo { 
    public class AlgoException : Exception
    {
        public AlgoException()
        {
        }

        public AlgoException(string a_message)
            : base(a_message)
        {
        }

        public AlgoException(string a_message, Exception a_inner)
            : base(a_message, a_inner)
        {
        }
    }
}