namespace OGS.Config
{
    using System;

    /// <summary>
    /// The configuration exception.
    /// </summary>
    public class ConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
        /// </summary>
        public ConfigurationException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
    }
}