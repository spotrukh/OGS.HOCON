namespace OGS.HOCON
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    using OGS.HOCON.Impl;

    /// <summary>
    /// The reader.
    /// </summary>
    public class Reader<TNode>
        where TNode : class, new()
    {
        /// <summary>
        /// The create or update value handler.
        /// </summary>
        public delegate void CreateOrUpdateValueHandler(string path, object value);

        /// <summary>
        /// The create or update node handler.
        /// </summary>
        public delegate void CreateOrUpdateNodeHandler(string path, TNode value);

        /// <summary>
        /// The remove node handler.
        /// </summary>
        public delegate void RemoveNodeHandler(string path);

        /// <summary>
        /// The get nodes or values handler.
        /// </summary>
        public delegate KeyValuePair<string, object>[] GetNodesOrValuesHandler(string startPath);

        /// <summary>
        /// The get node or value handler.
        /// </summary>
        public delegate object GetNodeOrValueHandler(string path);

        /// <summary>
        /// The resolve source handler.
        /// </summary>
        public delegate string ResolveSourceHandler(string source);

        /// <summary>
        /// The resolve source.
        /// </summary>
        public event ResolveSourceHandler ResolveSource;

        /// <summary>
        /// The create or update node.
        /// </summary>
        public event CreateOrUpdateNodeHandler CreateOrUpdateNode;

        /// <summary>
        /// The create or update value.
        /// </summary>
        public event CreateOrUpdateValueHandler CreateOrUpdateValue;

        /// <summary>
        /// The remove node.
        /// </summary>
        public event RemoveNodeHandler RemoveNode;

        /// <summary>
        /// The get nodes or values.
        /// </summary>
        public event GetNodesOrValuesHandler GetNodesOrValues;

        /// <summary>
        /// The get node or value.
        /// </summary>
        public event GetNodeOrValueHandler GetNodeOrValue;

        /// <summary>
        /// The read.
        /// </summary>
        public void Read(string sourcePath)
        {
            var startSource = WrapIncludeName(sourcePath);

            var tokenizer = new TokenProcessor(RiseResolveSource(sourcePath), TokenLibrary.Tokens);
            ReadKey(tokenizer, string.Empty, ref startSource);
        }

        /// <summary>
        /// The read from string.
        /// </summary>
        public void ReadFromString(string content)
        {
            var startSource = string.Empty;

            var tokenizer = new TokenProcessor(content, TokenLibrary.Tokens);
            ReadKey(tokenizer, string.Empty, ref startSource);
        }

        /// <summary>
        /// The read from stream.
        /// </summary>
        public void ReadFromStream(Stream stream)
        {
            ReadFromString((new StreamReader(stream)).ReadToEnd());
        }

        #region Event Helpers

        /// <summary>
        /// The rise get node or value.
        /// </summary>
        protected virtual object RiseGetNodeOrValue(string path)
        {
            var handler = GetNodeOrValue;
            return handler != null ? handler(path) : null;
        }

        /// <summary>
        /// The rise create or update value.
        /// </summary>
        protected virtual void RiseCreateOrUpdateValue(string path, object value)
        {
            var handler = CreateOrUpdateValue;
            if (handler != null) handler(path, value);
        }

        /// <summary>
        /// The rise create or update node.
        /// </summary>
        protected virtual void RiseCreateOrUpdateNode(string path, TNode value)
        {
            var handler = CreateOrUpdateNode;
            if (handler != null) handler(path, value);
        }

        /// <summary>
        /// The rise remove node.
        /// </summary>
        protected virtual void RiseRemoveNode(string path)
        {
            var handler = RemoveNode;
            if (handler != null) handler(path);
        }

        /// <summary>
        /// The rise get nodes or values.
        /// </summary>
        protected virtual KeyValuePair<string, object>[] RiseGetNodesOrValues(string startpath)
        {
            var handler = GetNodesOrValues;
            return handler != null ? handler(startpath) : new KeyValuePair<string, object>[0];
        }

        /// <summary>
        /// The rise resolve source.
        /// </summary>
        protected virtual string RiseResolveSource(string source)
        {
            var handler = ResolveSource;
            return handler != null ? handler(source) : string.Empty;
        }

        #endregion

        #region Reader Implementaion

        /// <summary>
        /// The wrap include name.
        /// </summary>
        private string WrapIncludeName(string path)
        {
            return string.Format("^{0}$", path);
        }

        /// <summary>
        /// The read include.
        /// </summary>
        private void ReadInclude(TokenProcessor tokenProcessor, string path, ref string alreadyIncluded)
        {
            var include = WrapIncludeName(path);

            if (alreadyIncluded.Contains(include))
                throw new ReaderException("Already included: {0}", path);

            alreadyIncluded += include;

            tokenProcessor.Include(RiseResolveSource(path));
        }

        /// <summary>
        /// The read key.
        /// </summary>
        private void ReadKey(TokenProcessor tokenProcessor, string originalPath, ref string alreadyIncluded)
        {
            TokenType token;
            string value;

            while (tokenProcessor.ReadNext(
                out token, out value, new[] { TokenType.Include, TokenType.Key }, new[] { TokenType.Comment, TokenType.Space }))
            {
                tokenProcessor.Consume();

                // Read include
                if (token == TokenType.Include)
                {
                    ReadInclude(tokenProcessor, value, ref alreadyIncluded);
                    continue;
                }
                
                // Update path
                var currentPath = originalPath;
                if (string.IsNullOrEmpty(currentPath) == false)
                    currentPath += ".";

                RiseCreateOrUpdateNode(currentPath += value, new TNode());

                // Read assign or scope
                if (tokenProcessor.ReadNext(
                    out token,
                    out value,
                    new[] { TokenType.BeginScope, TokenType.Assign },
                    new[] { TokenType.Comment, TokenType.Space }) == false)
                    throw new ReaderException("Expected assign or begin scope, but: {0}, offset: {1}", token, tokenProcessor.Offset);

                tokenProcessor.Consume();

                if (token == TokenType.Assign)
                {
                    var requestedTokens =
                        new[]
                        {
                            TokenType.StringValue, TokenType.NumericValue, TokenType.DeciamlValue,
                            TokenType.DoubleValue, TokenType.BooleanValue, TokenType.Substitution,
                            TokenType.SafeSubstitution, TokenType.BeginArray
                        };

                    if (tokenProcessor.ReadNext(
                        out token,
                        out value,
                        requestedTokens,
                        new[] { TokenType.Comment, TokenType.Space }) == false)
                        throw new ReaderException("Expected arra/string/numeric/bool/substitution, but: {0}, offset: {1}", token, tokenProcessor.Offset);

                    tokenProcessor.Consume();

                    // Read extends
                    object simpleValue;
                    if (token == TokenType.Substitution && RiseGetNodeOrValue(value) is TNode)
                    {
                        foreach (var item in RiseGetNodesOrValues(value))
                        {
                            var newPath = item.Key.Replace(value, currentPath);
                            var node = item.Value as TNode;

                            if (node != null)
                                RiseCreateOrUpdateNode(newPath, node);
                            else
                            {
                                RiseCreateOrUpdateValue(newPath, item.Value);
                            }
                        }

                        // Continue with scope
                        if (tokenProcessor.ReadNext(
                            out token,
                            out value,
                            new[] { TokenType.BeginScope },
                            new[] { TokenType.Comment, TokenType.Space }))
                        {
                            tokenProcessor.Consume();

                            ReadBeginScope(tokenProcessor, currentPath, ref alreadyIncluded);
                        }
                    }
                    else if (token == TokenType.BeginArray)
                        ReadArray(tokenProcessor, currentPath);
                    else if (ReadValue(token, value, out simpleValue))
                        RiseCreateOrUpdateValue(currentPath, simpleValue);
                    else
                        RiseRemoveNode(currentPath);

                }
                else if (token == TokenType.BeginScope)
                    ReadBeginScope(tokenProcessor, currentPath, ref alreadyIncluded);
            }
        }

        /// <summary>
        /// The read array.
        /// </summary>
        private void ReadArray(TokenProcessor tokenProcessor, string currentPath)
        {
            var array = new List<object>();
            TokenType token;
            string value;

            var requestedTokens =
                new[]
                {
                    TokenType.StringValue, TokenType.NumericValue, TokenType.DeciamlValue,
                    TokenType.DoubleValue, TokenType.BooleanValue, TokenType.Substitution,
                    TokenType.SafeSubstitution, TokenType.ArraySeparator, TokenType.EndArray
                };

            while (tokenProcessor.ReadNext(
                out token,
                out value,
                requestedTokens,
                new[] { TokenType.Comment, TokenType.Space }))
            {
                tokenProcessor.Consume();

                if (token == TokenType.EndArray)
                    break;
                
                if (token == TokenType.ArraySeparator)
                    continue;

                object arrayValue;
                if (ReadValue(token, value, out arrayValue))
                    array.Add(arrayValue);
            }

            RiseCreateOrUpdateValue(currentPath, array);
        }

        /// <summary>
        /// The read begin scope.
        /// </summary>
        private void ReadBeginScope(TokenProcessor tokenProcessor, string currentPath, ref string alreadyIncluded)
        {
            while (true)
            {
                TokenType token;
                string value;
              
                if (tokenProcessor.ReadNext(
                    out token,
                    out value,
                    new[] { TokenType.Key },
                    new[] { TokenType.Comment, TokenType.Space }))
                {
                    ReadKey(tokenProcessor, currentPath, ref alreadyIncluded);
                }
                else if (tokenProcessor.ReadNext(
                    out token, out value, new[] { TokenType.EndScope }, new[] { TokenType.Comment, TokenType.Space }))
                    {
                        tokenProcessor.Consume();
                        break;
                    }
                    else
                        throw new ReaderException("Expected begin end scope '}}' or a property, but: {0}, offset: {1}", token, tokenProcessor.Offset);
            }
        }

        /// <summary>
        /// The read value.
        /// </summary>
        private bool ReadValue(TokenType token, string content, out object value)
        {
            value = null;

            switch (token)
            {
                case TokenType.Substitution:
                    {
                        var data = RiseGetNodeOrValue(content);
                        if (data == null)
                            throw new ReaderException("Substitution not found: '{0}'", content);

                        value = data;
                    }

                    return true;

                case TokenType.SafeSubstitution:
                    {
                        var data = RiseGetNodeOrValue(content);
                        if (data == null) return false;

                        value = data;
                    }

                    return true;

                case TokenType.NumericValue:
                    value = int.Parse(content);
                    return true;

                case TokenType.DeciamlValue:
                    value = decimal.Parse(content, new NumberFormatInfo { CurrencyDecimalSeparator = "." });
                    return true;

                case TokenType.DoubleValue:
                    value = (decimal)double.Parse(content, new NumberFormatInfo { CurrencyDecimalSeparator = "." });
                    return true;

                case TokenType.BooleanValue:
                    value = content.Equals("on", StringComparison.InvariantCultureIgnoreCase) ||
                        content.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                        content.Equals("yes", StringComparison.InvariantCultureIgnoreCase) ||
                        content.Equals("enabled", StringComparison.InvariantCultureIgnoreCase);
                    return true;

                default:
                    value = content;
                    return true;
            }
        }
        
        #endregion
    }
}
