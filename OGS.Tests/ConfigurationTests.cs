namespace OGS.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using NUnit.Framework;

    using OGS.Config;

    /// <summary>
    /// The configuration tests.
    /// </summary>
    [TestFixture]
    public class ConfigurationTests
    {
        /// <summary>
        /// The config source.
        /// </summary>
        private const string ConfigSource =
@"
ssh_client {
    connection {
        host : 127.0.0.1
        port : 22
    }

    status : on
}
math {
    pi : 3.14
}
sources {
    s_array : [""first"", second]
    i_array : [1024, 2048]
    b_array : [on, off]
    d_array : [3.14, 2.0]
    o_array : [1, 3.14, hello]
}";

        /// <summary>
        /// The read test.
        /// </summary>
        [Test]
        public void ReadTest()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            Assert.IsTrue(config.HasPath("ssh_client"));
            Assert.IsTrue(config.HasPath("ssh_client.connection"));
            Assert.IsTrue(config.HasValue("ssh_client.connection.host"));
            Assert.IsTrue(config.HasValue("ssh_client.connection.port"));
            Assert.IsTrue(config.HasValue("ssh_client.status"));
        }

        /// <summary>
        /// The read from string test.
        /// </summary>
        [Test]
        public void ReadFromStringTest()
        {
            var config = new Configuration(_ => string.Empty);
            config.ReadFromString(ConfigSource);

            Assert.IsTrue(config.HasPath("ssh_client"));
            Assert.IsTrue(config.HasPath("ssh_client.connection"));
            Assert.IsTrue(config.HasValue("ssh_client.connection.host"));
            Assert.IsTrue(config.HasValue("ssh_client.connection.port"));
            Assert.IsTrue(config.HasValue("ssh_client.status"));
        }

        /// <summary>
        /// The read from stream test.
        /// </summary>
        [Test]
        public void ReadFromStreamTest()
        {
            var config = new Configuration(_ => string.Empty);
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(ConfigSource)))
            {
                config.ReadFromStream(stream);
            }

            Assert.IsTrue(config.HasPath("ssh_client"));
            Assert.IsTrue(config.HasPath("ssh_client.connection"));
            Assert.IsTrue(config.HasValue("ssh_client.connection.host"));
            Assert.IsTrue(config.HasValue("ssh_client.connection.port"));
            Assert.IsTrue(config.HasValue("ssh_client.status"));
        }

        /// <summary>
        /// The get string value test.
        /// </summary>
        [Test]
        public void GetStringValueTest()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            Assert.AreEqual("127.0.0.1", config.GetString("ssh_client.connection.host"));
            Assert.AreEqual("http://localhost", config.GetString("ssh_client.connection.url", "http://localhost"));
        }

        /// <summary>
        /// The get int value test.
        /// </summary>
        [Test]
        public void GetIntValueTest()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            Assert.AreEqual(22, config.GetInt("ssh_client.connection.port"));
            Assert.AreEqual(8080, config.GetInt("web_client.port", 8080));
        }

        /// <summary>
        /// The get decimal value test.
        /// </summary>
        [Test]
        public void GetDecimalValueTest()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            Assert.AreEqual(3.14m, config.GetDecimal("math.pi"));
            Assert.AreEqual(2.718m, config.GetDecimal("math.e", 2.718m));
        }

        /// <summary>
        /// The get bool value test.
        /// </summary>
        [Test]
        public void GetBoolValueTest()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            Assert.AreEqual(true, config.GetBool("ssh_client.status"));
            Assert.AreEqual(true, config.GetBool("ssh_client.logging", true));
        }

        /// <summary>
        /// The get object value test.
        /// </summary>
        [Test]
        public void GetObjectValueTest()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            var value = config.GetValue("ssh_client.status");
            Assert.IsTrue(value is bool);
            Assert.AreEqual(true, value);

            value = config.GetValue("ssh_client.welcomeMessage");
            Assert.AreEqual(null, value);
        }

        /// <summary>
        /// The get string list value test.
        /// </summary>
        [Test]
        public void GetStringListValueTest()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            var value = config.GetStringList("sources.s_array");
            Assert.AreEqual(2, value.Count);
            Assert.AreEqual("first", value[0]);
            Assert.AreEqual("second", value[1]);

            value = config.GetStringList("sources.s_array_dummy", new List<string> { "s1" });
            Assert.AreEqual(1, value.Count);
            Assert.AreEqual("s1", value[0]);
        }

        /// <summary>
        /// The get int list value test.
        /// </summary>
        [Test]
        public void GetIntListValueTest()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            var value = config.GetIntList("sources.i_array");
            Assert.AreEqual(2, value.Count);
            Assert.AreEqual(1024, value[0]);
            Assert.AreEqual(2048, value[1]);

            value = config.GetIntList("sources.i_array_dummy", new List<int> { 333 });
            Assert.AreEqual(1, value.Count);
            Assert.AreEqual(333, value[0]);
        }

        /// <summary>
        /// The get decimal list value test.
        /// </summary>
        [Test]
        public void GetDecimalListValueTest()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            var value = config.GetDecimalList("sources.d_array");
            Assert.AreEqual(2, value.Count);
            Assert.AreEqual(3.14m, value[0]);
            Assert.AreEqual(2.0m, value[1]);

            value = config.GetDecimalList("sources.d_array_dummy", new List<decimal> { 333.222m });
            Assert.AreEqual(1, value.Count);
            Assert.AreEqual(333.222m, value[0]);
        }

        /// <summary>
        /// The get bool list value test.
        /// </summary>
        [Test]
        public void GetBoolListValueTest()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            var value = config.GetBoolList("sources.b_array");
            Assert.AreEqual(2, value.Count);
            Assert.AreEqual(true, value[0]);
            Assert.AreEqual(false, value[1]);

            value = config.GetBoolList("sources.b_array_dummy", new List<bool> { false, true, false });
            Assert.AreEqual(3, value.Count);
            Assert.AreEqual(false, value[0]);
            Assert.AreEqual(true, value[1]);
            Assert.AreEqual(false, value[2]);
        }

        /// <summary>
        /// The get object list value test.
        /// </summary>
        [Test]
        public void GetObjectListValueTest()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            var value = config.GetValueList("sources.o_array");
            Assert.AreEqual(3, value.Count);
            Assert.AreEqual(1, value[0]);
            Assert.AreEqual(3.14m, value[1]);
            Assert.AreEqual("hello", value[2]);

            value = config.GetValueList("sources.o_array_dummy", new List<object> { "first", true, 1 });
            Assert.AreEqual(3, value.Count);
            Assert.AreEqual("first", value[0]);
            Assert.AreEqual(true, value[1]);
            Assert.AreEqual(1, value[2]);
        }

        /// <summary>
        /// The negative case 1 test.
        /// </summary>
        [Test, ExpectedException(typeof(ConfigurationException), 
            ExpectedMessage = "Invalid type, expected 'System.String', but: 'System.Collections.Generic.List`1[System.Object]'")]
        public void NegativeCase1Test()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            config.GetString("sources.o_array");
        }

        /// <summary>
        /// The negative case 2 test.
        /// </summary>
        [Test, ExpectedException(typeof(ConfigurationException),
            ExpectedMessage = "Invalid type, expected 'System.Collections.Generic.List`1[System.String]', but: 'System.String'")]
        public void NegativeCase2Test()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            config.GetStringList("ssh_client.connection.host");
        }

        /// <summary>
        /// The negative case 3 test.
        /// </summary>
        [Test, ExpectedException(typeof(ConfigurationException),
            ExpectedMessage = "Invalid type, expected 'System.Collections.Generic.List`1[System.Object]', but: 'System.String'")]
        public void NegativeCase3Test()
        {
            var config = new Configuration(_ => ConfigSource);
            config.Read("main");

            config.GetValueList("ssh_client.connection.host");
        }
    }
}
