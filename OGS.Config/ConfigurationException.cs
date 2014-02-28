using System;

namespace OGS.Config
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message, params object[] args)
            : base(string.Format(message, args))
        {
        }
    }
}