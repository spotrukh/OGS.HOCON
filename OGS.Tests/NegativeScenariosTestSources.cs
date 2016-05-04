namespace OGS.Tests
{
    /// <summary>
    /// The negative scenarios test sources.
    /// </summary>
    public static class NegativeScenariosTestSources
    {
        /// <summary>
        /// The negative 1.
        /// </summary>
        public static readonly string Negative1 = @"client }";

        /// <summary>
        /// The negative 2.
        /// </summary>
        public static readonly string Negative2 = @"client : {";

        /// <summary>
        /// The negative 3.
        /// </summary>
        public static readonly string Negative3 = @"client { :";

        /// <summary>
        /// The negative 4.
        /// </summary>
        public static readonly string Negative4 = @"client { property : }";

        /// <summary>
        /// The negative 5.
        /// </summary>
        public static readonly string Negative5 = @"client = $(missed)";
    }
}
