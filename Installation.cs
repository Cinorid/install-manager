using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace SilentInstall {

  public class Installation {

    private XmlNode InstallNode;

    public string Name {
      get {
        XmlNode xn = this.InstallNode.SelectSingleNode("./name");
        string n = xn != null ? xn.InnerText : "";
        return n;
      }
    }

    public string CurrentVersion {
      get {
        XmlNode xn = this.InstallNode.SelectSingleNode("./registry");
        string v = "0.0.0.0";

        if (xn != null) {
          string subkey = xn.Attributes["latest-subkey"] != null ? xn.Attributes["latest-subkey"].InnerText.Trim() : "false";
          string kvp = xn.Attributes["value-pair"] != null ? xn.Attributes["value-pair"].InnerText.Trim() : "";
          string path = xn.InnerText.Trim();

          
          using (RegistryKey key = Registry.LocalMachine.OpenSubKey(path)) {
            if (key != null) {
              if (kvp.Length > 0 && subkey == "false") { v = Convert.ToString(key.GetValue(kvp)); }
              if (kvp.Length == 0 && subkey == "true") { v = key.SubKeyCount > 0 ? key.GetSubKeyNames()[key.SubKeyCount - 1] : "0.0.0.0"; }
              if (kvp.Length > 0 && subkey == "true") { v = key.SubKeyCount > 0 ? Convert.ToString(key.OpenSubKey((key.GetSubKeyNames()[key.SubKeyCount - 1])).GetValue(kvp)) : "0.0.0.0"; }
            }
            else {
              v = "0.0.0.0";
            }
          }
        }

        return v;
      }
    }

    public List<string> Clients {
      get {
        List<string> list = new List<string>();
        XmlNodeList xl = this.InstallNode.SelectNodes("./clients/name");

        foreach (XmlNode n in xl) { list.Add(n.InnerText.Trim().ToUpper()); }

        return list;
      }
    }

    public List<InstallationItem> InstallItems {
      get {
        List<InstallationItem> list = new List<InstallationItem>();
        XmlNodeList xl = this.InstallNode.SelectNodes("./install-item");

        foreach (XmlNode n in xl) { list.Add(new InstallationItem(n)); }

        return list;
      }
    }

    public Installation(XmlNode InstallNode) {
      this.InstallNode = InstallNode;
    }
  }

  public class InstallationItem {

    private XmlNode ItemNode;

    public string Version {
      get {
        XmlNode xn = this.ItemNode.SelectSingleNode("./version");
        string n = xn != null ? xn.InnerText : "0.0.0.0";
        return n;
      }
    }

    public string CommandType {
      get {
        XmlNode xn = this.ItemNode.SelectSingleNode("./command");
        string n = xn.Attributes["type"] != null ? xn.Attributes["type"].InnerText : "";
        return n;
      }
    }

    public string CommandText {
      get {
        XmlNode xn = this.ItemNode.SelectSingleNode("./command");
        string n = xn != null ? xn.InnerText : "";
        return n;
      }
    }

    public string CopyFrom {
      get {
        XmlNode xn = this.ItemNode.SelectSingleNode("./copy/from");
        string n = xn != null ? xn.InnerText.Trim() : "";
        return n;
      }
    }

    public string CopyTo {
      get {
        XmlNode xn = this.ItemNode.SelectSingleNode("./copy/to");
        string n = xn != null ? xn.InnerText.Trim() : "";
        return n;
      }
    }

    public InstallationItem(XmlNode ItemNode) {
      this.ItemNode = ItemNode;
    }
  }

  public class RegistryItem {

    private XmlNode RegistryNode;

    public string Name {
      get {
        XmlNode xn = this.RegistryNode.SelectSingleNode("./name");
        string n = xn != null ? xn.InnerText : "";
        return n;
      }
    }

    public string Root {
      get {
        XmlNode xn = this.RegistryNode.SelectSingleNode("./root");
        string n = xn != null ? xn.InnerText : "";
        return n;
      }
    }

    public string KeyStart {
      get {
        XmlNode xn = this.RegistryNode.SelectSingleNode("./key-start");
        string n = xn != null ? xn.InnerText : "";
        return n;
      }
    }

    public List<string> Clients {
      get {
        List<string> list = new List<string>();
        XmlNodeList xl = this.RegistryNode.SelectNodes("./clients/name");

        foreach (XmlNode n in xl) { list.Add(n.InnerText.Trim().ToUpper()); }

        return list;
      }
    }

    public RegistryItem(XmlNode RegistryNode) {
      this.RegistryNode = RegistryNode;
    }

    public int FixRegistry() {
      int flag = 0;

      foreach (XmlNode x in this.RegistryNode.SelectNodes("./subkeys/subkey")) {

        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(this.KeyStart + x.InnerText)) {

          if (key != null) {
            string nvp = x.Attributes["value-pair"].InnerText;
            string type = x.Attributes["type"].InnerText;
            string value = x.Attributes["value"].InnerText;

            try {
              if (key.GetValue(nvp).ToString() != value) {
                using (RegistryKey ckey = Registry.CurrentUser.CreateSubKey(this.KeyStart + x.InnerText)) {
                  ckey.SetValue(nvp, value, RegistryValueKind.DWord);
                }
                flag++;
              }
            }
            catch (NullReferenceException) {
              using (RegistryKey ckey = Registry.CurrentUser.CreateSubKey(this.KeyStart + x.InnerText)) {
                ckey.SetValue(nvp, value, RegistryValueKind.DWord);
              }
              flag++;
            }
          }
        }
      }

      return flag;
    }
  }
}
