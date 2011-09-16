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

namespace JenzabarSilentInstall {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {

    public MainWindow() {
      InitializeComponent();

      if (Application.Current.Properties["DBConnection"] != null) {
        this.textBox1.Text = Application.Current.Properties["DBConnection"] + "\r\n";
      }

      //RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE");

      List<string> thiss = new List<string>();
      // List<String> thiss = new ArrayList<string>();

      //foreach (String s in key.GetSubKeyNames()) {
        //this.textBox1.Text += s + "\r\n";
        string strBaseInstall = @"\\hadbsvc\Jenzabar\EX\Client_Installs\3.6\EX-3.6-Setup.exe /S /V";
        string strBaseOptions = @"\\hadbsvc\Jenzabar\Script_Files\EX36Setup.txt";
        string strBaseSwitches = " /qn SETUPFILE=";
        this.textBox1.Text = String.Format("{0} \"{1}\" {2}", strBaseInstall, strBaseSwitches, strBaseOptions);

       

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
