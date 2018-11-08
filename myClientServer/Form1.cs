using System;
using System.Data;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
//using System.Runtime.InteropServices;
//using System.Security.Cryptography;
//using System.Text;
using System.Threading;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace myClientServer
{
    public partial class Form1 :Form
    {
        //----------------// Variable  //----------------// 
        public static Form1 myForm = null; // статичный экземляр. Half-Singleton. для доступа к форме из др. классов
        private System.Diagnostics.FileVersionInfo myFileVersionInfo;
        private string strVersion;

        private System.Drawing.Bitmap myLogo;
        private System.Drawing.Icon myIco;
        private ToolTip myToolTip = new ToolTip();
        protected internal string FileLog = System.IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".log";
        protected internal string FileLogMessages = System.IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".msg";

        protected internal string strFolderUpdate = System.IO.Path.Combine(Application.StartupPath, "UPDATESERVER");
        protected internal string strFolderGet = System.IO.Path.Combine(Application.StartupPath, "GET");
        protected internal string strFolderSend = System.IO.Path.Combine(Application.StartupPath, "SEND");
        protected internal string strFolderUpdates = "";
        private bool FolderUpdateCreated = false;
        private bool FolderGetCreated = false;
        private bool FolderSendCreated = false;

        protected internal string myRegKey = @"SOFTWARE\RYIK\ClientServerCommunicator2";
        protected internal string modesWindow = "true|true|control";  //Режим отображения окна программы
        protected internal bool maximize = true;
        protected internal bool hiden = false;
        protected internal string controlled = "control";

        protected internal string selectedAction = "";
        protected internal string sFilePath = "";

        public ServerObject server = new ServerObject(); // сервер
        private Thread listenThread; // поток для прослушивания
        private TcpClient client = new TcpClient();
        //private NetworkStream stream;

        protected internal EndPointHost LocalHost = new EndPointHost();
        protected internal EndPointHost RemoteHost = new EndPointHost();
        private bool StopClient = false; //Stop working Client

        //Actions
        private System.Collections.Generic.List<string> sArActionName = new System.Collections.Generic.List<string>
        {
            "Перезапустить ПО",
            "Перезапустить Intellect",
            "Обновить ПО",
            "Очистить логи удаленного хоста",
            "Очистить логи локального хоста",
            "Идентификация хоста",
            "Сделать скриншот Рабочего стола",
            "Вывести окно",
            "Скрыть окно",
            "Включить скрытый режим",
            "Выключить скрытый режим",
            "Неуправляемый текстовый режим",
            "Неуправляемый проверочный режим",
            "Управляемый режим",
        };

        private System.Collections.Generic.List<string> sArAction = new System.Collections.Generic.List<string>
        {
            "restart myclientserver",
            "restart intellect",
            "updateserver",
            "clear log server",
            "clear log client",
            "name server",
            "take screenshot",
            "mode max server",
            "mode min server",
            "mode disapear server",
            "mode apear server",
            "mode uncontroltext server",
            "mode uncontrolcheck server",
            "mode control server",
        };

        //Crypt communication between server and client
        private readonly byte[] btsMess1 = Convert.FromBase64String(@"OCvesunvXXsxtt381jr7vp3+UCwDbE4ebdiL1uinGi0="); //Key Encrypt
        private readonly byte[] btsMess2 = Convert.FromBase64String(@"NO6GC6Zjl934Eh8MAJWuKQ=="); //Key Decrypt
        private EncryptDecrypt encryptDecrypt = new EncryptDecrypt();
        //After will finish the programm REMOVE this code from programm into safety place!!!!
        private string salt = "PasswordSimple!";

        //context menu for notification icon at taskbar
        private System.Windows.Forms.ContextMenu contextMenu1;

        //----------------//   Disable closing X. Start of Block
        const int MF_BYPOSITION = 0x400; //Declare the following as class level variable
        [System.Runtime.InteropServices.DllImport("User32")]
        private static extern int RemoveMenu(IntPtr hMenu, int nPosition, int wFlags);
        [System.Runtime.InteropServices.DllImport("User32")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [System.Runtime.InteropServices.DllImport("User32")]
        private static extern int GetMenuItemCount(IntPtr hWnd);
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }
        //----------------//   Disable closing X. End of Block

        //----------------// Variable //----------------// 



        //----------------//  Form1. Start Block //----------------// 
        public Form1()
        {
            InitializeComponent();
            TopMost = true;
            myForm = this; // для доступа к этой форме из др. классов

            //Hide-Show Form on Double Click
            notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new MouseEventHandler(notifyIcon1_MouseDoubleClick);
            this.Resize += new System.EventHandler(this.Form1_Resize);

            //Local Host            
            LocalHost = new EndPointHost();
            try
            {
                System.Net.NetworkInformation.NetworkInterface[] networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                foreach (System.Net.NetworkInformation.NetworkInterface network in networkInterfaces)
                {
                    System.Net.NetworkInformation.IPInterfaceProperties properties = network.GetIPProperties();  // Read the IP configuration for each network                  
                    foreach (System.Net.NetworkInformation.IPAddressInformation address in properties.UnicastAddresses)   // Each network interface may have multiple IP addresses 
                    {
                        // We're only interested in IPv4 addresses for now  and Ignore loopback addresses (e.g., 127.0.0.1) 
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address.Address) &&
                            (address.Address.ToString().StartsWith("192.168.") || address.Address.ToString().StartsWith("172.") || address.Address.ToString().StartsWith("10.")))
                        {
                            LocalHost.ipAddress = address.Address;
                            break;
                        }
                    }
                }
            } catch { }
            try { LocalHost.HostName = Dns.GetHostEntry(Environment.MachineName); } catch { }
            LocalHost.intPortChat = _numericUpDownClien();
            LocalHost.SetInfo();

            //OpenDialogForm
            openFileDialog1.InitialDirectory = Application.StartupPath;
            openFileDialog1.Title = "Выберите файл для отправки на сервер.";
            openFileDialog1.Filter = "Текстовые файлы (*.txt)|*.txt|Exe файлы (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
        }



        private async void Form1_Load(object sender, EventArgs e)
        {
            panelOpenPorts.Visible = false;
            richTextBoxEcho.ReadOnly = true;
            dgView.ReadOnly = true;
            panelRichTextBox.Visible = true;
            ShowInTaskbar = false;      //Hide the form window for taskbasr

            Icon = Properties.Resources.RYIKico; //my Icon
            myIco = Properties.Resources.RYIKico; //my Icon
            myLogo = new System.Drawing.Bitmap(Properties.Resources.RYIKpng); //my Logo
            myToolTip.SetToolTip(buttonSearchPort, "Scan opennig ports of the Remote Server");
            myToolTip.SetToolTip(textBoxClientIp, "IP address of the Remote Server");
            myToolTip.SetToolTip(buttonServer, "Set local Host as the Server");
            myToolTip.SetToolTip(numericUpDownClient, "Port of the Remote Server");
            StatusLabel2.Text = "";
            StatusLabel1.Alignment = ToolStripItemAlignment.Right;

            myFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(Application.ExecutablePath);
            strVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            StatusLabel1.Text = myFileVersionInfo.Comments + " ver." + myFileVersionInfo.FileVersion + " " + myFileVersionInfo.LegalCopyright;

            RegistryDataCheck();

            comboBoxAction.DataSource = sArActionName;

            //contextMenu in TaskrBar
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            notifyIcon1.ContextMenu = contextMenu1;
            contextMenu1.MenuItems.Add("Hide", Hide_Click);
            contextMenu1.MenuItems.Add("Show", Show_Click);
            contextMenu1.MenuItems.Add("About", About_Click);
            contextMenu1.MenuItems.Add("Exit", Form1_FormClose);
            notifyIcon1.Icon = Properties.Resources.RYIKico; //show icon in tray
            notifyIcon1.Text = myFileVersionInfo.Comments;
            notifyIcon1.BalloonTipText = myFileVersionInfo.LegalCopyright;
            this.Text = notifyIcon1.Text;


            //Disable closing by X
            IntPtr hMenu = GetSystemMenu(this.Handle, false);
            int menuItemCount = GetMenuItemCount(hMenu);
            RemoveMenu(hMenu, menuItemCount - 1, MF_BYPOSITION);


            //Make Folders
            System.Threading.Tasks.Task.Run(() => MakeFolder("UPDATESERVER", FolderUpdateCreated, true));
            System.Threading.Tasks.Task.Run(() => MakeFolder("GET", FolderGetCreated));
            System.Threading.Tasks.Task.Run(() => MakeFolder("SEND", FolderSendCreated, true));


            //Check running servers
            bool existed;
            string guid = System.Runtime.InteropServices.Marshal.GetTypeLibGuidForAssembly(System.Reflection.Assembly.GetExecutingAssembly()).ToString(); // получаем GIUD приложения
            Mutex mutexObj = new Mutex(true, guid, out existed);   //Проверка приложения на запуск 1 раз за раз
            if (existed)
            {
                //TODO if block other run
                //       MessageBox.Show("GUID приложения:\n" + guid + "\nПриложение");
            }
            else
            {
                //         MessageBox.Show("GUID приложения:\n" + guid + "\nПриложение.");
                return;
            }



            ChangeModeWindow(modesWindow);  //Change mode CSC

            ReadLogFileToRichTextbox(FileLogMessages);    //Read previous log file of messages
            ReadFileLog(FileLog);    //Read previous log file
            System.Threading.Tasks.Task.Run(() => ServerStart());      //Run the server part of CSC

            //Make DataTable 
            resultChecking.Columns.AddRange(dcCheck);

            // Make all columns required.
            // for (int i = 0; i < resultChecking.Columns.Count; i++)
            // { resultChecking.Columns[i].AllowDBNull = false; }

            // Make IP + Name require uniqueness.
            DataColumn[] unique_cols =
             {
                resultChecking.Columns["IP"],
                resultChecking.Columns["Name"]
             };
            resultChecking.Constraints.Add(new UniqueConstraint(unique_cols));

            //Check IPs from file-list
            await System.Threading.Tasks.Task.Run(() => CheckIpContinueWithPause(600000, false)); //pause is 10 min
        }
        //----------------// Form1. End Block //----------------// 



        //----------------//  Main. Start Block //----------------// 
        private async void CheckIpContinueWithPause(int pauseInMs, bool StopWait = false)
        {
            do
            {
                CheckList();
                await System.Threading.Tasks.Task.Delay(pauseInMs);
            }
            while (!StopWait);
        }

        private void ServerStart()
        {
            _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + LocalHost.UserName + ": Server started!");

            try
            {
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start(); //старт потока    
            } catch (Exception ex)
            {
                _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + LocalHost.UserName + ": " + ex.Message);
            }
        }

        private async void numericUpDownClient_ValueChanged(object sender, EventArgs e)
        { await System.Threading.Tasks.Task.Run(() => CheckValidRemoteIPName(_textBoxReturnText(textBoxClientIp), _numericUpDownClien())); }

        private async void textBoxClientIp_Leave(object sender, EventArgs e)
        { await System.Threading.Tasks.Task.Run(() => CheckValidRemoteIPName(_textBoxReturnText(textBoxClientIp), _numericUpDownClien())); }

        private void CheckValidRemoteIPName(string strIPName, int intPort)
        {
            _ControlEnable(buttonAction, false);
            _ControlEnable(textBoxMessage, false);

            RemoteHost = new EndPointHost();
            try { RemoteHost.HostName = Dns.GetHostEntry(strIPName); } catch { }
            try { RemoteHost.ipAddress = IPAddress.Parse(strIPName); } catch
            {
                try
                {
                    IPAddress[] addresslist = Dns.GetHostAddresses(strIPName);
                    foreach (IPAddress address in addresslist)
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address) &&
                                    (address.ToString().StartsWith("192.168.") || address.ToString().StartsWith("172.") || address.ToString().StartsWith("10.")))
                        {
                            RemoteHost.ipAddress = address;
                            break;
                        }
                    }
                } catch { }
            }
            try { RemoteHost.intPortChat = intPort; } catch { }   //take opened port for getting of the file in receive's server
            RemoteHost.SetInfo();
            RegistrySaveServer();
            _ControlEnable(buttonAction, true);
            _ControlEnable(textBoxMessage, true);
            _ControlSelect(textBoxMessage);
        }

        private void CheckAndParseValidRemoteIPName(string message, int Port = 1200)
        {
            RemoteHost = new EndPointHost();
            //27.12.2017 23:33: SB-315-18.CORP.AIS(10.0.32.1) updateserver

            try { RemoteHost.HostName = Dns.GetHostEntry((message.Split(':')[2]).Split('(')[0].Trim()); } catch { }
            try { RemoteHost.ipAddress = IPAddress.Parse((message.Split('(')[1]).Split(')')[0].Trim()); } catch
            {
                try
                {
                    IPAddress[] addresslist = Dns.GetHostAddresses((message.Split('(')[1]).Split(')')[0].Trim());
                    foreach (IPAddress address in addresslist)
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address) &&
                                    (address.ToString().StartsWith("192.168.") || address.ToString().StartsWith("172.") || address.ToString().StartsWith("10.")))
                        {
                            RemoteHost.ipAddress = address;
                            break;
                        }
                    }
                } catch { }
            }
            try { RemoteHost.intPortChat = Port; } catch { }   //take opened port for getting of the file in receive's server
            RemoteHost.SetInfo();
            RegistrySaveServer();
        }

        private AutoResetEvent aResEvntStopServer = new AutoResetEvent(false);

        private void buttonStopRun_Click(object sender, EventArgs e)
        {
            if (StopClient)
            {
                StopClient = false;
                aResEvntStopServer.Set();
            }
            else
            {
                StopClient = true;
                aResEvntStopServer.Set();
            }
        }

        private void ClientStartThread() //start  ClientStart() in the separated thread
        {
            if (!StopClient)
            {
                System.Threading.Tasks.Task t2 = System.Threading.Tasks.Task.Run(() =>
               ClientStart()
               );
            }
            else
            {
                StopClient = true;
                aResEvntStopServer.Set();
            }
        }

        private async void ClientStart()
        {
            string messagePlain = "";
            string messageCrypted = "";
            byte[] msgByte;

            try
            {
                using (var tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(RemoteHost.ipAddress.ToString(), RemoteHost.intPortChat);
                    using (var networkStream = tcpClient.GetStream())
                    {
                        //send name
                        messagePlain = LocalHost.UserName + ":" + LocalHost.intPortChat;
                        messageCrypted = encryptDecrypt.EncryptRijndael(messagePlain, salt);
                        msgByte = System.Text.Encoding.UTF8.GetBytes(messageCrypted);
                        await networkStream.WriteAsync(msgByte, 0, msgByte.Length);

                        msgByte = new byte[4096];
                        int byteCount = await networkStream.ReadAsync(msgByte, 0, msgByte.Length);
                        messageCrypted = System.Text.Encoding.UTF8.GetString(msgByte, 0, byteCount);
                        if (messageCrypted.Length == 0) messagePlain = "Проверка связи!";
                        else { messagePlain = encryptDecrypt.DecryptRijndael(messageCrypted, salt); }
                        //  _richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + RemoteHost.UserName + ": " + messagePlain);  //вывод сообщения

                        do
                        {
                            selectedAction = "";
                            //send text data
                            if (_textBoxReturnText(textBoxMessage).Length > 0)
                            {
                                messagePlain = _textBoxReturnText(textBoxMessage);
                                selectedAction = messagePlain;
                            }
                            else messagePlain = "Проверка связи!";
                            messageCrypted = encryptDecrypt.EncryptRijndael(messagePlain, salt);
                            msgByte = System.Text.Encoding.UTF8.GetBytes(messageCrypted);
                            await networkStream.WriteAsync(msgByte, 0, msgByte.Length);

                            msgByte = new byte[4096];
                            byteCount = await networkStream.ReadAsync(msgByte, 0, msgByte.Length);
                            messageCrypted = System.Text.Encoding.UTF8.GetString(msgByte, 0, byteCount);
                            //if (messageCrypted.Length == 0) messagePlain = "Проверка связи!";
                            if (messageCrypted.Length > 0) { messagePlain = encryptDecrypt.DecryptRijndael(messageCrypted, salt); }
                            if (messageCrypted.Length > 0)
                            { _richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + RemoteHost.UserName + ": " + messagePlain); }

                            aResEvntStopServer.Reset();
                            if (messagePlain.ToLower().Contains("ready to get a file on the port")) //Send file if Remote server is ready
                            {
                                if (selectedAction == "updateserver")  //Action is Updateserver
                                {
                                    //Copy main application to the updating folder with name "UPDATESERVER"
                                    sFilePath = System.IO.Path.Combine(strFolderUpdate, System.IO.Path.GetFileName(Application.ExecutablePath));
                                    try { System.IO.File.Copy(Application.ExecutablePath, sFilePath); } catch { }
                                }
                                else if (selectedAction.ToLower() == "get file") //Action with sending of file to server
                                { _OpenFileDialogResult(); } //Make dialog open file

                                //take name of Remote's server
                                try { RemoteHost.HostName = Dns.GetHostEntry((messagePlain.Split('(')[1]).Split(')')[0].Trim()); } catch { }
                                RemoteHost.ipAddress = IPAddress.Parse((messagePlain.Split('(')[1]).Split(')')[0].Trim());
                                RemoteHost.intPortGetFile = Convert.ToInt32(messagePlain.Split(':')[2].Trim());//take opened port for getting of the file in receive's server
                                RemoteHost.SetInfo();
                                //Send updating file
                                SendGetFile sendFile = new SendGetFile();
                                try
                                {
                                    if (sFilePath.Length == 0)
                                    {
                                        Random rnd = new Random();
                                        try { System.IO.File.Create(rnd.Next().ToString() + ".top"); } catch { }
                                    }
                                    sendFile.SendFile(RemoteHost.ipAddress.ToString(), RemoteHost.intPortGetFile, sFilePath);
                                    _richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + RemoteHost.UserName + " : is updating...");
                                } catch (Exception expt)
                                { _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + RemoteHost.UserName + ": Erorr: " + expt.ToString()); }//соединение было прервано
                                sendFile = null;
                            }
                            _textBoxClear(textBoxMessage);
                            aResEvntStopServer.WaitOne();
                        } while (!StopClient);

                        _richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + RemoteHost.UserName + ": " + messagePlain);
                        _textBoxClear(textBoxMessage);
                    }
                }
            } catch (Exception ex)
            { _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + LocalHost.UserName + ": " + ex.Message); }
        }

        private async void buttonSearchPort_Click(object sender, EventArgs e)
        { await System.Threading.Tasks.Task.Run(() => SearchPort()); }

        private async void SearchPort() //Search Open Ports
        {
            try { RemoteHost.HostName = Dns.GetHostEntry(_textBoxReturnText(textBoxClientIp)); } catch { }
            try { RemoteHost.ipAddress = IPAddress.Parse(_textBoxReturnText(textBoxClientIp)); } catch { }
            RemoteHost.intPortChat = _numericUpDownClien();
            RemoteHost.SetInfo();

            string strRemoteName = "";
            if (RemoteHost.ipAddress != null) strRemoteName = RemoteHost.ipAddress.ToString();
            else if (RemoteHost.HostName != null) strRemoteName = RemoteHost.HostName.HostName.ToString().ToUpper();
            else strRemoteName = "";
            if (strRemoteName.Length > 0)
            {
                try
                {
                    using (var tcpClient = new TcpClient())
                    {
                        await tcpClient.ConnectAsync(_textBoxReturnText(textBoxClientIp), RemoteHost.intPortChat);
                        using (var networkStream = tcpClient.GetStream())
                        {
                            byte[] msg = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael(LocalHost.UserName + ": test port!", salt));
                            await networkStream.WriteAsync(msg, 0, msg.Length);
                            _richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " +
                                    RemoteHost.UserName + ": " + "Open port: " + RemoteHost.intPortChat);
                        }
                    }
                } catch (Exception ex)
                {
                    _richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + ex.Message);
                }
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (ShowInTaskbar == true)
            {
                ShowInTaskbar = false;
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                this.Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (ShowInTaskbar == true)
            {
                ShowInTaskbar = false;
                WindowState = FormWindowState.Minimized;
            }
            else
            {
                this.Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            { Hide(); }
        }

        private void Show_Click(object sender, EventArgs e)
        { _ShowForm(true); }

        private void Hide_Click(object sender, EventArgs e)
        { _ShowForm(false); }

        protected internal void ChangeModeWindow(string Mode)
        {
            string[] mode = Mode.Split('|');
            if (mode.Length == 3)
            {
                maximize = Convert.ToBoolean(mode[0]);
                hiden = Convert.ToBoolean(mode[1]);
                controlled = mode[2];
            }

            _ShowForm(maximize);
            _UnSecretMode(hiden);
            _UnControlMode(controlled);

            modesWindow = maximize.ToString() + "|" + hiden.ToString() + "|" + controlled;
            RegistrySaveMode(modesWindow);
            _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Try to set Mode: " + modesWindow);
        }

        private void About_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                                myFileVersionInfo.Comments + "\n\nВерсия: " + myFileVersionInfo.FileVersion + "\nBuild: " +
                strVersion + "\n" + myFileVersionInfo.LegalCopyright, "About", MessageBoxButtons.OK, MessageBoxIcon.Information
                );
        }

        protected internal void RegistryDataCheck() //Read previously Saved inputed Credintials and Parameters from Registry
        {
            RemoteHost = new EndPointHost();
            string strServer = "";
            try
            {
                using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                      myRegKey,
                      Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree,
                      System.Security.AccessControl.RegistryRights.ReadKey))
                {
                    strServer = encryptDecrypt.DecryptBase64ToString(EvUserKey.GetValue("RemoteServerName").ToString(), btsMess1, btsMess2);
                }
            } catch { }

            try
            {
                using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                      myRegKey,
                      Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree,
                      System.Security.AccessControl.RegistryRights.ReadKey))
                {
                    string Mode = encryptDecrypt.DecryptBase64ToString(EvUserKey.GetValue("MODE").ToString(), btsMess1, btsMess2);
                    string[] mode = Mode.Split('|');
                    if (mode.Length == 3)
                    {
                        modesWindow = Mode;
                        maximize = Convert.ToBoolean(mode[0]);
                        hiden = Convert.ToBoolean(mode[1]);
                        controlled = mode[2];
                    }
                }
            } catch { }

            if (strServer.Length > 0)
            {
                try { RemoteHost.HostName = Dns.GetHostEntry(strServer); } catch { }
                try { RemoteHost.ipAddress = IPAddress.Parse(strServer); } catch
                {
                    try
                    {
                        IPAddress[] addresslist = Dns.GetHostAddresses(strServer);
                        foreach (IPAddress address in addresslist)
                        {
                            if (address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address) &&
                                        (address.ToString().StartsWith("192.168.") || address.ToString().StartsWith("172.") || address.ToString().StartsWith("10.")))
                            {
                                RemoteHost.ipAddress = address;
                                break;
                            }
                        }
                    } catch { }
                }
                RemoteHost.SetInfo();
            }

            try
            {
                if (RemoteHost.ipAddress != null) { _ControlAddText(textBoxClientIp, RemoteHost.ipAddress.ToString()); }
                else if (RemoteHost.HostName != null) { _ControlAddText(textBoxClientIp, RemoteHost.HostName.HostName.ToString().ToUpper()); }
            } catch { }
            //Delete previously created Folder "AutoUpdate" and "myCltSvr.cmd"
            try
            {
                using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(myRegKey, Microsoft.Win32.RegistryKeyPermissionCheck.ReadSubTree, System.Security.AccessControl.RegistryRights.ReadKey))
                {
                    strFolderUpdates = encryptDecrypt.DecryptBase64ToString(EvUserKey.GetValue("UPDATESERVERFOLDER").ToString(), btsMess1, btsMess2);
                }

                if (strFolderUpdates.Length > 1)
                {
                    if (System.IO.Directory.Exists(strFolderUpdates))
                    {
                        System.IO.Directory.Delete(strFolderUpdates, true);
                        using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(myRegKey))
                        { EvUserKey.SetValue("UPDATESERVERFOLDER", encryptDecrypt.EncryptStringToBase64Text("0", btsMess1, btsMess2), Microsoft.Win32.RegistryValueKind.String); }

                    }
                }
            } catch { }

            if (System.IO.File.Exists(System.IO.Path.Combine(".", "myCltSvr.cmd")))
            { try { System.IO.File.Delete(System.IO.Path.Combine(".", "myCltSvr.cmd")); } catch { } }
        }

        protected internal void RegistrySaveServer() //Save Parameters into Registry and variables
        {
            string RemoteServerName = _textBoxReturnText(textBoxClientIp);

            if (RemoteServerName != null && RemoteServerName.Length > 0)
            {
                try
                {
                    using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(myRegKey))
                    {
                        EvUserKey.SetValue("RemoteServerName", encryptDecrypt.EncryptStringToBase64Text(
                            RemoteServerName, btsMess1, btsMess2),
                            Microsoft.Win32.RegistryValueKind.String); //  EvUserKey.SetValue("FolderPhotos", sFolderPhotos, Microsoft.Win32.RegistryValueKind.String);
                    }
                } catch { }
            }
        }

        protected internal void RegistrySaveMode(string stringmodesWindow) //Save inputed Credintials and Parameters into Registry and variables
        {
            try
            {
                using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(myRegKey))
                {
                    EvUserKey.SetValue("MODE", encryptDecrypt.EncryptStringToBase64Text(
                        stringmodesWindow, btsMess1, btsMess2),
                        Microsoft.Win32.RegistryValueKind.String); //  EvUserKey.SetValue("FolderPhotos", sFolderPhotos, Microsoft.Win32.RegistryValueKind.String);
                }
            } catch { }
        }

        protected internal void RegistryDataSave(string stringmodesWindow)
        {
            RegistrySaveServer();
            RegistrySaveMode(modesWindow);
        }

        private void buttonSwitchPanels_Click(object sender, EventArgs e) //Call SwitchPanels()
        { ModePanels(); }

        private int iPanelNumber = 0;

        private void ModePanels()
        {
            ModePanel(iPanelNumber);
            iPanelNumber++;
            if (iPanelNumber > 2) iPanelNumber = 0;
        }

        protected internal async void ModePanel(int i)
        {
            System.Collections.ArrayList Empty = new System.Collections.ArrayList();
            if (i == 0)
            {
                dgView.DataSource = Empty;

                panelOpenPorts.Visible = false;

                panelRichTextBox.BringToFront();
                panelRichTextBox.Visible = true;
            }
            else if (i == 1)
            {
                panelRichTextBox.Visible = false;
                System.Collections.Generic.List<PortInfo> pi = GetOpenPort();

                dgView.DataSource = Empty;
                dgView.DataSource = pi;
                dgView.BringToFront();
                dgView.Refresh();

                panelOpenPorts.Visible = true;
                panelOpenPorts.BringToFront();
            }
            else if (i == 2)
            {
                panelRichTextBox.Visible = false;
                dgView.DataSource = Empty;
                dgView.DataSource = resultChecking;
                if (_ShowFormMaximize() == true)
                { await System.Threading.Tasks.Task.Run(() => ChangeDatagridCellsColor()); }
                dgView.BringToFront();
                dgView.Refresh();

                panelOpenPorts.Visible = true;
                panelOpenPorts.BringToFront();
            }
        }

        private System.Collections.Generic.List<PortInfo> GetOpenPort()
        {
            System.Net.NetworkInformation.IPGlobalProperties properties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();
            System.Net.NetworkInformation.TcpConnectionInformation[] tcpConnections = properties.GetActiveTcpConnections();

            return tcpConnections.Select(p =>
            {
                return new PortInfo(
                    i: p.LocalEndPoint.Port,
                    local: p.LocalEndPoint.Address + ":" + p.LocalEndPoint.Port,
                    remote: p.RemoteEndPoint.Address + ":" + p.RemoteEndPoint.Port,
                    state: p.State.ToString());
            }).ToList();
        }

        private DataTable resultChecking = new DataTable("ResultChecked");

        private DataColumn[] dcCheck ={
                              //    new DataColumn("iD",typeof(double)),
                                  new DataColumn("Time",typeof(string)),
                                  new DataColumn("IP",typeof(string)),
                                  new DataColumn("Name",typeof(string)),
                                  new DataColumn("Group",typeof(string)),
                                  new DataColumn("Ping",typeof(string)),
                                  new DataColumn("Check Port",typeof(string)),
                                  new DataColumn("Port",typeof(string)),
                              };
        //resultChecking.Rows[rowCurrent][0] = checkStatus.hostCheckTime;
        //resultChecking.Rows[rowCurrent][3] = checkStatus.hostGroup;
        //resultChecking.Rows[rowCurrent][4] = checkStatus.ResultPing;
        //resultChecking.Rows[rowCurrent][5] = checkStatus.ResultCheckPort;
        //resultChecking.Rows[rowCurrent][6] = checkStatus.hostPort.ToString();

        private CheckStatusHost checkStatus = new CheckStatusHost();

        private System.Collections.Generic.List<string> sListRows = new System.Collections.Generic.List<string>();

        private async void CheckList()
        {
            string sPathCmd = System.IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath) + ".lst"; //list with ip
            int iAllRowsInFile = 0;

            sListRows = new System.Collections.Generic.List<string>();
            try
            {
                if (System.IO.File.Exists(sPathCmd))
                {
                    foreach (string st in System.IO.File.ReadAllLines(sPathCmd, System.Text.Encoding.GetEncoding(1251)))
                    {
                        if (st != null && st.Trim().Length > 0 && st.Contains('|'))
                        {
                            sListRows.Add(st.Trim());
                            iAllRowsInFile++;
                        }
                        if (iAllRowsInFile > 100)
                        { break; }
                    }

                    if (iAllRowsInFile > 1)
                    {
                        int iIP = -1;
                        int iName = -1;
                        int iGroup = -1;
                        int iPort = -1;

                        string[] sCell = (sListRows.ToArray()[0]).Split('|');
                        int iCells = (sListRows.ToArray()[0]).Split('|').Length;
                        for (int iCell = 0; iCell < iCells; iCell++)
                        {
                            string row = sCell[iCell].ToLower().Trim();

                            if (row == "ip") iIP = iCell;
                            else if (row == "name") iName = iCell;
                            else if (row == "group") iGroup = iCell;
                            else if (row == "port") iPort = iCell;
                        }
                        int iD = 0;
                        if (iIP > -1 || iName > -1)
                        {
                            foreach (string st in sListRows.ToArray())
                            {
                                if (iD > 0)
                                {
                                    await System.Threading.Tasks.Task.Run(() => ParseRowCheckStatusHost(iD, iIP, iName, iGroup, iPort, st));
                                }
                                await System.Threading.Tasks.Task.Delay(500);//Make a Pause 0,5s
                                iD++;
                            }
                        }
                    }
                }
            } catch (Exception expt)
            {
                _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": CheckList() " + ": " + expt.Message);
            }
            if (_ShowFormMaximize() == true)
            { await System.Threading.Tasks.Task.Run(() => ChangeDatagridCellsColor()); }
        }

        private void ParseRowCheckStatusHost(int iD, int iIP, int iName, int iGroup, int iPort, string st)
        {
            try
            {
                string[] rowCell = st.Split('|');
                checkStatus = new CheckStatusHost();
                if (iIP > -1) checkStatus.hostIP = rowCell[iIP].Trim();
                if (iName > -1) checkStatus.hostName = rowCell[iName].Trim();
                if (iGroup > -1) checkStatus.hostGroup = rowCell[iGroup].Trim();
                try { if (iPort > -1) checkStatus.hostPort = Convert.ToInt32(rowCell[iPort].Trim()); } catch { }

                checkStatus.GetStatusHost();
                System.Threading.Tasks.Task.Delay(1000); //Make a Pause 1s

                int rowCurrent = -1;
                try
                {
                    for (int i = 0; i < resultChecking.Rows.Count; i++)
                    {
                        try
                        {
                            if (checkStatus.hostIP != null && resultChecking.Rows[i][1] != null && checkStatus.hostIP == resultChecking.Rows[i][1].ToString())
                            {
                                rowCurrent = i;  //if hostIP esist in the table
                                break;
                            }
                        } catch { }
                    }
                } catch { }

                if (checkStatus.hostIP != null && checkStatus.hostIP.Length > 3)
                {
                    DataRow row = resultChecking.NewRow();

                    if (rowCurrent > -1)
                    {
                        row["Time"] = checkStatus.hostCheckTime;
                        row["Ping"] = checkStatus.ResultPing;
                        row["Check Port"] = checkStatus.ResultCheckPort;
                        row["Port"] = checkStatus.hostPort.ToString();
                        if (checkStatus.hostIP.Length > 6)
                        { resultChecking.Rows.Add(row); }
                    }
                    else
                    {
                        // row["iD"] = iD;
                        row["Time"] = checkStatus.hostCheckTime;
                        row["IP"] = checkStatus.hostIP;
                        row["Name"] = checkStatus.hostName;
                        row["Group"] = checkStatus.hostGroup;
                        row["Ping"] = checkStatus.ResultPing;
                        row["Check Port"] = checkStatus.ResultCheckPort;
                        row["Port"] = checkStatus.hostPort.ToString();
                        if (checkStatus.hostIP.Length > 6)
                        { resultChecking.Rows.Add(row); }
                    }
                }
            } catch //(Exception expt)
            {
                //             _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": iD " + iD + ": IP " + iIP + ": " + expt.Message);
            }
            checkStatus = new CheckStatusHost();
        }

        private void buttonServer_Click(object sender, EventArgs e) //Use buttonServerStart()
        { ServerStart(); }

        private void textBoxMessage_KeyDown(object sender, KeyEventArgs e)  //Use  ClientStartThread()
        {
            if (e.KeyCode == Keys.Enter)
            { ClientStartThread(); }
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => AppendTextToFile(FileLog,
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Application Exit"
                ));
            //_AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Application Exit");
            RegistryDataSave(modesWindow);
            //  GC.Collect();
            notifyIcon1.Visible = false;
            Application.Exit();
            //   Environment.Exit(0);
        }

        private async void Form1_FormClose(object sender, EventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => AppendTextToFile(FileLog,
                DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Application Exit"
                ));
            //_AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Application Exit");

            RegistryDataSave(modesWindow);
            //  GC.Collect();
            notifyIcon1.Visible = false;
            Application.Exit();
            //   Environment.Exit(0);
        }

        private void textBoxMessage_Click(object sender, EventArgs e)
        { _textBoxClear(textBoxMessage); }

        private void AppendTextToFile(string sFileLog, string sText)
        {
            System.Collections.Generic.List<string> sListRows = new System.Collections.Generic.List<string>();
            sListRows.Add(sText);
            try { System.IO.File.AppendAllLines(sFileLog, sListRows, System.Text.Encoding.GetEncoding(1251)); } catch { }
            sListRows = null;
        }

        protected internal async void _AppendTextToFile(string sFileLog, string sText)
        {
            await System.Threading.Tasks.Task.Run(() => AppendTextToFile(sFileLog, sText));
        }

        protected internal void ReadFileLog(string sFileLog) //import local saved log into RichTextBox and check Updating
        {
            string sPathCmd = sFileLog;
            string sTemp = "";
            bool bServerUpdated = false;
            bool bClientExist = false;
            int iAllRowsInLog = 0;
            System.Collections.Generic.List<string> sListRows = new System.Collections.Generic.List<string>();
            try
            {
                if (System.IO.File.Exists(sPathCmd))
                {
                    foreach (string st in System.IO.File.ReadAllLines(sPathCmd, System.Text.Encoding.GetEncoding(1251)))
                    {
                        if (st != null && st.Trim().Length > 0)
                            sListRows.Add(st.Trim()); iAllRowsInLog++;
                    }
                }
            } catch { }

            string appName = System.IO.Path.GetFileName(System.Windows.Forms.Application.ExecutablePath).ToLower();
            int intLastRow = 0;
            int list = sListRows.ToArray().Length;
            if ((list - 15) <= 0)
            { intLastRow = 15 - list; }
            else
            { intLastRow = list - 15; }

            if (iAllRowsInLog > 2)
            {
                for (int k = list; k > intLastRow; k--) //will check only last 15 raws
                {
                    try
                    {
                        sTemp = sListRows.ToArray()[k];
                        if (sTemp.ToLower().Contains("принят файл") && sTemp.ToLower().Contains(appName))
                        { bServerUpdated = true; continue; }
                        else if (sTemp.Contains("updateserver") && sTemp.Contains(')'))
                        {
                            CheckAndParseValidRemoteIPName(sTemp);
                            _ControlAddText(textBoxClientIp, RemoteHost.ipAddress.ToString());
                            _ControlAddText(textBoxMessage, "Updating finished!");
                            bClientExist = true;
                            continue;
                        }
                        else if (sTemp.Contains("restart myclientserver") && sTemp.Contains(')'))
                        {
                            CheckAndParseValidRemoteIPName(sTemp);
                            _ControlAddText(textBoxClientIp, RemoteHost.ipAddress.ToString());
                            _ControlAddText(textBoxMessage, "Restarted!");
                            ClientStartThread();
                            break;
                        }

                        if (bServerUpdated && bClientExist)
                        {
                            ClientStartThread();
                            break;
                        }
                    } catch { }
                }
            }
            sTemp = null;
            sListRows = null;
        }

        protected internal void ReadLogFileToRichTextbox(string sFileLog) //import local saved log into RichTextBox and check Updating
        {
            string sPathCmd = sFileLog;
            System.Collections.Generic.List<string> sListRows = new System.Collections.Generic.List<string>();

            int iAllRowsInLog = 0;
            try
            {
                if (System.IO.File.Exists(sPathCmd))
                {
                    foreach (string st in System.IO.File.ReadAllLines(sPathCmd, System.Text.Encoding.GetEncoding(1251)))
                    {
                        try
                        {
                            if (st != null && st.Trim().Length > 0)
                                sListRows.Add(st.Trim()); iAllRowsInLog++;
                        } catch { }
                    }
                }
            } catch { }

            if (iAllRowsInLog > 2)
            {
                foreach (string st in sListRows.ToArray())
                {
                    try { _richTextBoxEchoAdd(st, true); } catch { }
                }
            }
            _richTextBoxEchoColor("=================", System.Drawing.Color.Red);
            _richTextBoxEchoColor("\n", System.Drawing.Color.Black);
        }

        private void buttonAction_Click(object sender, EventArgs e)
        { QuickAction(); }

        private void QuickAction()
        {
            string comboAction = sArAction.ToArray()[comboBoxAction.SelectedIndex];

            _ControlAddText(textBoxMessage, comboAction);

            if (comboBoxAction.SelectedIndex < 4)
            { _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + comboBoxAction.SelectedItem.ToString() + " на " + _textBoxReturnText(textBoxClientIp)); }
            else if (comboBoxAction.SelectedIndex == 4)
            {
                _richTextBoxEchoClear();
                _richTextBoxEchoColor("=================", System.Drawing.Color.Red);
                _richTextBoxEchoColor("\n", System.Drawing.Color.Black);
            }
            else if (comboBoxAction.SelectedIndex == 5)
            { _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Удаленный хост - " + comboBoxAction.SelectedItem.ToString() + " на " + _textBoxReturnText(textBoxClientIp)); }
            else if (comboBoxAction.SelectedIndex == 6) //Take Screenshot 
            {
                SendGetFile getFile = new SendGetFile();
                System.Threading.Tasks.Task t1 = System.Threading.Tasks.Task.Run(() =>
                   getFile.GetFile(strFolderGet, LocalHost.intPortGetFile));
                t1.Wait(3000);
            }
            else if (comboBoxAction.SelectedIndex == 7)
            {
                _ControlAddText(textBoxMessage, "mode " + @"true|" + hiden.ToString() + @"|" + controlled + " server");
                _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Удаленный хост - " + comboBoxAction.SelectedItem.ToString() + " на " + _textBoxReturnText(textBoxClientIp));
            }
            else if (comboBoxAction.SelectedIndex == 8)
            {
                _ControlAddText(textBoxMessage, "mode " + @"false|" + hiden.ToString() + @"|" + controlled + " server");
                _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Удаленный хост - " + comboBoxAction.SelectedItem.ToString() + " на " + _textBoxReturnText(textBoxClientIp));
            }
            else if (comboBoxAction.SelectedIndex == 9)
            {
                _ControlAddText(textBoxMessage, "mode " + maximize.ToString() + @"|true|" + controlled + " server");
                _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Удаленный хост - " + comboBoxAction.SelectedItem.ToString() + " на " + _textBoxReturnText(textBoxClientIp));
            }
            else if (comboBoxAction.SelectedIndex == 10)
            {
                _ControlAddText(textBoxMessage, "mode " + maximize.ToString() + @"|false|" + controlled + " server");
                _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Удаленный хост - " + comboBoxAction.SelectedItem.ToString() + " на " + _textBoxReturnText(textBoxClientIp));
            }
            else if (comboBoxAction.SelectedIndex == 11)
            {
                _ControlAddText(textBoxMessage, "mode " + maximize.ToString() + @"|" + hiden.ToString() + @"|uncontroltext" + " server");
                _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Удаленный хост - " + comboBoxAction.SelectedItem.ToString() + " на " + _textBoxReturnText(textBoxClientIp));
            }
            else if (comboBoxAction.SelectedIndex == 12)
            {
                _ControlAddText(textBoxMessage, "mode " + maximize.ToString() + @"|" + hiden.ToString() + @"|uncontrolcheck" + " server");
                _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Удаленный хост - " + comboBoxAction.SelectedItem.ToString() + " на " + _textBoxReturnText(textBoxClientIp));
            }
            else if (comboBoxAction.SelectedIndex == 13)
            {
                _ControlAddText(textBoxMessage, "mode " + maximize.ToString() + @"|" + hiden.ToString() + @"|control" + " server");
                _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Удаленный хост - " + comboBoxAction.SelectedItem.ToString() + " на " + _textBoxReturnText(textBoxClientIp));
            }

            if (comboBoxAction.SelectedIndex != 4)
            {
                ClientStartThread();
            }
        }

        private void MakeFolder(string strFolderInRegistry, bool FolderCreated, bool FolderPreviouslyDelete = false) // Create Folder and Write into the Registry
        {
            string strFolder = System.IO.Path.Combine(Application.StartupPath, strFolderInRegistry);
            if (System.IO.Directory.Exists(strFolder))
            {
                try
                {
                    if (FolderPreviouslyDelete)
                    {
                        System.IO.Directory.Delete(strFolder, true);
                        Thread.Sleep(100);
                        System.IO.Directory.CreateDirectory(strFolder);
                    }
                    FolderCreated = true;

                    using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(myRegKey))
                    {
                        EvUserKey.SetValue(strFolderInRegistry, encryptDecrypt.EncryptStringToBase64Text(strFolder, btsMess1, btsMess2), Microsoft.Win32.RegistryValueKind.String);
                    }
                } catch { }
            }
            else
            {
                try
                {
                    System.IO.Directory.CreateDirectory(strFolder);
                    FolderCreated = true;
                    using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(myRegKey))
                    {
                        EvUserKey.SetValue(strFolderInRegistry, encryptDecrypt.EncryptStringToBase64Text(strFolder, btsMess1, btsMess2), Microsoft.Win32.RegistryValueKind.String);
                    }
                    _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + LocalHost.UserName + ": Created: " + strFolder);
                } catch { }
            }
        }
        //----------------// Main. End Block  //----------------// 




        //----------------// Access to controls from Threads. Start Block  //----------------//
        protected internal bool _ShowFormMaximize() //Show Form1
        {
            bool currentMode = true;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        if (WindowState == FormWindowState.Minimized)
                        {
                            currentMode = false;
                        }
                        else
                        {
                            currentMode = true;
                        }
                    }));
                }
                else
                {
                    if (WindowState == FormWindowState.Minimized)
                    {
                        currentMode = false;
                    }
                    else
                    {
                        currentMode = true;
                    }
                }
            } catch { }
            return currentMode;
        }

        //Modes of showing window
        protected internal void _ShowForm(bool show) //Show Form1
        {
            maximize = show;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        if (show == true)
                        {
                            this.Show();
                            WindowState = FormWindowState.Normal;
                        }
                        else  //Hide Form1
                        {
                            ShowInTaskbar = false;
                            WindowState = FormWindowState.Minimized;
                        }
                    }));
                }
                else
                {
                    if (show == true)
                    {
                        this.Show();
                        WindowState = FormWindowState.Normal;
                    }
                    else
                    {
                        ShowInTaskbar = false;
                        WindowState = FormWindowState.Minimized;
                    }
                }
            } catch { }
        }

        protected internal void _UnSecretMode(bool bUnSecret) //Show Icon
        {
            hiden = bUnSecret;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        notifyIcon1.Visible = bUnSecret;
                    }));
                }
                else
                {
                    notifyIcon1.Visible = bUnSecret;
                }
            } catch { }
        }

        protected internal async void _UnControlMode(string mode) //Show Form1
        {
            controlled = mode;
            try
            {
                System.Collections.ArrayList Empty = new System.Collections.ArrayList();

                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        if (mode == "uncontroltext")
                        {
                            panelOpenPorts.Visible = false;
                            panelRichTextBox.BringToFront();
                            panelRichTextBox.Location = new System.Drawing.Point(3, 2);
                            panelRichTextBox.Size = new System.Drawing.Size(this.Width - 11, this.Height - 48);
                            panelRichTextBox.Visible = true;
                        }
                        else if (mode == "uncontrolcheck")
                        {
                            panelRichTextBox.Visible = false;
                            panelOpenPorts.BringToFront();
                            dgView.DataSource = Empty;
                            panelOpenPorts.Location = new System.Drawing.Point(3, 2);
                            panelOpenPorts.Size = new System.Drawing.Size(this.Width - 11, this.Height - 48);
                            dgView.DataSource = resultChecking;
                            if (_ShowFormMaximize() == true)
                            { System.Threading.Tasks.Task.Run(() => ChangeDatagridCellsColor()); }
                            dgView.BringToFront();
                            dgView.Refresh();
                            panelOpenPorts.Visible = true;
                        }
                        else
                        {

                            dgView.DataSource = Empty;
                            panelOpenPorts.Location = new System.Drawing.Point(0, 110);
                            panelOpenPorts.Size = new System.Drawing.Size(600, 225);
                            panelOpenPorts.Visible = false;
                            panelRichTextBox.Location = new System.Drawing.Point(0, 110);
                            panelRichTextBox.Size = new System.Drawing.Size(600, 225);
                            panelRichTextBox.BringToFront();
                            panelRichTextBox.Visible = true;
                        }
                    }));
                }
                else
                {
                    if (mode == "uncontroltext")
                    {
                        panelOpenPorts.Visible = false;
                        panelRichTextBox.BringToFront();
                        panelRichTextBox.Location = new System.Drawing.Point(3, 2);
                        panelRichTextBox.Size = new System.Drawing.Size(this.Width - 11, this.Height - 48);
                        panelRichTextBox.Visible = true;
                    }
                    else if (mode == "uncontrolcheck")
                    {
                        panelRichTextBox.Visible = false;
                        panelOpenPorts.BringToFront();
                        dgView.DataSource = Empty;
                        panelOpenPorts.Location = new System.Drawing.Point(3, 2);
                        panelOpenPorts.Size = new System.Drawing.Size(this.Width - 11, this.Height - 48);
                        dgView.DataSource = resultChecking;
                        if (_ShowFormMaximize() == true)
                        { await System.Threading.Tasks.Task.Run(() => ChangeDatagridCellsColor()); }

                        dgView.BringToFront();
                        dgView.Refresh();
                        panelOpenPorts.Visible = true;
                    }
                    else
                    {

                        dgView.DataSource = Empty;
                        panelOpenPorts.Location = new System.Drawing.Point(0, 110);
                        panelOpenPorts.Size = new System.Drawing.Size(600, 225);
                        panelOpenPorts.Visible = false;
                        panelRichTextBox.Location = new System.Drawing.Point(0, 110);
                        panelRichTextBox.Size = new System.Drawing.Size(600, 225);
                        panelRichTextBox.BringToFront();
                        panelRichTextBox.Visible = true;
                    }
                }
            } catch { }
        }

        protected internal void _OpenFileDialogResult() //Show Form1
        {
            sFilePath = "";
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            sFilePath = openFileDialog1.FileName;
                    }));
                }
                else
                {
                    if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        sFilePath = openFileDialog1.FileName;
                }
            } catch { }
        }

        private int _numericUpDownClien()
        {
            int t = 0;
            if (InvokeRequired)
            { Invoke(new MethodInvoker(delegate { t = (int)numericUpDownClient.Value; })); }
            else
            { t = (int)numericUpDownClient.Value; }
            return t;
        }

        private void _numericUpDownClientValue(int s)
        {
            try
            {
                if (InvokeRequired)
                { Invoke(new MethodInvoker(delegate { numericUpDownClient.Value = s; })); }
                else
                { numericUpDownClient.Value = s; }
            } catch { }
        }

        protected internal void _statusLabelText(ToolStripStatusLabel sLabel, string s)
        {
            try
            {
                if (InvokeRequired)
                { Invoke(new MethodInvoker(delegate { sLabel.Text = s; })); }
                else
                { sLabel.Text = s; }
            } catch { }
        }

        protected internal void _ControlAddText(Control controlName, string s)
        {
            try
            {
                if (InvokeRequired)
                { Invoke(new MethodInvoker(delegate { controlName.Text = s; })); }
                else
                { controlName.Text = s; }
            } catch { }
        }

        protected internal void _ControlEnable(Control controlName, bool enable)
        {
            try
            {
                if (InvokeRequired)
                { Invoke(new MethodInvoker(delegate { controlName.Enabled = enable; })); }
                else
                { controlName.Enabled = enable; }
            } catch { }
        }

        private void _ControlSelect(Control controlName)
        {
            try
            {
                if (InvokeRequired)
                { Invoke(new MethodInvoker(delegate { controlName.Select(); })); }
                else
                { controlName.Select(); }
            } catch { }
        }

        private int _textBoxTextLength(Control controlName)
        {
            int t = 0;
            if (InvokeRequired)
            { Invoke(new MethodInvoker(delegate { t = controlName.Text.Trim().Length; })); }
            else
            { t = controlName.Text.Trim().Length; }
            return t;
        }

        private string _textBoxReturnText(Control controlName)
        {
            string t = "";
            if (InvokeRequired)
            { Invoke(new MethodInvoker(delegate { t = controlName.Text.Trim(); })); }
            else
            { t = controlName.Text.Trim(); }
            return t;
        }

        protected internal void _richTextBoxEchoClear()
        {
            string s;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        try { System.IO.File.Delete(FileLog); } catch { }
                        System.Threading.Thread.Sleep(100);

                        richTextBoxEcho.Clear();

                        s = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": ";
                        richTextBoxEcho.AppendText(s);
                        richTextBoxEcho.Select(richTextBoxEcho.TextLength - s.Length, s.Length);
                        richTextBoxEcho.SelectionColor = System.Drawing.Color.Black;
                        richTextBoxEcho.SelectionStart = richTextBoxEcho.Text.Length;
                        richTextBoxEcho.ScrollToCaret();
                        s = ": " + @"Log was cleared...";
                        richTextBoxEcho.AppendText(s);
                        richTextBoxEcho.Select(richTextBoxEcho.TextLength - s.Length, s.Length);
                        richTextBoxEcho.SelectionColor = System.Drawing.Color.Red;
                        richTextBoxEcho.SelectionStart = richTextBoxEcho.Text.Length;
                        richTextBoxEcho.ScrollToCaret();
                        s = "";
                        richTextBoxEcho.AppendText(s);
                        richTextBoxEcho.Select(richTextBoxEcho.TextLength - s.Length, s.Length);
                        richTextBoxEcho.SelectionColor = System.Drawing.Color.Black;
                        richTextBoxEcho.SelectionStart = richTextBoxEcho.Text.Length;
                        richTextBoxEcho.ScrollToCaret();
                    }));
                }
                else
                {
                    try { System.IO.File.Delete(FileLog); } catch { }
                    System.Threading.Thread.Sleep(100);

                    richTextBoxEcho.Clear();

                    s = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": ";
                    richTextBoxEcho.AppendText(s);
                    richTextBoxEcho.Select(richTextBoxEcho.TextLength - s.Length, s.Length);
                    richTextBoxEcho.SelectionColor = System.Drawing.Color.Black;
                    richTextBoxEcho.SelectionStart = richTextBoxEcho.Text.Length;
                    richTextBoxEcho.ScrollToCaret();
                    s = ": " + @"Log was cleared...";
                    richTextBoxEcho.AppendText(s);
                    richTextBoxEcho.Select(richTextBoxEcho.TextLength - s.Length, s.Length);
                    richTextBoxEcho.SelectionColor = System.Drawing.Color.Red;
                    richTextBoxEcho.SelectionStart = richTextBoxEcho.Text.Length;
                    richTextBoxEcho.ScrollToCaret();
                    s = "\n";
                    richTextBoxEcho.AppendText(s);
                    richTextBoxEcho.Select(richTextBoxEcho.TextLength - s.Length, s.Length);
                    richTextBoxEcho.SelectionColor = System.Drawing.Color.Black;
                    richTextBoxEcho.SelectionStart = richTextBoxEcho.Text.Length;
                    richTextBoxEcho.ScrollToCaret();
                }
            } catch { }
        }

        public void _richTextBoxEchoAdd(string s, bool ReadFromFileLog = false)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        richTextBoxEcho.AppendText(s + "\n");
                        richTextBoxEcho.SelectionStart = richTextBoxEcho.Text.Length;
                        richTextBoxEcho.ScrollToCaret();
                        if (!ReadFromFileLog)
                        {
                            _AppendTextToFile(FileLogMessages, s);
                        }
                    }));
                }
                else
                {
                    richTextBoxEcho.AppendText(s + "\n");
                    richTextBoxEcho.SelectionStart = richTextBoxEcho.Text.Length;
                    richTextBoxEcho.ScrollToCaret();
                    if (!ReadFromFileLog)
                    {
                        _AppendTextToFile(FileLogMessages, s);
                    }
                }
            } catch { }
        }

        protected internal void _richTextBoxEchoColor(string s, System.Drawing.Color colorText)
        {
            try
            {
                if (InvokeRequired)
                    Invoke(new MethodInvoker(delegate
                    {
                        richTextBoxEcho.AppendText(s);
                        richTextBoxEcho.Select(richTextBoxEcho.TextLength - s.Length, s.Length);
                        richTextBoxEcho.SelectionColor = colorText;
                        richTextBoxEcho.SelectionStart = richTextBoxEcho.Text.Length;
                        richTextBoxEcho.ScrollToCaret();
                        _AppendTextToFile(FileLogMessages, s);
                    }));
                else
                {
                    richTextBoxEcho.AppendText(s);
                    richTextBoxEcho.Select(richTextBoxEcho.TextLength - s.Length, s.Length);
                    richTextBoxEcho.SelectionColor = colorText;
                    richTextBoxEcho.SelectionStart = richTextBoxEcho.Text.Length;
                    richTextBoxEcho.ScrollToCaret();
                    _AppendTextToFile(FileLogMessages, s);
                }
            } catch { }
        }

        private System.Collections.Generic.List<string> _richTextBoxEchoText()
        {
            System.Collections.Generic.List<string> sb = new System.Collections.Generic.List<string>();

            if (InvokeRequired)
            { Invoke(new MethodInvoker(delegate { foreach (string st in richTextBoxEcho.Lines) { sb.Add(st); } })); }
            else
            { foreach (string st in richTextBoxEcho.Lines) { sb.Add(st); } }
            return sb;
        }

        private void _textBoxClear(Control controlName)
        {
            if (InvokeRequired)
            { Invoke(new MethodInvoker(delegate { controlName.Text = ""; })); }
            else
            { controlName.Text = ""; }
        }

        //DataGrid Job
        private void _ShowDataTableAtDatagrid(DataTable dt) //Access into Datagrid from other threads
        {
            System.Collections.ArrayList Empty = new System.Collections.ArrayList();
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        dgView.DataSource = Empty;
                        dgView.DataSource = dt;
                        dgView.AutoResizeColumns();
                    }));
                }
                else
                {
                    dgView.DataSource = Empty;
                    dgView.DataSource = dt;
                    dgView.AutoResizeColumns();
                }
            } catch { }
        }

        private void _ChangeDataGridCurrentRowDefaultCellStyleBackColor(int i, int k, System.Drawing.Color color)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        try { dgView[i, k].Style.BackColor = color; } catch { }
                    }));
                }
                else
                {
                    try { dgView[i, k].Style.BackColor = color; } catch { }
                }
            } catch { }
        }

        private int _CountColumnDataGrid()
        {
            int collumn = 0;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        try { collumn = dgView.ColumnCount; } catch { }
                    }));
                }
                else
                {
                    try { collumn = dgView.ColumnCount; } catch { }
                }
            } catch { }
            return collumn;
        }

        private int _CountRowDataGrid()
        {
            int row = 0;
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        try { row = dgView.Rows.Count; } catch { }
                    }));
                }
                else
                {
                    try { row = dgView.Rows.Count; } catch { }
                }
            } catch { }
            return row;
        }

        private string _GetHeaderDataGrid(int i)
        {
            string header = "";
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        try { header = dgView.Columns[i].HeaderText.ToString(); } catch { }
                    }));
                }
                else
                {
                    try { header = dgView.Columns[i].HeaderText.ToString(); } catch { }
                }
            } catch { }
            return header;
        }

        private string _GetValueCellDataGrid(int c, int r)
        {
            string cell = "";
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        try { cell = dgView[c, r].Value.ToString(); } catch { }
                    }));
                }
                else
                {
                    try { cell = dgView[c, r].Value.ToString(); } catch { }
                }
            } catch { }
            return cell;
        }

        private void ChangeDatagridCellsColor()
        {
            string header = "";
            try
            {
                int collumnCount = _CountColumnDataGrid();

                if (collumnCount > 0)
                {
                    int iIndexColumn1 = -1;   //collumn IP   
                    int iIndexColumn2 = -1;   //collumn Name   
                    int iIndexColumn3 = -1;   //collumn Group   
                    int iIndexColumn4 = -1;   //collumn Ping   
                    int iIndexColumn5 = -1;   //collumn Check Port   
                    int iIndexColumn6 = -1;   //collumn Port   

                    for (int i = 0; i < collumnCount; i++)
                    {
                        header = _GetHeaderDataGrid(i);
                        if (header == "IP")
                        { iIndexColumn1 = i; }
                        else if (header == "Name")
                        { iIndexColumn2 = i; }
                        else if (header == "Group")
                        { iIndexColumn3 = i; }
                        else if (header == "Ping")
                        { iIndexColumn4 = i; }
                        else if (header == "Check Port")
                        { iIndexColumn5 = i; }
                        else if (header == "Port")
                        { iIndexColumn6 = i; }
                    }

                    for (int i = 0; i < _CountRowDataGrid(); ++i) //расскраска ячеек таблицы в зависимости от полученных результатов
                    {
                        if (_GetValueCellDataGrid(iIndexColumn1, i).Length > 0)
                        {
                            string resultPing = _GetValueCellDataGrid(iIndexColumn5, i).ToString();
                            bool bHostAlive = false;

                            if (resultPing == "Listen" || resultPing == "RDP" || resultPing == "SQLServer" || resultPing == "VideoServer")
                                bHostAlive = true;

                            if (_GetValueCellDataGrid(iIndexColumn4, i).Contains("Success") && bHostAlive)
                            {
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn1, i, System.Drawing.Color.PaleGreen);
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn4, i, System.Drawing.Color.PaleGreen);
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn5, i, System.Drawing.Color.PaleGreen);
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn6, i, System.Drawing.Color.PaleGreen);
                            }
                            else if (_GetValueCellDataGrid(iIndexColumn4, i).Contains("Success"))
                            {
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn1, i, System.Drawing.Color.Khaki);
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn4, i, System.Drawing.Color.PaleGreen);
                            }
                            else if (bHostAlive)
                            {
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn1, i, System.Drawing.Color.Khaki);
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn5, i, System.Drawing.Color.PaleGreen);
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn6, i, System.Drawing.Color.PaleGreen);
                            }
                            else if (!_GetValueCellDataGrid(iIndexColumn4, i).Contains("Success") && bHostAlive)
                            {
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn1, i, System.Drawing.Color.DarkOrange);
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn4, i, System.Drawing.Color.DarkOrange);
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn5, i, System.Drawing.Color.DarkOrange);
                                _ChangeDataGridCurrentRowDefaultCellStyleBackColor(iIndexColumn6, i, System.Drawing.Color.DarkOrange);
                            }
                        }
                    }
                }
            } catch { }
        }
        //----------------// Access to controls from Threads. End Block //----------------//
    }
}



