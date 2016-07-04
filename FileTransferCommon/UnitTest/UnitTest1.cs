using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileTransferCommon;

namespace FileTransferCommon.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestFileCommon()
        {
            string file_path = @"F:\project\FileTransfer\Test\example.txt";
            Assert.IsTrue(FileCommon.GetSize(file_path) > 0);
        }
        [TestMethod]
        public void TestFileReceiveGetLocalPort()
        {
            string file_path = @"F:\project\FileTransfer\Test\example_test.txt";
            FileReceive recTest = new FileReceive(file_path);
            Console.WriteLine(recTest.InitAndGetLocalEndPoint().ToString());
        }
        [TestMethod()]
        public void ComputeHashTest()
        {
            string file_path = @"F:\project\FileTransfer\Test\example.txt";
            Console.WriteLine(FileCommon.ComputeHash(file_path));
        }
    }
}
