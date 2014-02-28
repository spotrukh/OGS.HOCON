using System;
using System.IO;
using System.Collections.Generic;
using OGS.HOCON.Impl;

namespace OGS.HOCON
{
    public class Reader<TNode>
        where TNode : class, new()
    {
        public delegate void CreateOrUpdateValueHandler(string path, object value);
        public delegate void CreateOrUpdateNodeHandler(string path, TNode value);
        public delegate void RemoveNodeHandler(string path);
        public delegate KeyValuePair<string, object>[] GetNodesOrValuesHandler(string startPath);
        public delegate object GetNodeOrValueHandler(string path);
        public delegate string ResolveSourceHandler(string source);

        public event ResolveSourceHandler ResolveSource;
        public event CreateOrUpdateNodeHandler CreateOrUpdateNode;
        public event CreateOrUpdateValueHandler CreateOrUpdateValue;
        public event RemoveNodeHandler RemoveNode;

        public event GetNodesOrValuesHandler GetNodesOrValues;
        public event GetNodeOrValueHandler GetNodeOrValue;

        public void Read(string sourcePath)
        {
            var startSource = WrapIncludeName(sourcePath);

            var tokenizer = new Tokenizer(RiseResolveSource(sourcePath), TokenLibrary.Tokens);
            ReadKey(tokenizer, string.Empty, ref startSource);
        }

        public void ReadFromString(string content)
        {
            var startSource = string.Empty;

            var tokenizer = new Tokenizer(content, TokenLibrary.Tokens);
            ReadKey(tokenizer, string.Empty, ref startSource);
        }

        public void ReadFromStream(Stream stream)
        {
            ReadFromString((new StreamReader(stream)).ReadToEnd());
        }

        #region Event Helpers

        protected virtual object RiseGetNodeOrValue(string path)
        {
            var handler = GetNodeOrValue;
            return handler != null ? handler(path) : null;
        }

        protected virtual void RiseCreateOrUpdateValue(string path, object value)
        {
            var handler = CreateOrUpdateValue;
            if (handler != null) handler(path, value);
        }
        
        protected virtual void RiseCreateOrUpdateNode(string path, TNode value)
        {
            var handler = CreateOrUpdateNode;
            if (handler != null) handler(path, value);
        }

        protected virtual void RiseRemoveNode(string path)
        {
            var handler = RemoveNode;
            if (handler != null) handler(path);
        }

        protected virtual KeyValuePair<string, object>[] RiseGetNodesOrValues(string startpath)
        {
            var handler = GetNodesOrValues;
            return handler != null ? handler(startpath) : new KeyValuePair<string, object>[0];
        }

        protected virtual string RiseResolveSource(string source)
        {
            var handler = ResolveSource;
            return handler != null ? handler(source) : string.Empty;
        }

        #endregion

        #region Reader Implementaion
        
        private string WrapIncludeName(string path)
        {
            return string.Format("^{0}$", path);
        }

        private void ReadInclude(Tokenizer tokenizer, string path, ref string alreadyIncluded)
        {
            var include = WrapIncludeName(path);

            if (alreadyIncluded.Contains(include))
                throw new ReaderException("Already included: {0}", path);

            alreadyIncluded += include;

            tokenizer.Include(RiseResolveSource(path));
        }

        private void ReadKey(Tokenizer tokenizer, string originalPath, ref string alreadyIncluded)
        {
            TokenType token;
            string value;

            while (tokenizer.ReadNext(out token, out value,
                                      new[] { TokenType.Include, TokenType.Key },
                                      new[] { TokenType.Comment, TokenType.Space }))
            {
                tokenizer.Consume();

                // Read include
                if (token == TokenType.Include)
                {
                    ReadInclude(tokenizer, value, ref alreadyIncluded);
                    continue;
                }


                // Update path
                var currentPath = originalPath;
                if (string.IsNullOrEmpty(currentPath) == false)
                    currentPath += ".";

                RiseCreateOrUpdateNode((currentPath += value), new TNode());
                
                // Read assign or scope
                if (tokenizer.ReadNext(out token, out value,
                    new[] { TokenType.BeginScope, TokenType.Assign },
                    new[] { TokenType.Comment, TokenType.Space }) == false)
                    throw new ReaderException("Expected assign or begin scope, but: {0}, offset: {1}", token, tokenizer.Offset);

                tokenizer.Consume();

                if (token == TokenType.Assign)
                {
                    if (tokenizer.ReadNext(out token, out value,
                        new[]
                            {
                                TokenType.StringValue, 
                                TokenType.NumericValue, 
                                TokenType.DeciamlValue, 
                                TokenType.DoubleValue,
                                TokenType.BooleanValue, 
                                TokenType.Substitution,
                                TokenType.SafeSubstitution,
                                TokenType.BeginArray
                            },
                        new[] { TokenType.Comment, TokenType.Space }) == false)
                        throw new ReaderException("Expected arra/string/numeric/bool/substitution, but: {0}, offset: {1}", token, tokenizer.Offset);

                    tokenizer.Consume();

                    // Read extends
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
                        if (tokenizer.ReadNext(out token, out value,
                                               new[] { TokenType.BeginScope },
                                               new[] { TokenType.Comment, TokenType.Space }))
                        {
                            tokenizer.Consume();

                            ReadBeginScope(tokenizer, currentPath, ref alreadyIncluded);
                        }
                    }
                    else if (token == TokenType.BeginArray)
                        ReadAray(tokenizer, currentPath);
                    else
                    {
                        object simpleValue;
                        if (ReadValue(token, value, out simpleValue))
                            RiseCreateOrUpdateValue(currentPath, simpleValue);
                        else
                            RiseRemoveNode(currentPath);
                    }
                }
                else if (token == TokenType.BeginScope)
                    ReadBeginScope(tokenizer, currentPath, ref alreadyIncluded);
            }
        }