/*
    DateTime Tthen = DateTime.Now;
    do { Application.DoEvents(); } while ((Tthen.AddSeconds(2) > DateTime.Now)); //Wait 2 sec
    */

/*private async void SendMessageFromSocket(string sNameOrIPRemoteServer, int port, string LastAction)
{
    _richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + sNameOrIPRemoteServer + ": " + port + ": " + LastAction);  //вывод сообщения
    string messageDecrypted = "";
    byte[] msg = new byte[0];
    using (var tcpClient = new TcpClient())
    {
        await tcpClient.ConnectAsync(sNameOrIPRemoteServer, port);
        using (var networkStream = tcpClient.GetStream())
        {
            networkStream.ReadTimeout = 500;
            networkStream.WriteTimeout = 500;

            if (LastAction.Trim().Length > 0)
            {
                msg = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael(LocalHost.UserName + ": " + LastAction, salt));
            }
            else
            {
                msg = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael("Проверка связи!", salt));
            }

            await networkStream.WriteAsync(msg, 0, msg.Length);

            System.Threading.Tasks.Task.WaitAll();
            byte[] buffer = new byte[4096];
            int byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            string response = System.Text.Encoding.UTF8.GetString(buffer, 0, byteCount);
            if (response.Length == 0) messageDecrypted = "Проверка связи!";
            else messageDecrypted = encryptDecrypt.DecryptRijndael(response, salt);
            _richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + messageDecrypted);  //вывод сообщения

            System.Threading.Tasks.Task.WaitAll();
            networkStream.Dispose();
        }
    }
}


    /*
    string sPathCmd = "";
    try
    {
        RegistryDataSave(modesWindow);
        sPathCmd = System.IO.Path.Combine(".", "myCltSvr.cmd");

        if (System.IO.File.Exists(sPathCmd))
        {
            try { System.IO.File.Delete(sPathCmd); } catch { }
            Thread.Sleep(500);
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("@echo off");
        sb.AppendLine("taskkill /F /IM " + System.IO.Path.GetFileName(Application.ExecutablePath) + " > nul");
        sb.AppendLine("taskkill /F /IM " + System.IO.Path.GetFileName(Application.ExecutablePath) + " /T > nul");
        System.IO.File.WriteAllText(sPathCmd, sb.ToString(), System.Text.Encoding.GetEncoding(866));
        sb = null;
    }
    catch { }
    _AppendTextToFile(FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Application Exit");

    try
    {
        System.Diagnostics.Process pr = new System.Diagnostics.Process();
        pr.StartInfo.FileName = sPathCmd;
        pr.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        pr.Start();
    }
    catch { }*/


//var location = System.Reflection.Assembly.GetEntryAssembly().Location;
//var directory = System.IO.Path.GetDirectoryName(location);
//var file = System.IO.Path.Combine(directory,System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe");

