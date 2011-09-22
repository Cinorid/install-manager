using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management.Automation;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace SilentInstall {
  public partial class MainWindow : Window {

    private XmlDocument xmlDocument = new XmlDocument();
    private string ConfigurationPath { get; set; }

    public MainWindow() {
      InitializeComponent();

      StringBuilder sb = new StringBuilder();
      sb.AppendFormat("{0}", Environment.CurrentDirectory);
      sb.AppendFormat(@"\config\NT{0}.", Environment.OSVersion.Version.Major.ToString());
      sb.AppendFormat(@"{0}-config-x{1}.xml", Environment.OSVersion.Version.Minor.ToString(), Environment.Is64BitOperatingSystem ? "64" : "86");

      this.ConfigurationPath = sb.ToString();

      try {
        using (StreamReader sr = new StreamReader(this.ConfigurationPath)) {
          this.xmlDocument.LoadXml(sr.ReadToEnd());
          sr.Close();
        }
      }
      catch (FileNotFoundException e) {

      }
      catch (XmlException e) {

      }
      catch (Exception e) {

      }
    }
  }
}
