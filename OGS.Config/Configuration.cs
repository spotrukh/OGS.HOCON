namespace OGS.Config
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using OGS.HOCON;

    /// <summary>
    /// The configuration.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration(ResolveConfigSourceHandler resolveConfigSource)
        {
            ConfigurationData = new Dictionary<string, object>();
            ResolveConfigSource += resolveConfigSource;
        }

        /// <summary>
        /// The resolve config source handler.
        /// </summary>
        public delegate string ResolveConfigSourceHandler(string configSource);

        /// <summary>
        /// The resolve config source.
        /// </summary>
        public event ResolveConfigSourceHandler ResolveConfigSource;

        /// <summary>
        /// Gets or sets the configuration data.
        /// </summary>
        protected Dictionary<string, object> ConfigurationData { get; set; }

        /// <summary>
        /// The read.
        /// </summary>
        public void Read(string configSource)
        {
            var reader = this.CreateReader();
            reader.Read(configSource);
        }

        /// <summary>
        /// The read from string.
        /// </summary>
        public void ReadFromString(string configContent)
        {
            var reader = this.CreateReader();
            reader.ReadFromString(configContent);
        }

        /// <summary>
        /// The read from stream.
        /// </summary>
        public void ReadFromStream(Stream configStream)
        {
            var reader = this.CreateReader();
            reader.ReadFromStream(configStream);
        }

        /// <summary>
        /// The has path.
        /// </summary>
        public bool HasPath(string path)
        {
            return ConfigurationData.ContainsKey(path);
        }

        /// <summary>
        /// The has value.
        /// </summary>
        public bool HasValue(string path)
        {
            object value;

            return ConfigurationData.TryGetValue(path, out value) && value != null && value is ConfigurationNode == false;
        }

        /// <summary>
        /// The get string.
        /// </summary>
        public string GetString(string path, string defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToValue<string>(value);
        }

        /// <summary>
        /// The get int.
        /// </summary>
        public int GetInt(string path, int defaultValue = 0)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToValue<int>(value);
        }

        /// <summary>
        /// The get decimal.
        /// </summary>
        public decimal GetDecimal(string path, decimal defaultValue = 0.0m)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToValue<decimal>(value);
        }

        /// <summary>
        /// The get bool.
        /// </summary>
        public bool GetBool(string path, bool defaultValue = false)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToValue<bool>(value);
        }

        /// <summary>
        /// The get value.
        /// </summary>
        public object GetValue(string path, object defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;
            
            return value;
        }

        /// <summary>
        /// The get value list.
        /// </summary>
        public List<object> GetValueList(string path, List<object> defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            var list = value as List<object>;
            if (list == null && value != null)
                throw new ConfigurationException("Invalid type, expected '{0}', but: '{1}'", typeof(List<object>), value.GetType());

            return list == null ? defaultValue : list.ToList();
        }

        /// <summary>
        /// The get string list.
        /// </summary>
        public List<string> GetStringList(string path, List<string> defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToList(value, defaultValue);
        }

        /// <summary>
        /// The get int list.
        /// </summary>
        public List<int> GetIntList(string path, List<int> defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToList(value, defaultValue);
        }

        /// <summary>
        /// The get decimal list.
        /// </summary>
        public List<decimal> GetDecimalList(string path, List<decimal> defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToList(value, defaultValue);
        }

        /// <summary>
        /// The get bool list.
        /// </summary>
        public List<bool> GetBoolList(string path, List<bool> defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToList(value, defaultValue);
        }

        /// <summary>
        /// The rise resolve config source.
        /// </summary>
        protected virtual string RiseResolveConfigSource(string configsource)
        {
            var handler = ResolveConfigSource;
            return (handler != null) ? handler(configsource) : string.Empty;
        }

        /// <summary>
        /// The cast to list.
        /// </summary>
        private List<T> CastToList<T>(object source, List<T> defaultValue)
        {
            var list = source as List<object>;
            if (list == null && source != null)
                throw new ConfigurationException("Invalid type, expected '{0}', but: '{1}'", typeof(List<T>), source.GetType());

            return (list == null) ? (defaultValue ?? new List<T>()) : list.Cast<T>().ToList();
        }

        /// <summary>
        /// The cast to value.
        /// </summary>
        private T CastToValue<T>(object source)
        {
            if (source is T == false)
                throw new ConfigurationException("Invalid type, expected '{0}', but: '{1}'", typeof(T), source.GetType());

            return (T)source;
        }

        /// <summary>
        /// The create reader.
        /// </summary>
        private Reader<ConfigurationNode> CreateReader()
        {
            var reader = new Reader<ConfigurationNode>();
            reader.CreateOrUpdateValue += (path, value) => ConfigurationData[path] = value;
            reader.CreateOrUpdateNode += (path, node) => ConfigurationData[path] = node;
            reader.RemoveNode += path => ConfigurationData.Remove(path);
            reader.GetNodeOrValue += path => ConfigurationData.ContainsKey(path) ? ConfigurationData[path] : null;
            reader.GetNodesOrValues += path => ConfigurationData.Where(item => path == item.Key || item.Key.StartsWith(path + ".")).ToArray();

            reader.ResolveSource += RiseResolveConfigSource;

            return reader;
        }

        /// <summary>
        /// The configuration node.
        /// </summary>
        private sealed class ConfigurationNode
        {
        }
    }
}
