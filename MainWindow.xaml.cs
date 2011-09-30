using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.Reflection;

namespace SilentInstall {
  public partial class MainWindow : Window {

    private XmlDocument xmlDocument = new XmlDocument();   
    private BackgroundWorker installer = new BackgroundWorker();

    private List<string> ClientList {
      get {
        List<string> list = new List<string>();

        XmlDocument xd = new XmlDocument();
        try {
          xd.Load(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location + @"\config\ClientList.xml"));

          foreach (XmlNode xnode in xd.DocumentElement.SelectNodes("clients/name")) {
            list.Add(xnode.InnerText.Trim().ToUpper());
          }
        }
        catch (Exception) {
          list.Clear();
        }

        return list;
      }
    }
    private string ConfigurationPath {
      get {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("{0}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        sb.AppendFormat(@"\config\NT{0}.", Environment.OSVersion.Version.Major.ToString());
        sb.AppendFormat(@"{0}-config-x{1}.xml", Environment.OSVersion.Version.Minor.ToString(), Environment.Is64BitOperatingSystem ? "64" : "86");

        return sb.ToString();
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

      try {
        this.xmlDocument.Load(this.ConfigurationPath);
        this.installer.RunWorkerAsync();
      }
      catch (FileNotFoundException) {
        MessageBox.Show("Could not locate config file, make sure the \\config folder is in the same folder as the .exe", "File Not Found");
        Application.Current.Shutdown(1);
      }
      catch (XmlException) {
        MessageBox.Show("The configuration file was an invalid xml file. Pleas make sure to validate your configuraiton file.", "XML Syntax");
        Application.Current.Shutdown(1);
      }
      catch (Exception) {
        MessageBox.Show("The application encountered a critical exception and must close.", "Critical Exception");
        Application.Current.Shutdown(2);
      }
    }

    private void ThreadedWorker(object sender, DoWorkEventArgs e) {
      //App.BlockInput(true);
      List<Installation> Installations = new List<Installation>();

      foreach (XmlNode n in this.xmlDocument.DocumentElement.SelectNodes("install")) {
        Installations.Add(new Installation(n));
      }

      foreach (Installation i in Installations) {
          foreach (InstallationItem item in i.InstallItems) {
            InstallSoftware(item, i);

            if (new Version(i.CurrentVersion).CompareTo(new Version(item.Version)) == -1) {
              if (this.Debug == "true") {
                InstallSoftware(item, i);
              }
              else {
                if (i.Clients.Count > 0 && i.Clients.Contains(Environment.MachineName)) {
                  InstallSoftware(item, i);
                  break;
                }
                else if (this.ClientList.Contains(Environment.MachineName)) {
                    InstallSoftware(item, i);
                }
              }
            }         
          }
        }
      //App.BlockInput(false);
    }

    private void WriteOutput(string message) {
      if (this.textBox1.Dispatcher.CheckAccess()) {
        this.textBox1.AppendText(message + "\r\n");
        this.textBox1.ScrollToEnd();
      }
      else {
        this.textBox1.Dispatcher.Invoke(DispatcherPriority.Normal, new Action( delegate() {
          this.textBox1.AppendText(message + "\r\n");
          this.textBox1.ScrollToEnd();
        }));
      }
    }

    private void InstallSoftware(InstallationItem item, Installation i) {
      if (i.CurrentVersion == "0.0.0.0") {
        WriteOutput(String.Format("Installing {0} version {1} ...", i.Name, item.Version));
      }
      else {
        WriteOutput(String.Format("Upgrading {0} version {1} to version {2}", i.Name, i.CurrentVersion, item.Version));
      }

      if (item.CommandType.ToLower() == "cmd") {
        using (Process p = new Process()) {
          ProcessStartInfo sinfo = new ProcessStartInfo("cmd", "/c " + item.CommandText);

          if (item.CopyFrom.Length > 0 && item.CopyTo.Length > 0) {
            CopyDirectoryRecursive(new DirectoryInfo(item.CopyFrom), new DirectoryInfo(item.CopyTo));
            sinfo.WorkingDirectory = item.CopyTo;
          }

          sinfo.WindowStyle = ProcessWindowStyle.Hidden;
          p.StartInfo = sinfo;

          p.Start();
          p.WaitForExit();
        }
      }

      WriteOutput(String.Format("{0} version {1} installation finished.", i.Name, item.Version));
    }

    private static void CopyDirectoryRecursive(DirectoryInfo source, DirectoryInfo target)  {

        if (Directory.Exists(target.FullName) == false) {
            Directory.CreateDirectory(target.FullName);
        }

        foreach (FileInfo fi in source.GetFiles()) {
            fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
        }

        foreach (DirectoryInfo SubDirectory in source.GetDirectories()) {
            DirectoryInfo nextDirectory = target.CreateSubdirectory(SubDirectory.Name);
            CopyDirectoryRecursive(SubDirectory, nextDirectory);
        }
    }

  }
}
