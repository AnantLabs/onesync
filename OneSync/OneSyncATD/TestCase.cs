using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSyncATD
{
    class TestCase
    {
        public String testLine, testID, testMethod, testParameters, testExpected, testActual;
        public String testComment = "";
        public Boolean testPassed = false;

        public TestCase(string readLine)
        {
            String[] caseInfo = readLine.Split(':');
            if (caseInfo.Length < 4) throw new Exception("Wrong Test Case Syntax.");
            this.testLine = readLine;
            this.testID = caseInfo[0].Trim();
            this.testMethod = caseInfo[1].Trim();
            this.testParameters = caseInfo[2].Trim();
            this.testExpected = caseInfo[3].Trim();
            if (caseInfo.Length > 4) this.testComment = caseInfo[4];
            this.testActual = "";
        }

        public string showResult()
        {
            String testResult = testPassed ? "pass" : "fail";
            return testResult + "[" + testLine + "]" + "test actual: " + testActual;
        }
    }
}
