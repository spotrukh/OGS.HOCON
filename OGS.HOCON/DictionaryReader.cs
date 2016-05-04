using System.Collections.Generic;
using System.Linq;

namespace OGS.HOCON
{
    public class DictionaryReaderNode
    {
    }

    public class DictionaryReader : Reader<DictionaryReaderNode>
    {
        public IDictionary<string, object> Source { get; private set; }

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
    }
}