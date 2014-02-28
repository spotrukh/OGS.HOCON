using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OGS.HOCON;

namespace OGS.Tests
{
    public class HOCONReaderTestHelper
    {
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

        static public DictionaryReader CreateReader()
        {
            var reader = new DictionaryReader(TestSourcesResolver);
            return reader;
        }
    }

    [TestFixture]
    public class HOCONReaderTests
    {
        private class Node
        {
        }

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

        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Already included: include_cycles_2")]
        public void IncludeCycleDependencyTest()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("include_cycles_1");
        }

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

        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Expected assign or begin scope, but: Unknown, offset: 7")]
        public void NegativeScenarious1Test()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("negative1");
        }

        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Expected arra/string/numeric/bool/substitution, but: Unknown, offset: 9")]
        public void NegativeScenarious2Test()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("negative2");
        }

        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Expected begin end scope '}' or a property, but: Unknown, offset: 9")]
        public void NegativeScenarious3Test()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("negative3");
        }

        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Expected arra/string/numeric/bool/substitution, but: Unknown, offset: 20")]
        public void NegativeScenarious4Test()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("negative4");
        }

        [Test, ExpectedException(typeof(ReaderException), ExpectedMessage = "Substitution not found: 'missed'")]
        public void NegativeScenarious5Test()
        {
            // Prepare
            var reader = HOCONReaderTestHelper.CreateReader();

            // Act
            reader.Read("negative5");
        }
    }
}
