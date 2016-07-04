using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileTransferCommon;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestFileCommon()
        {
            string file_path = @"F:\project\FileTransfer\Test\example.txt";
            FileCommon fileTest = new FileCommon(file_path);
            Assert.IsTrue(fileTest.GetSize() > 0);
        }
        [TestMethod]
        public void TestFileReceiveGetLocalPort()
        {
            string file_path = @"F:\project\FileTransfer\Test\example_test.txt";
            FileReceive recTest = new FileReceive(file_path);
            Console.WriteLine(recTest.InitAndGetLocalEndPoint().ToString());
        }
    }
}
