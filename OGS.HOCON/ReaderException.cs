using System;

namespace OGS.HOCON
{
    public class ReaderException : Exception
    {
        public ReaderException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
    }
}