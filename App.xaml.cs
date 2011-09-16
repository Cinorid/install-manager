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

      if (e.Args != null && e.Args.Count() > 0) {
        this.Properties["DBConnection"] = e.Args[0];
      }

      base.OnStartup(e);
    }
  }
}
