using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace JenzabarSilentInstall {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {

    protected override void OnStartup(StartupEventArgs e) {

      this.Properties["ConfigurationPath"] = "";

      if (e.Args != null && e.Args.Count() > 0 && e.Args.Count() % 2 == 0) {

        for (int i = 0; i < e.Args.Count(); i += 2) {
          if (e.Args[i] == "-c" || e.Args[i] == "--config") { this.Properties["ConfigurationPath"] = e.Args[i + 1]; }
        }

      }

      base.OnStartup(e);
    }
  }
}
