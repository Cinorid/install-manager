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

namespace SilentInstall {
  public partial class MainWindow : Window {

    private XmlDocument xmlDocument = new XmlDocument();   
    private BackgroundWorker installer = new BackgroundWorker();

    private List<string> ClientList {
      get {
        List<string> list = new List<string>();

        XmlDocument xd = new XmlDocument();
        try {
          xd.Load(Environment.CurrentDirectory + @"\config\ClientList.xml");

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
        sb.AppendFormat("{0}", Environment.CurrentDirectory);
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

      //System.Version v = new System.Version("3.6.7.1");
      //WriteOutput(v.CompareTo(new System.Version("3.6.7.2")).ToString());


      this.installer.RunWorkerAsync();
    }

    private void ThreadedWorker(object sender, DoWorkEventArgs e) {
      List<Installation> Installations = new List<Installation>();

      foreach (XmlNode n in this.xmlDocument.DocumentElement.SelectNodes("install")) {
        Installations.Add(new Installation(n));
      }

      foreach (Installation i in Installations) {
        //WriteOutput(String.Format("{0} : {1}", i.Name, i.CurrentVersion));
        if (i.CurrentVersion == "0.0.0.0") {
          foreach (InstallationItem item in i.InstallItems) {
            if (this.Debug == "true") {
              WriteOutput(String.Format("Installing {0} version {1}...", i.Name, item.Version));
              Thread.Sleep(5000);
              WriteOutput(String.Format("{0} version {1} installation finished.", i.Name, item.Version));
            }
            else {
              if (i.Clients.Count > 0 && i.Clients.Contains(Environment.MachineName)) {
                WriteOutput(String.Format("Installing {0} version {1}...", i.Name, item.Version));
                Thread.Sleep(5000);
                WriteOutput(String.Format("{0} version {1} installation finished.", i.Name, item.Version));
              }
              else {
                if (this.ClientList.Contains(Environment.MachineName)) {
                  WriteOutput(String.Format("Installing {0} version {1}...", i.Name, item.Version));
                  Thread.Sleep(5000);
                  WriteOutput(String.Format("{0} version {1} installation finished.", i.Name, item.Version));
                }
              }
            }
          }
        }
        else {
          foreach (InstallationItem item in i.InstallItems) {
            if (new Version(i.CurrentVersion).CompareTo(new Version(item.Version)) == -1) {
              if (this.Debug == "true") {
                WriteOutput(String.Format("Installing {0} version {1}...", i.Name, item.Version));
                Thread.Sleep(5000);
                WriteOutput(String.Format("{0} version {1} installation finished.", i.Name, item.Version));
              }
              else {
                if (i.Clients.Count > 0 && i.Clients.Contains(Environment.MachineName)) {
                  WriteOutput(String.Format("Installing {0} version {1}...", i.Name, item.Version));
                  Thread.Sleep(5000);
                  WriteOutput(String.Format("{0} version {1} installation finished.", i.Name, item.Version));
                }
                else {
                  if (this.ClientList.Contains(Environment.MachineName)) {
                    WriteOutput(String.Format("Installing {0} version {1}...", i.Name, item.Version));
                    Thread.Sleep(5000);
                    WriteOutput(String.Format("{0} version {1} installation finished.", i.Name, item.Version));
                  }
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
}
