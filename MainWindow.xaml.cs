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

namespace JenzabarSilentInstall {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {

    private XmlDocument xmlDocument = new XmlDocument();

    private string ConfigurationPath { get; set; }

    private List<string> ClientList = new List<string>();

    public MainWindow() {
      InitializeComponent();

      this.ConfigurationPath = Application.Current.Properties["ConfigurationPath"] != null ? Application.Current.Properties["ConfigurationPath"].ToString() : Environment.CurrentDirectory + @"\config\";
      this.ConfigurationPath = Directory.Exists(ConfigurationPath) ? ConfigurationPath : Environment.CurrentDirectory + @"\config\";
      this.ConfigurationPath += ConfigurationPath.EndsWith(@"\") ? "" : @"\";
      this.ConfigurationPath += Environment.Is64BitOperatingSystem ? "config-x64.xml" : "config-x86.xml";

      if (File.Exists(ConfigurationPath)) {
        using (StreamReader sr = new StreamReader(ConfigurationPath)) {
          xmlDocument.LoadXml(sr.ReadToEnd());
          sr.Close();
        }
      }

      this.lblMacNameDescription.Content = Environment.MachineName;
      this.lblOSDescription.Content = String.Format("{0} 64 bit support: {1}", Environment.OSVersion, Environment.Is64BitOperatingSystem ? "Yes" : "No");

      foreach (XmlNode n in xmlDocument.DocumentElement.SelectNodes("ClientList/name")) {
        this.ClientList.Add(n.InnerText.Trim());
      }

      if (this.ClientList.Contains(Environment.MachineName)) {

      }
      else {
        Application.Current.Shutdown(0);
      }
       

        //if (File.Exists("config/sample.txt")) {
        //  using (StreamReader sr = new StreamReader("config/sample.txt")) {
        //    Process p = new Process();
        //    p.StartInfo = new ProcessStartInfo("cmd", "/c " + sr.ReadToEnd());
        //    p.Start();
        //    p.WaitForExit();
        //  }
        //}
      //}

      
    }
  }
}
