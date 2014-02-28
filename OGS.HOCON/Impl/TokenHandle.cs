namespace OGS.HOCON.Impl
{
    internal class TokenHandle
    {
        public string Value { get; private set; }
        private readonly int _nextOffset;

        public TokenHandle(string value, int nextOffset)
        {
            Value = value;
            _nextOffset = nextOffset;
        }

        public int Consume()
        {
            return _nextOffset;
        }
    }
}