        private void ReadAray(Tokenizer tokenizer, string currentPath)
        {
            var array = new List<object>();
            TokenType token;
            string value;

            while (tokenizer.ReadNext(out token, out value,
                new[]
                    {
                        TokenType.StringValue,
                        TokenType.NumericValue,
                        TokenType.DeciamlValue,
                        TokenType.DoubleValue,
                        TokenType.BooleanValue,
                        TokenType.Substitution,
                        TokenType.SafeSubstitution,
                        TokenType.ArraySeparator,
                        TokenType.EndArray
                    },
                new[] { TokenType.Comment, TokenType.Space }))
            {
                tokenizer.Consume();

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

        private void ReadBeginScope(Tokenizer tokenizer, string currentPath, ref string alreadyIncluded)
        {
            while (true)
            {
                TokenType token;
                string value;
              
                if (tokenizer.ReadNext(out token, out value,
                                       new[] { TokenType.Key },
                                       new[] { TokenType.Comment, TokenType.Space }))
                {
                    ReadKey(tokenizer, currentPath, ref alreadyIncluded);
                }
                else
                    if (tokenizer.ReadNext(out token, out value,
                                           new[] { TokenType.EndScope },
                                           new[] { TokenType.Comment, TokenType.Space }))
                    {
                        tokenizer.Consume();
                        break;
                    }
                    else
                        throw new ReaderException("Expected begin end scope '}}' or a property, but: {0}, offset: {1}", token, tokenizer.Offset);
            }
        }

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
                    value = decimal.Parse(content);
                    return true;

                case TokenType.DoubleValue:
                    value = (decimal)double.Parse(content);
                    return true;

                case TokenType.BooleanValue:
                    value = 
                        (
                            content.Equals("on", StringComparison.InvariantCultureIgnoreCase) ||
                            content.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                            content.Equals("yes", StringComparison.InvariantCultureIgnoreCase) ||
                            content.Equals("enabled", StringComparison.InvariantCultureIgnoreCase)
                        );
                    return true;

                //case TokenType.StringValue:
                default:
                    value = content;
                    return true;
            }
        }
        
        #endregion
    }
}