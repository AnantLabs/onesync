using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSyncATD
{
    class TestResultWriting
    {
        public void writeResult(String testFolderPath, List<TestCase> listCases)
        {
            String resultFileName = string.Format(testFolderPath + "\\outputresult_{0:yyyyMMdd_hhmmsstt}.txt", DateTime.Now);
            StreamWriter streamWriter = new StreamWriter(resultFileName);
            foreach (TestCase resultCases in listCases)
            {                
                streamWriter.WriteLine(resultCases.showResult());
                streamWriter.Flush();
            }
            streamWriter.Close();
        }
    }
}
