using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace myClientServer
{
    public class DoAction
    {
        private string myRegKey = @"SOFTWARE\RYIK\ServerClientCommunicator2";

        public string message = "";
        public string answer = "";
        private string[] stringWordsArray;
        protected internal string RemoteHostName = "";
        protected internal string ActionSelected;
        protected internal string ApplicationName;
        protected internal string AdditionalParameter;
        protected internal string ActionFolder;
        protected internal string sNameWindows = "";
        protected internal string sBuildWindows = "";
        protected internal string sBuildWindowsEx = "";
        protected internal string sVersionSoftware = "";
        
        private string[] sIntellect = new string[]
            {
            "slave.exe",
            "intellect.exe",
            "video.run",
            "event_viewer.run",
            "itvscript.exe",
            "vitlpr.run",
            "vitlprview.run",
            "vitlprvmon.run",
            "slave",
            "intellect",
            "video",
            "event_viewer",
            "itvscript",
            "vitlpr",
            "vitlprview",
            "vitlprvmon",
            };
        private readonly byte[] btsMess1 = Convert.FromBase64String(@"OCvesunvXXsxtt381jr7vp3+UCwDbE4ebdiL1uinGi0="); //Key Encrypt
        private readonly byte[] btsMess2 = Convert.FromBase64String(@"NO6GC6Zjl934Eh8MAJWuKQ=="); //Key Decrypt
        private EncryptDecrypt encryptDecrypt = new EncryptDecrypt();

        private string GetAddressFromMessage(string str)
        {
            RemoteHostName = (str.Split('(')[1]).Split(')')[0].Trim().ToUpper();
            //    try { ipLocalHostAddr = ((str.Split('(')[1]).Split(')')[0].Trim()); } catch { }
            return RemoteHostName;
        }

        public void CheckGotMessage()
        {
            RemoteHostName = "";
            ActionSelected = "";
            ApplicationName = "";
            AdditionalParameter = "";
            if (message != null && message.Trim().Length > 0)
            {

                try { stringWordsArray = message.Split(' '); } catch { }
                int iWords = 0;

                foreach (string sWord in stringWordsArray)
                { iWords++; }

                if (iWords > 0)
                {
                    try
                    {
                        RemoteHostName = GetAddressFromMessage(message);
                    }
                    catch { }
                    try { ActionSelected = stringWordsArray[1].Trim().ToUpper(); } catch { }
                    try { ApplicationName = stringWordsArray[2].Trim().ToLower(); } catch { }
                    try { AdditionalParameter = stringWordsArray[3].Trim().ToUpper(); } catch { }
                }

                //   MessageBox.Show(message+"\nRemoteHostName " + RemoteHostName+ "\nActionSelected "+ ActionSelected+ "\nApplicationName "+ ApplicationName+ "\nAdditionalParameter"+ AdditionalParameter);
                if (message.Contains("test port!")) ActionSelected = ""; //Test for opened ports from remote host

                if (ActionSelected.Length > 0)
                {
                    //  Form1.myForm._richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + Form1.myForm.RemoteHost.UserName + ": " + ActionSelected.ToLower());

                    switch (ActionSelected)
                    {
                        case "RUN":
                            RunApp(ApplicationName);
                            break;
                        case "TAKE":
                            if (ApplicationName.Length > 0 && ApplicationName == "screenshot")
                            {
                                SendGetFile sendFile = new SendGetFile();
                                try
                                {
                                    string str = TakeScreenshot(Form1.myForm.strFolderSend); //Path to Screenshot's File
                                    Form1.myForm._AppendTextToFile(Form1.myForm.FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + RemoteHostName + ": " + str);

                                    System.Threading.Tasks.Task t1 = System.Threading.Tasks.Task.Run(() =>
                                       sendFile.SendFile(RemoteHostName, Form1.myForm.RemoteHost.intPortGetFile, str)
                                        );
                                    t1.Wait(3000);
                                }
                                catch (Exception expt)
                                { Form1.myForm._AppendTextToFile(Form1.myForm.FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + RemoteHostName + ": Erorr: " + expt.ToString()); }//соединение было прервано
                            }
                            break;
                        case "MODE":
                            if (AdditionalParameter.Length > 0)
                            {
                                if (AdditionalParameter == "SERVER")
                                {
                                    if (ApplicationName.Length > 0)
                                    {
                                        Form1.myForm.modesWindow = ApplicationName;
                                        Form1.myForm.ChangeModeWindow(ApplicationName);
                                    }
                                }
                            }
                            break;
                        case "KILL":
                            KillApp(ApplicationName, " /T "); // option -  " /T /F "
                            break;
                        case "TASKLIST":
                            break;
                        case "GET":  //Action to get file by Server
                            break;
                        case "UPDATESERVER":
                            Form1.myForm._AppendTextToFile(Form1.myForm.FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + message);
                           // Random rnd = new Random();
                           // ActionFolder = rnd.Next().ToString();
                          //  UpdateServerMakecmd(ActionFolder);
                            break;
                        case "CLEAR":
                            if (ApplicationName.Length > 0 && RemoteHostName.Length > 0)
                            {
                                if (ApplicationName == "log" && RemoteHostName.ToUpper() != Form1.myForm.LocalHost.HostName.ToString().ToUpper())
                                {
                                    if (Form1.myForm != null)
                                    { Form1.myForm._richTextBoxEchoClear(); } //Доступ к главной форме из другого класса
                                }
                            }
                            break;
                        case "NAME":
                            TakePropertyServer();
                            break;
                        case "RESTART":
                            Form1.myForm._AppendTextToFile(Form1.myForm.FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + message);
                            RestartProcess(ApplicationName);
                            break;
                        default:
                            Form1.myForm._ShowForm(true);
                            Form1.myForm._richTextBoxEchoColor(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + Form1.myForm.RemoteHost.UserName + ": " + message, System.Drawing.Color.Red);
                            Form1.myForm._richTextBoxEchoColor("\n", System.Drawing.Color.Black);
                            //ActionSelected = "";
                            break;
                    }
                }
            }
            else { ActionSelected = "nothing"; }
        }

        //Action
        protected internal string TakeScreenshot(string strFolderSave)
        {
            string format = "yyyy.MM.dd HH.mm.ss";
            string ext = "png";
            System.Drawing.Size screenSz = Screen.PrimaryScreen.Bounds.Size;
            System.Drawing.Bitmap screenshot = new System.Drawing.Bitmap(screenSz.Width, screenSz.Height);
            System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(screenshot);
            gr.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, screenSz);
            string filepath = System.IO.Path.Combine(strFolderSave, Form1.myForm.LocalHost.UserName + " " + DateTime.Now.ToString(format)) + "." + ext;
            List<System.Reflection.PropertyInfo> props = typeof(System.Drawing.Imaging.ImageFormat).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).ToList();
            var imgformat = (System.Drawing.Imaging.ImageFormat)props.Find(prop => prop.Name.ToLower() == ext).GetValue(null, null);
            screenshot.Save(filepath, imgformat);
            return filepath;
        }

        protected internal void  UpdateServerMakecmd(string sPathCmd, string ActionFolder)
        {            
            try
            {
                using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(myRegKey))
                {
                    EvUserKey.SetValue("UPDATESERVERFOLDER", encryptDecrypt.EncryptStringToBase64Text(ActionFolder, btsMess1, btsMess2), Microsoft.Win32.RegistryValueKind.String);
                }
            }
            catch { }

            try
            {
                if (System.IO.File.Exists(sPathCmd))
                {
                    try { System.IO.File.Delete(sPathCmd); } catch { }
                    Thread.Sleep(500);
                }

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("@echo off");
                sb.AppendLine("taskkill /f /im " +  System.IO.Path.GetFileName(Application.ExecutablePath) + " > nul");
                sb.AppendLine("timeout /T 2 /NOBREAK > nul");
                sb.AppendLine("del /F /Q \"" +Application.ExecutablePath + "\" > nul");
                sb.AppendLine("timeout /T 2 /NOBREAK > nul");
                sb.AppendLine("copy /Y /V \"" + (System.IO.Path.GetDirectoryName(Application.ExecutablePath)) + @"\" + ActionFolder + @"\" + System.IO.Path.GetFileName(Application.ExecutablePath) + "\" " +
                    " \"" + (System.IO.Path.GetDirectoryName(Application.ExecutablePath)) + @"\" + System.IO.Path.GetFileName(Application.ExecutablePath) + "\" " + " > nul");
                sb.AppendLine("timeout /T 2 /NOBREAK > nul");
                sb.AppendLine("start \"\" \"" + Application.ExecutablePath + "\"");
                System.IO.File.WriteAllText(sPathCmd, sb.ToString().Replace(@"\\", @"\"), System.Text.Encoding.GetEncoding(866));
                sb = null;
            }
            catch { }
        }

        protected internal void TakePropertyServer()
        {
            System.Diagnostics.FileVersionInfo myFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            sVersionSoftware = myFileVersionInfo.ProductName + " ver. b." + myFileVersionInfo.FileVersion;

            string sIntellectRegPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";   //Check Windows Type
            using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sIntellectRegPath, Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
            {
                try
                {
                    sNameWindows = EvUserKey.GetValue("ProductName").ToString().ToUpper().Trim();
                    sBuildWindows = EvUserKey.GetValue("CurrentBuild").ToString().ToLower().Trim();
                    sBuildWindowsEx = EvUserKey.GetValue("BuildLabEx").ToString().ToLower().Trim();
                    //BuildLabEx      -   10240.17354.amd64fre.th1_st1.170327-1827

                    /*
Текущие версии Windows 10 по вариантам обслуживания
Вариант обслуживания                        	Version 	OS build 	
Полугодовой канал (для определенных устройств) 	1709    	16299.98 	
Текущая ветвь (CB)                           	1703    	15063.729 	
Текущая ветвь (CB)                             	1607    	14393.1914
Текущая ветвь (CB)                          	1511    	10586.1232
Полугодовой канал                           	1703    	15063.729 
Текущая ветвь для бизнеса (CBB)               	1607    	14393.1914 
Текущая ветвь для бизнеса (CBB) 	            1511    	10586.1232
Ветвь долгосрочного обслуживания (LTSB)     	1607    	14393.1914
Ветвь долгосрочного обслуживания (LTSB)     	1507 (RTM) 	10240.17673 
                     */
                }
                catch { }
            }

            answer = "\nHost:" + Form1.myForm.LocalHost.UserName + "\nOS: " + sNameWindows + " " + sBuildWindows + " " + sBuildWindowsEx + "\n" + Environment.OSVersion + "\nSoftware: " + sVersionSoftware + "\nUser: " + Environment.UserName;
        }

        private void RunApp(string sRunApp, bool HidenWindow = false)
        {
            if (sRunApp.Length > 0)
            {
                if (sRunApp.ToLower().Contains("intellect") || sRunApp.ToLower().Contains("slave"))
                {
                    string sFilePath = "";
                    string sIntellectRegPath = @"SOFTWARE\Wow6432Node\ITV\INTELLECT";   //Check Intellect
                    using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sIntellectRegPath, Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                    {
                        try
                        {
                            sFilePath = EvUserKey.GetValue("InstallPath").ToString().Trim() + @"\" + EvUserKey.GetValue("core_module_name").ToString().Trim();
                        }
                        catch { }
                    }

                    if (sFilePath.Length == 0) //if any Intellect modules in memory did not find it searches in registry
                    {
                        sIntellectRegPath = @"SOFTWARE\ITV\INTELLECT";   //Check Intellect
                        using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sIntellectRegPath, Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                        {
                            try
                            {
                                sFilePath = EvUserKey.GetValue("InstallPath").ToString().Trim() + @"\" + EvUserKey.GetValue("core_module_name").ToString().Trim();
                            }
                            catch { }
                        }
                    }
                    try { System.Diagnostics.Process.Start(sFilePath); } catch { }
                    sFilePath = null; sIntellectRegPath = null;
                }
                else
                {
                    try
                    {
                        System.Diagnostics.Process pr = new System.Diagnostics.Process();
                        pr.StartInfo.FileName = sRunApp;
                        if (HidenWindow)
                        { pr.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden; }//for hidden run
                        pr.Start();
                    }
                    catch { }
                }
            }
        }

        private void KillApp(string sKillApp, string sOption)
        {
            string arguments;
            System.Diagnostics.Process pr = new System.Diagnostics.Process();
            if (sKillApp.Length > 0)
            {
                try
                {
                    pr = new System.Diagnostics.Process();
                    pr.StartInfo.UseShellExecute = false;
                    pr.StartInfo.CreateNoWindow = true;// Given that is is started without a window so you cannot terminate it on the desktop, it must terminate itself or you can do it programmatically from this application using the Kill method.
                    pr.StartInfo.FileName = "taskkill.exe";
                    arguments = " /F /IM " + sKillApp + " " + sOption;
                    pr.StartInfo.Arguments = arguments;
                    pr.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    pr.Start();
                }
                catch { }
                try
                {
                    pr = new System.Diagnostics.Process();
                    pr.StartInfo.UseShellExecute = false;
                    pr.StartInfo.CreateNoWindow = true;// Given that is is started without a window so you cannot terminate it on the desktop, it must terminate itself or you can do it programmatically from this application using the Kill method.
                    pr.StartInfo.FileName = "taskkill.exe";
                    arguments = " /F /IM " + sKillApp + "* " + sOption;
                    pr.StartInfo.Arguments = arguments;
                    pr.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    pr.Start();
                }
                catch { }
                try
                {
                    pr = new System.Diagnostics.Process();
                    pr.StartInfo.UseShellExecute = false;
                    pr.StartInfo.CreateNoWindow = true;// Given that is is started without a window so you cannot terminate it on the desktop, it must terminate itself or you can do it programmatically from this application using the Kill method.
                    pr.StartInfo.FileName = "taskkill.exe";
                    arguments = " /F /IM " + sKillApp + ".exe " + sOption;
                    pr.StartInfo.Arguments = arguments;
                    pr.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    pr.Start();
                }
                catch { }
                try
                {
                    pr = new System.Diagnostics.Process();
                    pr.StartInfo.UseShellExecute = false;
                    pr.StartInfo.CreateNoWindow = true;// Given that is is started without a window so you cannot terminate it on the desktop, it must terminate itself or you can do it programmatically from this application using the Kill method.
                    pr.StartInfo.FileName = "taskkill.exe";
                    arguments = " /F /IM " + sKillApp + "*.exe " + sOption;
                    pr.StartInfo.Arguments = arguments;
                    pr.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    pr.Start();
                }
                catch { }
                try
                {
                    pr = new System.Diagnostics.Process();
                    pr.StartInfo.UseShellExecute = false;
                    pr.StartInfo.CreateNoWindow = true;// Given that is is started without a window so you cannot terminate it on the desktop, it must terminate itself or you can do it programmatically from this application using the Kill method.
                    pr.StartInfo.FileName = "taskkill.exe";
                    arguments = " /F /IM " + sKillApp + ".com " + sOption;
                    pr.StartInfo.Arguments = arguments;
                    pr.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    pr.Start();
                }
                catch { }
                try
                {
                    pr = new System.Diagnostics.Process();
                    pr.StartInfo.UseShellExecute = false;
                    pr.StartInfo.CreateNoWindow = true;// Given that is is started without a window so you cannot terminate it on the desktop, it must terminate itself or you can do it programmatically from this application using the Kill method.
                    pr.StartInfo.FileName = "taskkill.exe";
                    arguments = " /F /IM " + sKillApp + "*.com " + sOption;
                    pr.StartInfo.Arguments = arguments;
                    pr.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    pr.Start();
                }
                catch { }
            }
            arguments = null; pr = null;
        }

        private void TaskList() //Need Administrator Privelegies - isElevated has to be true
        {
            string sResult = "";
            int iStringSend = 0;
            try
            {
                System.Diagnostics.Process pr = new System.Diagnostics.Process();
                System.Diagnostics.Process[] localAll = System.Diagnostics.Process.GetProcesses();

                foreach (System.Diagnostics.Process sTemp in localAll)
                {
                    if (sTemp.MainModule.FileName.ToLower().Contains(ApplicationName.ToLower()))
                    {
                        try
                        {
                            sResult += sTemp.MainModule.FileName + "|";
                            iStringSend++;
                        }
                        catch { sResult += " |"; }
                        try
                        {
                            sResult += sTemp.ProcessName + "|";
                        }
                        catch { sResult += " |"; }
                        try
                        {
                            sResult += sTemp.MainWindowTitle + "|";
                        }
                        catch { sResult += " |"; }
                        try
                        {
                            sResult += sTemp.Id + "|";
                        }
                        catch { sResult += " |"; }
                        sResult += "-InTheEnd-";
                    }
                }
                if (iStringSend == 0)
                { sResult += ApplicationName + " not found in the memory|-InTheEnd-"; }

                pr.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                pr.Start();
                pr.WaitForExit();
            }
            catch { }
        }

        private void RestartProcess(string sRestartApp)
        {
            string sPathCmd = "";
            string sFilePath = "";
            int iIdProcess = 999999;
            if (sRestartApp.Length > 0)
            {
                string sResult = "";
                try
                {
                    if (sRestartApp.ToLower() == "intellect" || sRestartApp.ToLower() == "slave" || sRestartApp.ToLower() == "video")
                    {
                        iIdProcess = 999999;

                        System.Diagnostics.Process[] localByName = System.Diagnostics.Process.GetProcessesByName("intellect");

                        foreach (System.Diagnostics.Process sTemp in localByName) //search only intellect(Server Module) in memory 
                        {
                            if (sTemp.MainModule.FileName.ToLower().Contains("intellect"))
                            {
                                try
                                {
                                    sFilePath = sTemp.MainModule.FileName;
                                    break;
                                }
                                catch { }
                            }
                        }
                        if (sFilePath.Length == 0) //if Intellect Server in memory did not found it tries to search Intellect-Client
                        {
                            localByName = System.Diagnostics.Process.GetProcessesByName("slave");

                            foreach (System.Diagnostics.Process sTemp in localByName)
                            {
                                if (sTemp.MainModule.FileName.ToLower().Contains("slave"))
                                {
                                    try
                                    {
                                        sFilePath = sTemp.MainModule.FileName;
                                        break;
                                    }
                                    catch { }
                                }
                            }
                        }
                        Thread.Sleep(10);
                        if (sFilePath.Length == 0) //if any Intellect modules in memory did not find it searches in registry
                        {
                            string sIntellectRegPath = @"SOFTWARE\Wow6432Node\ITV\INTELLECT";   //Check Intellect
                            using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sIntellectRegPath, Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                            {
                                string sTypeIntellect = "";
                                try
                                {
                                    sTypeIntellect = EvUserKey.GetValue("InstallType").ToString().ToLower().Trim();

                                    if (sTypeIntellect.Contains("client"))
                                    { sFilePath = EvUserKey.GetValue("InstallPath").ToString().Trim() + @"\slave.exe"; }
                                    else if (sTypeIntellect.Contains("server"))
                                    { sFilePath = EvUserKey.GetValue("InstallPath").ToString().Trim() + @"\" + EvUserKey.GetValue("core_module_name").ToString().Trim(); }
                                }
                                catch { }
                            }

                            if (sFilePath.Length == 0) //if any Intellect modules in memory did not find it searches in registry
                            {
                                sIntellectRegPath = @"SOFTWARE\ITV\INTELLECT";   //Check Intellect
                                using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sIntellectRegPath, Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                                {
                                    try
                                    {
                                        sFilePath = EvUserKey.GetValue("InstallPath").ToString().Trim() + @"\" + EvUserKey.GetValue("core_module_name").ToString().Trim();
                                    }
                                    catch { }
                                }
                                if (sFilePath.Length == 0)
                                { sResult += "Intellect did not find in the memory and on the Disk |-InTheEnd-"; }
                            }
                            sIntellectRegPath = null;
                        }
                        Thread.Sleep(10);
                        foreach (string sTempIntellect in sIntellect) //try to kill all intellect process
                        {
                            KillApp(sTempIntellect, " ");
                            Thread.Sleep(100);
                        }
                        Thread.Sleep(100);
                    }
                    else if ((sRestartApp.ToLower().Contains("myclientserver")))
                    {
                        System.Diagnostics.Process[] localByName = System.Diagnostics.Process.GetProcessesByName("myclientserver");
                        foreach (System.Diagnostics.Process sTemp in localByName)
                        {
                            if (sTemp.MainModule.FileName.ToLower().Contains(sRestartApp.ToLower()))
                            {
                                try
                                {
                                    sFilePath = sTemp.MainModule.FileName;
                                    sPathCmd = System.IO.Path.Combine(".", "myCltSvr.cmd");

                                    if (System.IO.File.Exists(sPathCmd))
                                    {
                                        try { System.IO.File.Delete(sPathCmd); } catch { }
                                        Thread.Sleep(500);
                                    }

                                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                                    sb.AppendLine("@echo off");
                                    sb.AppendLine("taskkill /f /im " + System.IO.Path.GetFileName(Application.ExecutablePath) + " > nul");
                                    sb.AppendLine("timeout /T 2 /NOBREAK > nul");
                                    sb.AppendLine("start " + Application.ExecutablePath);
                                    sb.AppendLine("exit");
                                    System.IO.File.WriteAllText(sPathCmd, sb.ToString(), System.Text.Encoding.GetEncoding(866));
                                    sb = null;
                                    break;
                                }
                                catch { }
                            }
                        }
                        Thread.Sleep(200);

                        try
                        {
                            System.Threading.Tasks.Task t1 = System.Threading.Tasks.Task.Run(() => RunProcess(sPathCmd));
                            t1.Wait(5000);
                            Form1.myForm._AppendTextToFile(Form1.myForm.FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Error: Can't run - " + sPathCmd);

                            // System.Threading.Thread clientThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart((obj) => RunProcess(sPathCmd)));
                            // clientThread.Start();
                            //  System.Windows.Forms.Application.Exit();
                        }
                        catch(Exception expt)
                        {
                            Form1.myForm._AppendTextToFile(Form1.myForm.FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Error: " + expt.Message);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Process[] localByName = System.Diagnostics.Process.GetProcessesByName(sRestartApp.ToLower());

                        foreach (System.Diagnostics.Process sTemp in localByName)
                        {
                            if (sTemp.MainModule.FileName.ToLower().Contains(sRestartApp.ToLower()))
                            {
                                iIdProcess = 999999;
                                try
                                {
                                    sFilePath = sTemp.MainModule.FileName;
                                    iIdProcess = sTemp.Id;
                                }
                                catch { }
                                if (iIdProcess != 999999)
                                {
                                    KillApp(ApplicationName, " /T ");
                                }
                            }
                        }
                        if (sFilePath.Length == 0)
                        { sResult += sRestartApp + " did not find in the memory|-InTheEnd-"; }
                    }
                }
                catch { }

                if (sFilePath.Length > 0)
                {
                    Thread.Sleep(500);
                    try { System.Diagnostics.Process.Start(sFilePath); } catch { }
                }
            }
            sFilePath = null; iIdProcess = 0;
        }

        protected internal void RunProcess(string sPathCmd)
        {
            System.Diagnostics.Process pr = new System.Diagnostics.Process();
            pr.StartInfo.FileName = sPathCmd;
            pr.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pr.Start();
        }        
    }
}
