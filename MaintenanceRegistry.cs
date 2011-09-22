using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.Management.Automation;

namespace SilentInstall {
  public class MaintenanceRegistry {

    public static string Jenzabar {
      get {
        string version = "";
        if (Environment.Is64BitOperatingSystem) {
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Jenzabar\EX\4\Application")) {
            version = Convert.ToString(key.GetValue("ProductVersion"));
          }
        }
        else {
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Jenzabar\EX\4\Application")) {
            version = Convert.ToString(key.GetValue("ProductVersion"));
          }
        }
        return version;
      }
    }
    public static string PowerFaids {
      get {
        string version = "";
        if (Environment.Is64BitOperatingSystem) {
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\College Board\PowerFAIDS")) {
            version = key.SubKeyCount > 0 ? key.GetSubKeyNames()[key.SubKeyCount - 1] : "";
          }
        }
        else {
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\College Board\PowerFAIDS")) {
            version = key.SubKeyCount > 0 ? key.GetSubKeyNames()[key.SubKeyCount - 1] : "";
          }
        }
        return version;
      }
    }
    public static string InfoMaker {
      get {
        string version = "";
        if (Environment.Is64BitOperatingSystem) {
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Sybase\InfoMaker")) {
            version = key.SubKeyCount > 0 ? Convert.ToString(key.OpenSubKey((key.GetSubKeyNames()[key.SubKeyCount - 1])).GetValue("Build")) : "";
          }
        }
        else {
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Sybase\InfoMaker")) {
            version = key.SubKeyCount > 0 ? Convert.ToString(key.OpenSubKey((key.GetSubKeyNames()[key.SubKeyCount - 1])).GetValue("Build")) : "";
          }
        }
        return version;
      }
    }

    public static string GhostScript {
      get {
        string version = "";
        if (Environment.Is64BitOperatingSystem) {
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\GPL Ghostscript")) {
            version = key.SubKeyCount > 0 ? key.GetSubKeyNames()[key.SubKeyCount - 1] : "";
          }
        }
        else {
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\GPL Ghostscript")) {
            version = key.SubKeyCount > 0 ? key.GetSubKeyNames()[key.SubKeyCount - 1] : "";
          }
        }
        return version;
      }
    }
    public static string MicrosoftSQLServer {
      get {
        string version = "";
        if (Environment.Is64BitOperatingSystem) {
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Microsoft\Microsoft SQL Server\100\Tools\ClientSetup\CurrentVersion")) {
            version = Convert.ToString(key.GetValue("CurrentVersion"));
          }
        }
        else {
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Microsoft SQL Server\100\Tools\ClientSetup\CurrentVersion")) {
            version = Convert.ToString(key.GetValue("CurrentVersion"));
          }
        }
        return version;
      }
    }

    public MaintenanceRegistry() {

    }
  }
}
