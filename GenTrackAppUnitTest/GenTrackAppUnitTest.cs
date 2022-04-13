using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GenTrackApp;

namespace GenTrackAppUnitTest
{
    [TestClass]
    public class GenTrackAppUnitTest
    {
        [DataRow("200,12345678901,E1,E1,E1,N1,HGLMET501,KWH,30,", "12345678901")]
        [DataRow("200,/?1ac~`,E1,E1,E1,N1,HGLMET502,KWH,30,", "/?1ac~`")]
        [DataRow("300,20180117,1.000,1.000,1.008,1.000,1.000,1.000,1.96,1.6,4.212,1.992,2.132,2.532,6.192,5.396,5.616,6.012,5.544,7.436,7.472,5.888,4.316,4.66,5.368,5.644,5.392,6.612,5.8,6.636,6.572,6.36,10.992,9.52,10.268,9.704,9.616,9.308,13.1,20.36,16.456,11.144,9.712,6.076,6.064,5.324,7.18,6.228,5.628,5.94,A,,,20180120032031,", "20180117")]
        [DataRow("12514,,0,0,0,0,0,0,0,0,0,0,0,0,0,0,00,0,0,0,0,0,0,0,0,0,0,0,0,0,0,", "")]
        [DataTestMethod]
        public void TestGetFileName(string testVal, string expected)
        {
            string actual = Program.GetFileName(testVal);
            Assert.AreEqual(expected, actual);
        }

        [DataRow("200,12345678901,E1,E1,E1,N1,HGLMET501,KWH,30,", 200)]
        [DataRow("900,/?1ac~`,E1,E1,E1,N1,HGLMET502,KWH,30,", 900)]
        [DataRow("012,20180117,1.000,1.000,1.008,1.000,1.000,1.000,1.96,1.6,4.212,1.992,2.132,", 012)]
        [DataRow("12514,,0,0,0,0,0,0,0,0,0,0,0,0,0,0,00,0,0,0,0,0,0,0,0,0,0,0,0,0,0,", 125)]
        [DataTestMethod]
        public void TestReadFirstElement(string testVal, int expected)
        {
            int actual = Program.ReadFirstElement(testVal);
            Assert.AreEqual(expected, actual);
        }
    }
}
