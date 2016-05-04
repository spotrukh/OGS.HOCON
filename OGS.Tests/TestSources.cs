namespace OGS.Tests
{
    /// <summary>
    /// The test sources.
    /// </summary>
    public static class TestSources
    {
        /// <summary>
        /// The example 1.
        /// </summary>
        public static readonly string Example1 =
            @"
#comment 1
// comment 2
client { 
    url : ""http://127.0.0.1""
    port : 80
}";

        /// <summary>
        /// The include 1.
        /// </summary>
        public static readonly string Include1 =
            @"
client { 
    url : ""http://127.0.0.1""
    port : 80
}
include ""include2""
";

        /// <summary>
        /// The include 2.
        /// </summary>
        public static readonly string Include2 =
            @"client { 
# url : ""http://127.0.0.1""
    port : 1024
    status: on
}";

        /// <summary>
        /// The include cycles 1.
        /// </summary>
        public static readonly string IncludeCycles1 = @"include ""include_cycles_2""";

        /// <summary>
        /// The include cycles 2.
        /// </summary>
        public static readonly string IncludeCycles2 = @"include ""include_cycles_2""";

        /// <summary>
        /// The substitution.
        /// </summary>
        public static readonly string Substitution =
            @"
defaultPort : 22

ssh { 
    ip : ""127.0.0.1""
    port : $(defaultPort)
    enabled : true
}";

        /// <summary>
        /// The safe substitution.
        /// </summary>
        public static readonly string SafeSubstitution =
            @"
defaultPort : 22
ssh { 
    ip : ""127.0.0.1""
    port : $(?defaultPort)
    fallback : $(?preferedPort)
    enabled : true
}";

        /// <summary>
        /// The data types.
        /// </summary>
        public static readonly string DataTypes =
            @"
dataTypes { 
    string1 : ""127.0!@#$%^&   _(*)_   +|}{><,.""
    string2 : /home/root
    string3 : 123.456.7890
    string4 : 1234567890_-abcde
    number1 : 123
    number2 : -123
    number3 : 123.1234
    number4 : -123.1234
    number5 : 1e5
    number6 : -1E5
    boolean1 : true
    boolean2 : false
    boolean3 : on
    boolean4 : off
    boolean5 : enabled
    boolean6 : disabled
}";

        /// <summary>
        /// The array tests.
        /// </summary>
        public static readonly string ArrayTests =
            @"
dataTypes { 
    array1 : [1, 2, 3]
    array2 : [1.1, 2.2, 3.3]
    array3 : [""s1"", ""s2"", ""s3""]
    array4 : [true, false, on, off, enabled, disabled]
}";

        /// <summary>
        /// The mixed array tests.
        /// </summary>
        public static readonly string MixedArrayTests = @"dataTypes { array : [1, 2.2, ""s3"", on] }";

        /// <summary>
        /// The extends.
        /// </summary>
        public static readonly string Extends =
            @"base { 
    property1 : 1
}

child : $(base) { 
    property2 : 2
}
child2 : ${base} { 
    property2 : 3
}";

        /// <summary>
        /// The overrides.
        /// </summary>
        public static readonly string Overrides =
            @"base { 
    property1 : 1
}

base { 
    property1 : -2 # special override case
    property1 : 2
    property2 : 3
}";
    }
}
