﻿using MongoDB.Driver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FluentAssertions;

namespace MongoDB.MSTest
{


    /// <summary>
    ///This is a test class for IAdminDatabaseExtensionsTest and is intended
    ///to contain all IAdminDatabaseExtensionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IAdminDatabaseExtensionsTest
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
        ///A test for CloseAllDatabases
        ///</summary>
        [TestMethod()]
        public void CloseAllDatabasesTest()
        {
            IAdminOperations adminDatabase = null; // TODO: Initialize to an appropriate value
            IAdminOperationExtensions.CloseAllDatabases(adminDatabase);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for CopyDatabase
        ///</summary>
        [TestMethod()]
        public void CopyDatabaseTest()
        {
            IAdminOperations adminDatabase = null; // TODO: Initialize to an appropriate value
            IDatabase fromDatabase = null; // TODO: Initialize to an appropriate value
            IDatabase toDatabase = null; // TODO: Initialize to an appropriate value
            IAdminOperationExtensions.CopyDatabase(adminDatabase, fromDatabase, toDatabase);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for CopyDatabaseGetNonce
        ///</summary>
        [TestMethod()]
        public void CopyDatabaseGetNonceTest()
        {
            IAdminOperations adminDatabase = null; // TODO: Initialize to an appropriate value
            IDatabase fromDatabase = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = IAdminOperationExtensions.CopyDatabaseGetNonce(adminDatabase, fromDatabase);
            actual.Should().Be(expected);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for FSync
        ///</summary>
        [TestMethod()]
        public void FSyncTest()
        {
            IAdminOperations adminDatabase = null; // TODO: Initialize to an appropriate value
            bool asynchronous = false; // TODO: Initialize to an appropriate value
            bool shouldLock = false; // TODO: Initialize to an appropriate value
            IAdminOperationExtensions.FSync(adminDatabase, asynchronous, shouldLock);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for GetCmdCollection
        ///</summary>
        [TestMethod()]
        public void GetCmdCollectionTest()
        {
            //IAdminOperations db = null; // TODO: Initialize to an appropriate value
            //IDBCollection expected = null; // TODO: Initialize to an appropriate value
            //IDBCollection actual;
            //actual = IAdminDatabaseExtensions.GetCmdCollection(db);
            //actual.Should().Be(expected);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ReplacePeer
        ///</summary>
        [TestMethod()]
        public void ReplacePeerTest()
        {
            IAdminOperations adminDatabase = null; // TODO: Initialize to an appropriate value
            IAdminOperationExtensions.ReplacePeer(adminDatabase);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Shutdown
        ///</summary>
        [TestMethod()]
        public void ShutdownTest()
        {
            IAdminOperations adminDatabase = null; // TODO: Initialize to an appropriate value
            IAdminOperationExtensions.Shutdown(adminDatabase);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
