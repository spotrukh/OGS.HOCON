namespace OGS.HOCON
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The writer.
    /// </summary>
    /// <typeparam name="TNode">
    /// </typeparam>
    public class Writer<TNode>
        where TNode : class, new()
    {
        /// <summary>
        /// The write stream.
        /// </summary>
        public void WriteStream(Stream stream, IEnumerable<KeyValuePair<string, object>> data, string headline = null)
        {
            var writter = new StreamWriter(stream);
            writter.Write(WriteString(data, headline));
            writter.Flush();
        }

        /// <summary>
        /// The write string.
        /// </summary>
        public string WriteString(IEnumerable<KeyValuePair<string, object>> data, string headline = null)
        {
            var builder = new StringBuilder();

            if (string.IsNullOrEmpty(headline) == false)
            {
                foreach (var line in headline.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    builder.Append("# ");
                    builder.AppendLine(line);
                }
            }

            var blocks = new Stack<string>();

            foreach (var entry in data.OrderBy(item => item.Key))
            {
                if (entry.Value is TNode)
                {
                    if (blocks.Count == 0)
                    {
                        builder.AppendLine();
                        builder.AppendFormat("{0} {{", entry.Key);
                        builder.AppendLine();

                        blocks.Push(entry.Key);
                    }
                    else if (entry.Key.StartsWith(blocks.Peek() + "."))
                    {
                        builder.AppendLine();
                        builder.AppendFormat(
                            "{0}{1} {{", 
                            new string('\t', blocks.Count),
                            entry.Key.Replace(blocks.Peek() + ".", string.Empty));
                        builder.AppendLine();

                        blocks.Push(entry.Key);
                    }
                    else
                    {
                        while (blocks.Count > 0)
                        {
                            blocks.Pop();

                            builder.AppendFormat("{0}}}", new string('\t', blocks.Count));
                            builder.AppendLine();
                        }

                        blocks.Push(entry.Key);

                        builder.AppendLine();
                        builder.AppendFormat("{0} {{", entry.Key);
                        builder.AppendLine();
                    }
                }
                else
                {
                    if (blocks.Count == 0 || entry.Key.StartsWith(blocks.Peek() + ".") == false)
                    {
                        while (blocks.Count > 0)
                        {
                            blocks.Pop();
                            builder.Append(new string('\t', blocks.Count));
                            builder.AppendLine("}");
                        }

                        builder.AppendLine();
                    }

                    builder.Append(new string('\t', blocks.Count));

                    builder.AppendFormat(
                        "{0} : ",
                        blocks.Count > 0 ? entry.Key.Replace(blocks.Peek() + ".", string.Empty) : entry.Key);

                    WriteValue(builder, entry.Value);

                    builder.AppendLine();
                }
            }

            while (blocks.Count > 0)
            {
                blocks.Pop();
                builder.Append(new string('\t', blocks.Count));
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        /// <summary>
        /// The write value.
        /// </summary>
        private void WriteValue(StringBuilder builder, object value)
        {
            var array = value as List<object>;
            if (array != null)
            {
                builder.Append("[");
                
                var count = array.Count;
                foreach (var item in array)
                {
                    WriteValue(builder, item);
                    if (--count > 0) builder.Append(", ");
                }

                builder.Append("]");
            }
            else if (value is string)
            {
                builder.AppendFormat("\"{0}\"", value);
            }
            else if (value is bool)
            {
                builder.Append((bool)value ? "true" : "false");
            }
            else if (value is decimal)
            {
                var numberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };
                builder.AppendFormat("{0}", ((decimal)value).ToString(numberFormat));
            }
            else if (value is double)
            {
                var numberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };
                builder.AppendFormat("{0}", ((double)value).ToString(numberFormat));
            }
            else
            {
                builder.AppendFormat("{0}", value);
            }
        }
    }
}
