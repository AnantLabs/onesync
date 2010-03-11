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
        private const string MenuName = "Folder\\shell\\OneSyncMenuOption-v0.0";
        private const string Command = "Folder\\shell\\OneSyncMenuOption-v0.0\\command";

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
                    regmenu.SetValue("", "Sync this folder with OneSync Zero");
                regcmd = Registry.ClassesRoot.CreateSubKey(Command);
                if(regcmd != null)
                    regcmd.SetValue("", "\"" + Assembly.GetExecutingAssembly().Location + "\" \"%1\"");
            }
            catch(Exception ex)
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
                reg = Registry.ClassesRoot.OpenSubKey(MenuName);
                if (reg != null)
                {
                    reg.Close();
                    Registry.ClassesRoot.DeleteSubKey(MenuName);
                }
            }
            catch (Exception ex)
            {
                //Do nothing again?
            }

            //Delete the installation dir because there is a data.md file stored inside.
            //if(File.Exists(System.Windows.Forms.Application.StartupPath + @"\data.md"))
            //{
                //Directory.Delete(System.Windows.Forms.Application.StartupPath + @"\data.md", true);
            //}
        }
    }
}
