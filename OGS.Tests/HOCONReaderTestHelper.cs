namespace OGS.Tests
{
    using System;

    using OGS.HOCON;

    /// <summary>
    /// The HOCON reader test helper.
    /// </summary>
    public class HOCONReaderTestHelper
    {
        /// <summary>
        /// The create reader.
        /// </summary>
        /// <returns>
        /// The <see cref="DictionaryReader"/>.
        /// </returns>
        public static DictionaryReader CreateReader()
        {
            var reader = new DictionaryReader(TestSourcesResolver);
            return reader;
        }

        /// <summary>
        /// The test sources resolver.
        /// </summary>
        private static string TestSourcesResolver(string sourceName)
        {
            switch (sourceName.ToLower())
            {
                case "example1": return TestSources.Example1;

                case "include1": return TestSources.Include1;
                case "include2": return TestSources.Include2;

                case "include_cycles_1": return TestSources.IncludeCycles1;
                case "include_cycles_2": return TestSources.IncludeCycles2;

                case "substitution": return TestSources.Substitution;
                case "safe-substitution": return TestSources.SafeSubstitution;

                case "data-types": return TestSources.DataTypes;

                case "array-tests": return TestSources.ArrayTests;
                case "mixed-array-tests": return TestSources.MixedArrayTests;

                case "extends": return TestSources.Extends;
                case "overrides": return TestSources.Overrides;

                case "negative1": return NegativeScenariosTestSources.Negative1;
                case "negative2": return NegativeScenariosTestSources.Negative2;
                case "negative3": return NegativeScenariosTestSources.Negative3;
                case "negative4": return NegativeScenariosTestSources.Negative4;
                case "negative5": return NegativeScenariosTestSources.Negative5;
            }

            throw new Exception(string.Format("Source not found: '{0}'", sourceName));
        }
    }
}
