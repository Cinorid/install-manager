using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading;

namespace SilentInstall {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {

    [DllImportAttribute("user32.dll", EntryPoint="BlockInput")]
    [return: System.Runtime.InteropServices.MarshalAsAttribute(UnmanagedType.Bool)]
    public static extern  bool BlockInput([System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)] bool fBlockIt);


    public static void BlockMouseKeyboard(TimeSpan span) {
      try { 
        App.BlockInput(true);
        Thread.Sleep(span);
      } 
      finally {
        App.BlockInput(false);
      }
    }

    protected override void OnStartup(StartupEventArgs e) {

      this.Properties["ConfigurationPath"] = "";
      this.Properties["Authentication"] = "standard";
      this.Properties["Debug"] = "false";

      if (e.Args != null && e.Args.Count() > 0 && e.Args.Count() % 2 == 0) {
        for (int i = 0; i < e.Args.Count(); i += 2) {
          if (e.Args[i] == "-c" || e.Args[i] == "--config") { this.Properties["ConfigurationPath"] = e.Args[i + 1]; }
          if (e.Args[i] == "-a" || e.Args[i] == "--auth") { this.Properties["Authentication"] = e.Args[i + 1]; }
          if (e.Args[i] == "-d" || e.Args[i] == "--debug") { this.Properties["Debug"] = e.Args[i + 1]; }
        }
      }

      base.OnStartup(e);
    }
  }
}
