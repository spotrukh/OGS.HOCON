namespace OGS.HOCON.Impl
{
    internal enum TokenType
    {
        Unknown,
        Comment,
        Space,

        Include,

        Key,
        Assign,
        BeginScope,
        EndScope,

        StringValue,
        NumericValue,
        DeciamlValue,
        BooleanValue,
        Substitution,
        SafeSubstitution,

        BeginArray,
        EndArray,
        ArraySeparator,
        DoubleValue
    }
}