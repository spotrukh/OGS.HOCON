namespace OGS.HOCON
{
    using System;

    /// <summary>
    /// The reader exception.
    /// </summary>
    public class ReaderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReaderException"/> class.
        /// </summary>
        public ReaderException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
    }
}
