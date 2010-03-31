/*
 $Id$
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Reflection;
using System.IO;


namespace OneSync
{
    [RunInstaller(true)]
    public partial class OneSyncInstaller : Installer
    {
        private const string MenuName = "Folder\\shell\\OneSyncMenuOption-v1.0";
        private const string Command = "Folder\\shell\\OneSyncMenuOption-v1.0\\command";

        private const string AppMenuName = "Folder\\shell\\OneSyncMenuOption-v1.0App";
        private const string AppCommand = "Folder\\shell\\OneSyncMenuOption-v1.0App\\command";

        public OneSyncInstaller()
        {
            InitializeComponent();
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);
            RegistrationServices regSrv = new RegistrationServices();
            regSrv.RegisterAssembly(base.GetType().Assembly,
              AssemblyRegistrationFlags.SetCodeBase);
            
            //Add a new context menu item.
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(MenuName);
                if(regmenu != null)
                    regmenu.SetValue("", "Sync with OneSync V1.0");
                regcmd = Registry.ClassesRoot.CreateSubKey(Command);
                if(regcmd != null)
                    regcmd.SetValue("", "\"" + Assembly.GetExecutingAssembly().Location + "\" \"%1\"");

                regmenu = Registry.ClassesRoot.CreateSubKey(AppMenuName);
                if (regmenu != null)
                    regmenu.SetValue("", "Open OneSync V1.0");
                regcmd = Registry.ClassesRoot.CreateSubKey(AppCommand);
                if (regcmd != null)
                    regcmd.SetValue("", "\"" + Assembly.GetExecutingAssembly().Location + "\"");
            }
            catch(Exception)
            {
                //Do nothing?!
            }
            finally       
            {
                if(regmenu != null)
                    regmenu.Close();
                if(regcmd != null)
                    regcmd.Close();
            }
        }

        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);
            RegistrationServices regSrv = new RegistrationServices();
            regSrv.UnregisterAssembly(base.GetType().Assembly);
            //Remove the added context menu item.
            try
            {
                RegistryKey reg = Registry.ClassesRoot.OpenSubKey(Command);
                if (reg != null)
                {
                    reg.Close();
                    Registry.ClassesRoot.DeleteSubKey(Command);
                }
                reg = Registry.ClassesRoot.OpenSubKey(AppCommand);
                if (reg != null)
                {
                    reg.Close();
                    Registry.ClassesRoot.DeleteSubKey(AppCommand);
                }

                reg = Registry.ClassesRoot.OpenSubKey(MenuName);
                if (reg != null)
                {
                    reg.Close();
                    Registry.ClassesRoot.DeleteSubKey(MenuName);
                }
                reg = Registry.ClassesRoot.OpenSubKey(AppMenuName);
                if (reg != null)
                {
                    reg.Close();
                    Registry.ClassesRoot.DeleteSubKey(AppMenuName);
                }
            }
            catch (Exception)
            {
                //Do nothing again?
            }
            
            //Delete the installation dir because there is a data.md file stored inside.
            string datamdLocation = Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.LastIndexOf(@"\")) + @"\data.md";
            if (File.Exists(datamdLocation))
            {
                File.Delete(datamdLocation);
            }
        }
    }
}
