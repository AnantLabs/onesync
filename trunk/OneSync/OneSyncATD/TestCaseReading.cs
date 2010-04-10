using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OneSyncATD
{
    class TestCaseReading
    {
        public List<TestCase> readTestCases(String filePath)
        {
            List<TestCase> listCases = new List<TestCase>();
            try 
            {
                using (StreamReader streamReader = File.OpenText(filePath))
                {
                    String inputLine;
                    while ((inputLine = streamReader.ReadLine()) != null)
                    {
                        listCases.Add(new TestCase(inputLine));
                        
                    }
                    streamReader.Close();
                }
            }
            catch (Exception exception)
            {
                throw (exception);
            }
            return listCases;
        }
    }
}
