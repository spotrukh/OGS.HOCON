namespace OGS.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using NUnit.Framework;

    using OGS.HOCON;

    /// <summary>
    /// The HOCON reader tests.
    /// </summary>
    [TestFixture]
    public class HOCONReaderTests
    {
        /// <summary>
        /// The read test.
        /// </summary>
        [Test]
        public void ReadTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("example1");

            // Validate
            object testValue;
            Assert.IsTrue(reader.Source.TryGetValue("client.url", out testValue));
            Assert.IsTrue(testValue is string);
            Assert.AreEqual("http://127.0.0.1", testValue);

            Assert.IsTrue(reader.Source.TryGetValue("client.port", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(80, testValue);
        }

        /// <summary>
        /// The read from string test.
        /// </summary>
        [Test]
        public void ReadFromStringTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.ReadFromString(TestSources.Example1);

            // Validate
            object testValue;
            Assert.IsTrue(reader.Source.TryGetValue("client.url", out testValue));
            Assert.IsTrue(testValue is string);
            Assert.AreEqual("http://127.0.0.1", testValue);

            Assert.IsTrue(reader.Source.TryGetValue("client.port", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(80, testValue);
        }

        /// <summary>
        /// The read from stream test.
        /// </summary>
        [Test]
        public void ReadFromStreamTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(TestSources.Example1)))
            {
                reader.ReadFromStream(stream);
            }
            
            // Validate
            object testValue;
            Assert.IsTrue(reader.Source.TryGetValue("client.url", out testValue));
            Assert.IsTrue(testValue is string);
            Assert.AreEqual("http://127.0.0.1", testValue);

            Assert.IsTrue(reader.Source.TryGetValue("client.port", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(80, testValue);
        }

        /// <summary>
        /// The include test.
        /// </summary>
        [Test]
        public void IncludeTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("include1");

            // Validate
            object testValue;
            Assert.IsTrue(reader.Source.TryGetValue("client.url", out testValue));
            Assert.IsTrue(testValue is string);
            Assert.AreEqual("http://127.0.0.1", testValue);

            Assert.IsTrue(reader.Source.TryGetValue("client.status", out testValue));
            Assert.IsTrue(testValue is bool);
            Assert.AreEqual(true, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("client.port", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(1024, testValue);
        }

        /// <summary>
        /// The include cycle dependency test.
        /// </summary>
        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Already included: include_cycles_2")]
        public void IncludeCycleDependencyTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("include_cycles_1");
        }

        /// <summary>
        /// The substitution test.
        /// </summary>
        [Test]
        public void SubstitutionTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("substitution");

            // Validate
            object testValue;
            Assert.IsTrue(reader.Source.TryGetValue("defaultPort", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(22, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("ssh.ip", out testValue));
            Assert.IsTrue(testValue is string);
            Assert.AreEqual("127.0.0.1", testValue);

            Assert.IsTrue(reader.Source.TryGetValue("ssh.port", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(22, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("ssh.enabled", out testValue));
            Assert.IsTrue(testValue is bool);
            Assert.AreEqual(true, testValue);
        }

        /// <summary>
        /// The safe substitution test.
        /// </summary>
        [Test]
        public void SafeSubstitutionTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("safe-substitution");

            // Validate
            object testValue;
            Assert.IsTrue(reader.Source.TryGetValue("ssh.ip", out testValue));
            Assert.IsTrue(testValue is string);
            Assert.AreEqual("127.0.0.1", testValue);

            Assert.IsTrue(reader.Source.TryGetValue("ssh.port", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(22, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("ssh.fallback", out testValue) == false);
            
            Assert.IsTrue(reader.Source.TryGetValue("ssh.enabled", out testValue));
            Assert.IsTrue(testValue is bool);
            Assert.AreEqual(true, testValue);
        }

        /// <summary>
        /// The values test.
        /// </summary>
        [Test]
        public void ValuesTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("data-types");

            // Validate
            object testValue;
            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.string1", out testValue));
            Assert.IsTrue(testValue is string);
            Assert.AreEqual("127.0!@#$%^&   _(*)_   +|}{><,.", testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.string2", out testValue));
            Assert.IsTrue(testValue is string);
            Assert.AreEqual("/home/root", testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.number1", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(123, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.string3", out testValue));
            Assert.IsTrue(testValue is string);
            Assert.AreEqual("123.456.7890", testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.string4", out testValue));
            Assert.IsTrue(testValue is string);
            Assert.AreEqual("1234567890_-abcde", testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.number2", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(-123, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.number3", out testValue));
            Assert.IsTrue(testValue is decimal);
            Assert.AreEqual(123.1234d, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.number4", out testValue));
            Assert.IsTrue(testValue is decimal);
            Assert.AreEqual(-123.1234d, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.number5", out testValue));
            Assert.IsTrue(testValue is decimal);
            Assert.AreEqual(100000m, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.number6", out testValue));
            Assert.IsTrue(testValue is decimal);
            Assert.AreEqual(-100000m, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.boolean1", out testValue));
            Assert.IsTrue(testValue is bool);
            Assert.AreEqual(true, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.boolean2", out testValue));
            Assert.IsTrue(testValue is bool);
            Assert.AreEqual(false, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.boolean3", out testValue));
            Assert.IsTrue(testValue is bool);
            Assert.AreEqual(true, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.boolean4", out testValue));
            Assert.IsTrue(testValue is bool);
            Assert.AreEqual(false, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.boolean5", out testValue));
            Assert.IsTrue(testValue is bool);
            Assert.AreEqual(true, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.boolean6", out testValue));
            Assert.IsTrue(testValue is bool);
            Assert.AreEqual(false, testValue);
        }

        /// <summary>
        /// The arrays test.
        /// </summary>
        [Test]
        public void ArraysTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("array-tests");

            // Validate int array
            object testValue;
            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.array1", out testValue));
            var array = testValue as List<object>;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.IsTrue(array[0] is int);
            Assert.AreEqual(1, array[0]);
            Assert.IsTrue(array[1] is int);
            Assert.AreEqual(2, array[1]);
            Assert.IsTrue(array[2] is int);
            Assert.AreEqual(3, array[2]);

            // Validate decimal array
            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.array2", out testValue));
            array = testValue as List<object>;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.IsTrue(array[0] is decimal);
            Assert.AreEqual(1.1d, array[0]);
            Assert.IsTrue(array[1] is decimal);
            Assert.AreEqual(2.2d, array[1]);
            Assert.IsTrue(array[2] is decimal);
            Assert.AreEqual(3.3d, array[2]);

            // Validate string array
            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.array3", out testValue));
            array = testValue as List<object>;
            Assert.IsNotNull(array);
            Assert.AreEqual(3, array.Count);
            Assert.IsTrue(array[0] is string);
            Assert.AreEqual("s1", array[0]);
            Assert.IsTrue(array[1] is string);
            Assert.AreEqual("s2", array[1]);
            Assert.IsTrue(array[2] is string);
            Assert.AreEqual("s3", array[2]);

            // Validate boolean array
            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.array4", out testValue));
            array = testValue as List<object>;
            Assert.IsNotNull(array);
            Assert.AreEqual(6, array.Count);
            Assert.IsTrue(array[0] is bool);
            Assert.AreEqual(true, array[0]);
            Assert.IsTrue(array[1] is bool);
            Assert.AreEqual(false, array[1]);
            Assert.IsTrue(array[2] is bool);
            Assert.AreEqual(true, array[2]);
            Assert.IsTrue(array[3] is bool);
            Assert.AreEqual(false, array[3]);
            Assert.IsTrue(array[4] is bool);
            Assert.AreEqual(true, array[4]);
            Assert.IsTrue(array[5] is bool);
            Assert.AreEqual(false, array[5]);
        }

        /// <summary>
        /// The mixed data types array test.
        /// </summary>
        [Test]
        public void MixedDataTypesArrayTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("mixed-array-tests");

            // Validate
            object testValue;
            Assert.IsTrue(reader.Source.TryGetValue("dataTypes.array", out testValue));
            var array = testValue as List<object>;
            Assert.IsNotNull(array);
            Assert.AreEqual(4, array.Count);
            Assert.IsTrue(array[0] is int);
            Assert.AreEqual(1, array[0]);
            Assert.IsTrue(array[1] is decimal);
            Assert.AreEqual(2.2d, array[1]);
            Assert.IsTrue(array[2] is string);
            Assert.AreEqual("s3", array[2]);
            Assert.IsTrue(array[3] is bool);
            Assert.AreEqual(true, array[3]);
        }

        /// <summary>
        /// The extends test.
        /// </summary>
        [Test]
        public void ExtendsTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("extends");

            // Validate
            object testValue;
            Assert.IsTrue(reader.Source.TryGetValue("base.property1", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(1, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("base.property2", out testValue) == false);

            Assert.IsTrue(reader.Source.TryGetValue("child.property1", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(1, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("child.property2", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(2, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("child2.property1", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(1, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("child2.property2", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(3, testValue);
        }

        /// <summary>
        /// The overrides test.
        /// </summary>
        [Test]
        public void OverridesTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("overrides");

            // Validate
            object testValue;
            Assert.IsTrue(reader.Source.TryGetValue("base.property1", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(2, testValue);

            Assert.IsTrue(reader.Source.TryGetValue("base.property2", out testValue));
            Assert.IsTrue(testValue is int);
            Assert.AreEqual(3, testValue);
        }

        /// <summary>
        /// The negative scenario 1 test.
        /// </summary>
        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Expected assign or begin scope, but: Unknown, offset: 7")]
        public void NegativeScenario1Test()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("negative1");
        }

        /// <summary>
        /// The negative scenario 2 test.
        /// </summary>
        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Expected arra/string/numeric/bool/substitution, but: Unknown, offset: 9")]
        public void NegativeScenario2Test()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("negative2");
        }

        /// <summary>
        /// The negative scenario 3 test.
        /// </summary>
        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Expected begin end scope '}' or a property, but: Unknown, offset: 9")]
        public void NegativeScenario3Test()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("negative3");
        }

        /// <summary>
        /// The negative scenario 4 test.
        /// </summary>
        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Expected arra/string/numeric/bool/substitution, but: Unknown, offset: 20")]
        public void NegativeScenario4Test()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("negative4");
        }

        /// <summary>
        /// The negative scenario 5 test.
        /// </summary>
        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Substitution not found: 'missed'")]
        public void NegativeScenario5Test()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("negative5");
        }

        /// <summary>
        /// The node.
        /// </summary>
        private class Node
        {
        }
    }
}
