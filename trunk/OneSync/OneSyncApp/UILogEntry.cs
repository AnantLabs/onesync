/*
 $Id$
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OneSync
{
	public class UILogEntry
	{
		public string ImageSrc { get; set; }
		public string FileName { get; set; }
    	public string Status { get; set; }
    	public string Message { get; set; }
	}
}