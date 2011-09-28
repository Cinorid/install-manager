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
    private BackgroundWorker installer = new BackgroundWorker();

    private List<string> ClientList {
      get {
        List<string> list = new List<string>();

        foreach (XmlNode xnode in this.xmlDocument.DocumentElement.SelectNodes("clients/name")) {
          list.Add(xnode.InnerText.Trim().ToUpper());
        }

        return list;
      }
    }

    private string Authentication {
      get { return Convert.ToString(Application.Current.Properties["Authentication"]); }
    }

    private string Debug {
      get { return Convert.ToString(Application.Current.Properties["Debug"]); }
    }

    public MainWindow() {
      InitializeComponent();

      installer.DoWork += new DoWorkEventHandler(ThreadedWorker);

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
      catch (FileNotFoundException) {
        MessageBox.Show("Could not locate config file, make sure the \\config folder is in the same folder as the .exe", "File Not Found");
        Application.Current.Shutdown(1);
      }
      catch (XmlException e) {
        MessageBox.Show("The configuration file was an invalid xml file. Pleas make sure to validate your configuraiton file.", "XML Syntax");
        Application.Current.Shutdown(1);
      }
      catch (Exception e) {
        MessageBox.Show("The application encountered a critical exception and must close.", "Critical Exception");
        Application.Current.Shutdown(2);
      }

      this.installer.RunWorkerAsync(this.xmlDocument);
    }

    private void ThreadedWorker(object sender, DoWorkEventArgs e) {
      this.WriteOutput(this.ConfigurationPath);

      foreach (string s in this.ClientList) {
        this.WriteOutput(s);
      }
      foreach (XmlNode xnode in xmlDocument.DocumentElement.SelectNodes("*")) {
        if (xnode.SelectSingleNode("./install") != null) {
          Installation inst = new Installation() { InstallationNode = xnode, Name = xnode.Name };
          this.WriteOutput(inst.Name);
          foreach (string s in inst.AllowedMachines) {
            WriteOutput(s);
          }
          WriteOutput(inst.RegistryKey + "|" + inst.RegistryAttribute + "|" + inst.CurrentVersion);
          foreach (InstallItem i in inst.InstallationItems) {
            this.WriteOutput(i.Version);
            foreach (InstallCommand cmd in i.Commands) {
              if (cmd.Type.ToUpper() == "PS") {

              }
              else {
                using (Process p = new Process()) {
                  ProcessStartInfo info = new ProcessStartInfo("cmd", "/c " + cmd.Command);
                  info.UseShellExecute = false;
                  info.WindowStyle = ProcessWindowStyle.Hidden;
                  info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                  p.StartInfo = info;
                  //p.Start();
                }
              }             
            }
          }
        }
      }
    }

    private void WriteOutput(string message) {
      if (this.textBox1.Dispatcher.CheckAccess()) {
        this.textBox1.AppendText(message + "\r\n");
      }
      else {
        this.textBox1.Dispatcher.Invoke(DispatcherPriority.Normal, new Action( delegate() {
          this.textBox1.AppendText(message + "\r\n");
        }));
      }
    }

  }

  public class Installation {

    public XmlNode InstallationNode { get; set; }
    public string Name { get; set; }
    public string RegistryKey {
      get {
        if (this.InstallationNode.SelectSingleNode("./registry") != null) {
          return this.InstallationNode.SelectSingleNode("./registry").InnerText;
        }
        else {
          return "";
        }
      }
    }

    public string RegistryAttribute {
      get {
        if (this.InstallationNode.SelectSingleNode("./registry") != null) {
          return this.InstallationNode.SelectSingleNode("./registry").Attributes["property"].InnerText;
        }
        else {
          return "";
        }
      }
    }

    public string CurrentVersion {
      get {
        string version = "";

        if (!String.IsNullOrEmpty(this.RegistryKey)) {
          if (!String.IsNullOrEmpty(this.RegistryAttribute)) {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(this.RegistryKey)) {
              version = Convert.ToString(key.GetValue(this.RegistryAttribute));
            }
          }
          else {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(this.RegistryKey)) {
              version = key.SubKeyCount > 0 ? key.GetSubKeyNames()[key.SubKeyCount - 1] : "";
            }
          }
        }

        return version.Trim();
      }

    }

    public List<InstallItem> InstallationItems {
      get {
        List<InstallItem> list = new List<InstallItem>();
        foreach (XmlNode n in this.InstallationNode.SelectNodes("./install")) {
          list.Add(new InstallItem() { Version = n.SelectSingleNode("./version").InnerText, ItemNode = n });
        }
        return list;
      }
    }

    public List<string> AllowedMachines {
      get {
        List<string> list = new List<string>();
        foreach (XmlNode n in this.InstallationNode.SelectNodes("./allowed-machines/name")) {
          list.Add(n.InnerText);
        }
        return list;
      }
    }

    public Installation() {

    }

  }

  public class InstallItem {

    public XmlNode ItemNode { get; set; }
    public string Version { get; set; }
    public List<InstallCommand> Commands {
      get {
        List<InstallCommand> list = new List<InstallCommand>();
        foreach (XmlNode n in this.ItemNode.SelectNodes("./command")) {
          list.Add(new InstallCommand() { Type = n.Attributes["type"].InnerText, Command = n.InnerText });
        }
        return list;
      }
    }

    public InstallItem() {

    }
  }

  public class InstallCommand {
    public string Type { get; set; }
    public string Command { get; set; }

    public InstallCommand() {

    }
  }
}
