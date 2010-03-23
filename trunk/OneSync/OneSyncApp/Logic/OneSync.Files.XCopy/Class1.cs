/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneSync.Files.XCopy
{
    public class Class1
    {
       
       public Class1()
       {
           XFilter filter = new XFilter();
           string source = @"C:\Users\Chockablock\Desktop\source";
           string destination = @"C:\Users\Chockablock\Desktop\destination";
           XCopy copy = new XCopy(source, destination, null);
           copy.Copy();
       }
       
    }
}
