using FSync.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace FSync.Test
{
    
    
    /// <summary>
    ///This is a test class for FileInfoComparerTest and is intended
    ///to contain all FileInfoComparerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FileInfoComparerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest()
        {
            string baseSource = string.Empty; // TODO: Initialize to an appropriate value
            string baseDestination = string.Empty; // TODO: Initialize to an appropriate value
            FileInfoComparer target = new FileInfoComparer(baseSource, baseDestination); // TODO: Initialize to an appropriate value
            FileInfo fileInfo1 = null; // TODO: Initialize to an appropriate value
            FileInfo fileInfo2 = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Equals(fileInfo1, fileInfo2);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for FileInfoComparer Constructor
        ///</summary>
        [TestMethod()]
        public void FileInfoComparerConstructorTest()
        {
            string baseSource = string.Empty; // TODO: Initialize to an appropriate value
            string baseDestination = string.Empty; // TODO: Initialize to an appropriate value
            FileInfoComparer target = new FileInfoComparer(baseSource, baseDestination);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod()]
        public void GetHashCodeTest()
        {
            string baseSource = string.Empty; // TODO: Initialize to an appropriate value
            string baseDestination = string.Empty; // TODO: Initialize to an appropriate value
            FileInfoComparer target = new FileInfoComparer(baseSource, baseDestination); // TODO: Initialize to an appropriate value
            FileInfo fi = null; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.GetHashCode(fi);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
