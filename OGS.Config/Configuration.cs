using System.IO;
using System.Linq;
using System.Collections.Generic;
using OGS.HOCON;

namespace OGS.Config
{
    public class Configuration
    {
        private sealed class ConfigurationNode
        {
        }

        protected Dictionary<string, object> ConfigurationData = new Dictionary<string, object>();

        public delegate string ResolveConfigSourceHandler(string configSource);
        public event ResolveConfigSourceHandler ResolveConfigSource;

        public Configuration(ResolveConfigSourceHandler resolveConfigSource)
        {
            ResolveConfigSource += resolveConfigSource;
        }

        public void Read(string configSource)
        {
            var reader = Createreader();
            reader.Read(configSource);
        }

        public void ReadFromString(string configContent)
        {
            var reader = Createreader();
            reader.ReadFromString(configContent);
        }

        public void ReadFromStream(Stream configStream)
        {
            var reader = Createreader();
            reader.ReadFromStream(configStream);
        }

        private Reader<ConfigurationNode> Createreader()
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

        public bool HasPath(string path)
        {
            return ConfigurationData.ContainsKey(path);
        }

        public bool HasValue(string path)
        {
            object value;

            return 
                ConfigurationData.TryGetValue(path, out value) && 
                value != null &&
                value as ConfigurationNode == null;
        }

        public string GetString(string path, string defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToValue<string>(value);
        }

        public int GetInt(string path, int defaultValue = 0)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToValue<int>(value);
        }

        public decimal GetDecimal(string path, decimal defaultValue = 0.0m)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToValue<decimal>(value);
        }

        public bool GetBool(string path, bool defaultValue = false)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToValue<bool>(value);
        }

        public object GetValue(string path, object defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;
            
            return value;
        }

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

        public List<string> GetStringList(string path, List<string> defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToList(value, defaultValue);
        }

        public List<int> GetIntList(string path, List<int> defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToList(value, defaultValue);
        }

        public List<decimal> GetDecimalList(string path, List<decimal> defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToList(value, defaultValue);
        }

        public List<bool> GetBoolList(string path, List<bool> defaultValue = null)
        {
            object value;
            if (ConfigurationData.TryGetValue(path, out value) == false)
                return defaultValue;

            return CastToList(value, defaultValue);
        }

        private List<T> CastToList<T>(object source, List<T> defaultValue)
        {
            var list = source as List<object>;
            if (list == null && source != null)
                throw new ConfigurationException("Invalid type, expected '{0}', but: '{1}'", typeof(List<T>), source.GetType());

            return (list == null) ? (defaultValue ?? new List<T>()) : list.Cast<T>().ToList();
        }

        private T CastToValue<T>(object source)
        {
            if (source is T == false)
                throw new ConfigurationException("Invalid type, expected '{0}', but: '{1}'", typeof(T), source.GetType());

            return (T)source;
        }

        protected virtual string RiseResolveConfigSource(string configsource)
        {
            var handler = ResolveConfigSource;
            return (handler != null) ? handler(configsource) : string.Empty;
        }
    }
}
