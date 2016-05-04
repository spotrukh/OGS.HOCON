namespace OGS.HOCON
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// DictionaryReader class
    /// </summary>
    public class DictionaryReader : Reader<DictionaryReaderNode>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryReader"/> class.
        /// </summary>
        /// <param name="resolveSource">
        /// The resolve source.
        /// </param>
        public DictionaryReader(ResolveSourceHandler resolveSource)
        {
            Source = new Dictionary<string, object>();

            ResolveSource += resolveSource;

            CreateOrUpdateValue += (path, value) => Source[path] = value;
            CreateOrUpdateNode += (path, node) => Source[path] = node;
            RemoveNode += path => Source.Remove(path);
            GetNodeOrValue += path => Source.ContainsKey(path) ? Source[path] : null;
            GetNodesOrValues += path => Source.Where(item => path == item.Key || item.Key.StartsWith(path + ".")).ToArray();
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        public IDictionary<string, object> Source { get; private set; }
    }
}