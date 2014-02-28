namespace OGS.Tests
{
    public static class TestSources
    {
        public static readonly string Example1 =
            @"
#comment 1
// comment 2
client { 
    url : ""http://127.0.0.1""
    port : 80
}";

        public static readonly string Include1 =
            @"
client { 
    url : ""http://127.0.0.1""
    port : 80
}
include ""include2""
";

        public static readonly string Include2 =
            @"client { 
# url : ""http://127.0.0.1""
    port : 1024
    status: on
}";

        public static readonly string IncludeCycles1 = @"include ""include_cycles_2""";

        public static readonly string IncludeCycles2 = @"include ""include_cycles_2""";

        public static readonly string Substitution =
            @"
defaultPort : 22

ssh { 
    ip : ""127.0.0.1""
    port : $(defaultPort)
    enabled : true
}";
        public static readonly string SafeSubstitution =
            @"
defaultPort : 22
ssh { 
    ip : ""127.0.0.1""
    port : $(?defaultPort)
    fallback : $(?preferedPort)
    enabled : true
}";

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

        public static readonly string ArrayTests =
            @"
dataTypes { 
    array1 : [1, 2, 3]
    array2 : [1.1, 2.2, 3.3]
    array3 : [""s1"", ""s2"", ""s3""]
    array4 : [true, false, on, off, enabled, disabled]
}";

        public static readonly string MixedArrayTests = @"dataTypes { array : [1, 2.2, ""s3"", on] }";

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

    public static class NegativeScenariosTestSources
    {
        public static readonly string Negative1 = @"client }";
        public static readonly string Negative2 = @"client : {";
        public static readonly string Negative3 = @"client { :";
        public static readonly string Negative4 = @"client { property : }";
        public static readonly string Negative5 = @"client = $(missed)";
    }
}
