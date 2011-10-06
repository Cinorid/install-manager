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
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace SilentInstall {
  public partial class MainWindow : Window {

    private XmlDocument xmlDocument = new XmlDocument();   
    private BackgroundWorker installer = new BackgroundWorker();

    private List<string> ClientList {
      get {
        List<string> list = new List<string>();

        XmlDocument xd = new XmlDocument();
        try {
          xd.Load(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\config\ClientList.xml");

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

      this.ShowInTaskbar = false;

      installer.DoWork += new DoWorkEventHandler(ThreadedWorker);
      this.MouseLeftButtonDown += delegate(object sender, MouseButtonEventArgs e) {
        this.DragMove();
      };

      this.versionLabel.Content += Assembly.GetExecutingAssembly().GetName().Version.ToString();

      // COLOR FUN
      if (Environment.MachineName == "DRAKE" || Environment.MachineName == "MORTON" || Environment.MachineName == "CLEANMACHINE") {
        this.MainBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EECC6600"));
        this.MainBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEFFFFFF"));
      }
      // END COLOR FUN

      using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Office\14.0\Word\Options")) {
        MessageBox.Show(key.GetValue("SqlSecurityCheck").ToString());
      }

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
      List<Installation> Installations = new List<Installation>();

      foreach (XmlNode n in this.xmlDocument.DocumentElement.SelectNodes("install")) {
        Installations.Add(new Installation(n));
      }
     
      int installs = 0;

      foreach (XmlNode n in this.xmlDocument.DocumentElement.SelectNodes("uninstall")) {
        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(n.SelectSingleNode("./registry").InnerText)) {
          if (key != null) {
            string name = n.SelectSingleNode("./name").InnerText;
            string vers = n.SelectSingleNode("./remove-item/version").InnerText;

            WriteOutput(String.Format("Uninstalling {0}  version {1} ...", name, vers));
            using (Process p = new Process()) {
              ProcessStartInfo sinfo = new ProcessStartInfo("cmd", "/c " + n.SelectSingleNode("./remove-item/command").InnerText);

              sinfo.WindowStyle = ProcessWindowStyle.Hidden;
              p.StartInfo = sinfo;

              p.Start();
              p.WaitForExit();
            }
            WriteOutput(String.Format("{0}  version {1} successfully uninstalled.", name, vers));
          }
        }
      }

      foreach (Installation i in Installations) {
        foreach (InstallationItem item in i.InstallItems) {
          if (new Version(i.CurrentVersion).CompareTo(new Version(item.Version)) == -1) {
            if (this.Debug == "true") { 
              installs++;
            }
            else {
              if (i.Clients.Contains(Environment.MachineName)) { installs++; }
              if (i.Clients.Count == 0 && this.ClientList.Contains(Environment.MachineName)) { installs++; }
            }
          }
        }
      }

      this.SetProgressMaximum(installs);

      foreach (Installation i in Installations) {
        foreach (InstallationItem item in i.InstallItems) {
          if (new Version(i.CurrentVersion).CompareTo(new Version(item.Version)) == -1) {
            if (this.Debug == "true") {
              InstallSoftware(item, i);
              this.AdjustProgress();
            }
            else {
              if (i.Clients.Contains(Environment.MachineName)) {
                InstallSoftware(item, i);
                this.AdjustProgress();
              }

              if (i.Clients.Count == 0 && this.ClientList.Contains(Environment.MachineName)) {
                InstallSoftware(item, i);
                this.AdjustProgress();
              }
            }
          }        
        }
      }

      foreach (XmlNode n in this.xmlDocument.DocumentElement.SelectNodes("registry")) {
        RegistryItem r = new RegistryItem(n);
        if (!r.FixRegistry()) {
          WriteOutput("Installing " + r.Name + "...");
          using (Process p = new Process()) {
            ProcessStartInfo sinfo = new ProcessStartInfo("cmd", "/c " + r.FailOver);

            sinfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo = sinfo;

            p.Start();
            p.WaitForExit();
          }
          WriteOutput("Finished installing " + r.Name + ".");
          r.FixRegistry();
          WriteOutput("Fixing " + r.Name + " registry entry.");
        }
      }

      WriteOutput("");
      WriteOutput("All software upgrades/installations are finished.");
      WriteOutput("The software update is now complete and will close shortly. Thank you!");
      Thread.Sleep(10000);

      ThreadStart ts = delegate() {
        Dispatcher.BeginInvoke(new Action(delegate() {
          Application.Current.Shutdown();
        }));
      };

      new Thread(ts).Start();
    }

    #region "Threading Delegate Dispatcher Functions"
    private void AdjustProgress() {
      if (this.ProgressReport.Dispatcher.CheckAccess()) {
        this.ProgressReport.Value++;
      }
      else {
        this.ProgressReport.Dispatcher.BeginInvoke(new Action(delegate() {
          this.ProgressReport.Value++;
        }));
      }
    }

    private void SetProgressMaximum(int max) {
      if (this.ProgressReport.Dispatcher.CheckAccess()) {
        this.ProgressReport.Value = 0;
        this.ProgressReport.Maximum = max;
      }
      else {
        this.ProgressReport.Dispatcher.BeginInvoke(new Action(delegate() {
          this.ProgressReport.Value = 0;
          this.ProgressReport.Maximum = max;
        }));
      }
    }

    private void WriteOutput(string message) {
      if (this.SummaryText.Dispatcher.CheckAccess()) {
        this.SummaryText.AppendText(message + "\r\n");
        this.SummaryText.ScrollToEnd();
      }
      else {
        this.SummaryText.Dispatcher.Invoke(new Action(delegate() {
          this.SummaryText.AppendText(message + "\r\n");
          this.SummaryText.ScrollToEnd();
        }));
      }
    }
    #endregion

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

    private static void CopyDirectoryRecursive(DirectoryInfo source, DirectoryInfo target) {
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