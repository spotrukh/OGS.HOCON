namespace OGS.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using NUnit.Framework;

    using OGS.HOCON;

    /// <summary>
    /// The hocon writer tests.
    /// </summary>
    [TestFixture]
    public class HOCONWriterTests
    {
        /// <summary>
        /// The writer string test.
        /// </summary>
        [Test]
        public void WriterStringTest()
        {
            // Prepare
            var writer = new Writer<Node>();
            var values = new Dictionary<string, object>
                             {
                                 { "levelKey", 1 },
                                 { "multilevel", new Node() },
                                 { "multilevel.level2", new Node() },
                                 { "multilevel.level2.number", 1 },
                                 { "multilevel.level2.decimal", 1.1d },
                                 { "multilevel.level2.boolean", true },
                                 { "multilevel.level2.string", "str1" },
                                 { "second_multilevel", new Node() },
                                 {
                                     "second_multilevel.array",
                                     new List<object> { "str1", 1, 2.2d }
                                 },
                             };

            // Act
            var data = writer.WriteString(values);

            // Validate
            const string Expected = @"
levelKey : 1

multilevel {

	level2 {
		boolean : true
		decimal : 1.1
		number : 1
		string : ""str1""
	}
}

second_multilevel {
	array : [""str1"", 1, 2.2]
}
";
            Assert.AreEqual(Expected, data);
        }

        /// <summary>
        /// The writer stream test.
        /// </summary>
        [Test]
        public void WriterStreamTest()
        {
            // Prepare
            var writer = new Writer<Node>();
            var values = new Dictionary<string, object>
                             {
                                 { "rootLevelKey", 1 },
                                 { "multilevel", new Node() },
                                 { "multilevel.level2", new Node() },
                                 { "multilevel.level2.number", 1 },
                                 { "multilevel.level2.decimal", 1.1d },
                                 { "multilevel.level2.boolean", true },
                                 { "multilevel.level2.string", "str1" },
                             };

            // Act
            var stream = new MemoryStream();
            writer.WriteStream(stream, values);
            stream.Seek(0, SeekOrigin.Begin);

            // Validate
            const string Expected = @"
multilevel {

	level2 {
		boolean : true
		decimal : 1.1
		number : 1
		string : ""str1""
	}
}

rootLevelKey : 1
";
            Assert.AreEqual(Expected, (new StreamReader(stream)).ReadToEnd());
        }

        /// <summary>
        /// The writer wit comment test.
        /// </summary>
        [Test]
        public void WriterWitCommentTest()
        {
            // Prepare
            var writer = new Writer<Node>();
            var values = new Dictionary<string, object>
                             {
                                 { "rootLevelKey", 1 },
                                 { "multilevel", new Node() },
                                 { "multilevel.level2", new Node() },
                                 { "multilevel.level2.number", 1 },
                                 { "multilevel.level2.decimal", 1.1d },
                                 { "multilevel.level2.boolean", true },
                                 { "multilevel.level2.string", "str1" },
                             };

            // Act
            var data = writer.WriteString(values, "Multiline" + Environment.NewLine + "comment");

            // Validate
            const string Expected = @"# Multiline
# comment

multilevel {

	level2 {
		boolean : true
		decimal : 1.1
		number : 1
		string : ""str1""
	}
}

rootLevelKey : 1
";
            Assert.AreEqual(Expected, data);
        }

        /// <summary>
        /// The node.
        /// </summary>
        private class Node
        {
        }
    }
}
