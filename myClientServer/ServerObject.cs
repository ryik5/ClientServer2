using System;
using System.Text;

namespace myClientServer
{
    public class ServerObject
    {
        protected internal string message = "";
        private string salt = @"PasswordSimple!";
        private readonly byte[] btsMess1 = Convert.FromBase64String(@"OCvesunvXXsxtt381jr7vp3+UCwDbE4ebdiL1uinGi0="); //Key Encrypt
        private readonly byte[] btsMess2 = Convert.FromBase64String(@"NO6GC6Zjl934Eh8MAJWuKQ=="); //Key Decrypt
        private string myRegKey = @"SOFTWARE\RYIK\ServerClientCommunicator2";
        private EncryptDecrypt encryptDecrypt = new EncryptDecrypt();
        private DoAction doAction = new DoAction();
        private string[] actionWord = {
        "RUN",
        "TAKE",
        "MODE",
        "KILL",
        "TASKLIST",
        "GET",
        "UPDATESERVER",
        "CLEAR",
        "NAME",
        "RESTART",
        };

        private System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();

        protected internal async void Listen()  // прослушивание входящих подключений
        {
            System.Net.Sockets.TcpListener tcpListener = System.Net.Sockets.TcpListener.Create(Form1.myForm.LocalHost.intPortChat);
            try
            {
                tcpListener.Start();
                while (true)
                {
                    using (var tcpClient = await tcpListener.AcceptTcpClientAsync())
                    {
                        string id = Guid.NewGuid().ToString();   // id connection
                        // Form1.myForm._richTextBoxEchoAdd( DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Client has connected " + id);

                        using (var networkStream = tcpClient.GetStream())
                        {
                            var buffer = new byte[4096];
                            var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                            var message = encryptDecrypt.DecryptRijndael(Encoding.UTF8.GetString(buffer, 0, byteCount), salt);
                            try { cts.Cancel(); } catch { }
                            try
                            {
                                if (Form1.myForm.RemoteHost.HostName == null)
                                {
                                    try { Form1.myForm.RemoteHost.HostName = System.Net.Dns.GetHostEntry(message.Split('(')[0].Trim()); } catch { }
                                    try { Form1.myForm.RemoteHost.ipAddress = System.Net.IPAddress.Parse((message.Split('(')[1]).Split(')')[0].Trim()); } catch { }
                                    try { Form1.myForm.RemoteHost.intPortGetFile = Convert.ToInt32(message.Split(':')[2].Trim()); } catch { }
                                    Form1.myForm.RemoteHost.SetInfo();
                                }
                                else if (Form1.myForm.RemoteHost.HostName != null)
                                {
                                    string currentIP = (message.Split('(')[1]).Split(')')[0].Trim();
                                    if (currentIP != Form1.myForm.RemoteHost.ipAddress.ToString())
                                    {
                                        try { Form1.myForm.RemoteHost.HostName = System.Net.Dns.GetHostEntry(message.Split('(')[0].Trim()); } catch { }
                                        try { Form1.myForm.RemoteHost.ipAddress = System.Net.IPAddress.Parse((message.Split('(')[1]).Split(')')[0].Trim()); } catch { }
                                        try { Form1.myForm.RemoteHost.intPortGetFile = Convert.ToInt32(message.Split(':')[2].Trim()); } catch { }
                                        Form1.myForm.RemoteHost.SetInfo();
                                    }
                                }

                                // Form1.myForm._richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + message);

                                buffer = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael(message, salt));

                                //byte[] msg = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael("Hi!", salt));
                                await networkStream.WriteAsync(buffer, 0, buffer.Length);

                                buffer = new byte[4096];
                                byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                                message = encryptDecrypt.DecryptRijndael(Encoding.UTF8.GetString(buffer, 0, byteCount), salt);
                                //Write all info into log
                                //Form1.myForm._richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + Form1.myForm.RemoteHost.UserName + ": " + message);
                                                            
                                doAction = new DoAction();
                                doAction.message = Form1.myForm.RemoteHost.UserName + " " + message;
                                doAction.CheckGotMessage();
                                if (doAction.ActionSelected == "NAME")
                                {
                                    buffer = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael(doAction.answer, salt));
                                    await networkStream.WriteAsync(buffer, 0, buffer.Length);
                                }
                                else if (doAction.ActionSelected == "TAKE")
                                {
                                    buffer = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael(doAction.answer, salt));
                                    // await networkStream.WriteAsync(buffer, 0, buffer.Length);
                                }
                                else if (doAction.ActionSelected == "UPDATESERVER")
                                {
                                    Random rnd = new Random();
                                    string sActionFolder = rnd.Next().ToString();
                                    System.IO.DirectoryInfo UpdateFolderFullPath = new System.IO.DirectoryInfo(System.Windows.Forms.Application.StartupPath + "\\" + sActionFolder);

                                    try { UpdateFolderFullPath.Create(); } catch { }
                                    Form1.myForm.strFolderUpdates = UpdateFolderFullPath.ToString();
                                    try
                                    {
                                        using (Microsoft.Win32.RegistryKey EvUserKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(myRegKey))
                                        {
                                            EvUserKey.SetValue("UPDATESERVERFOLDER", encryptDecrypt.EncryptStringToBase64Text(Form1.myForm.strFolderUpdates, btsMess1, btsMess2), Microsoft.Win32.RegistryValueKind.String);
                                        }
                                    }
                                    catch { }

                                    string sPathCmd = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "myCltSvr.cmd");
                                    doAction.UpdateServerMakecmd(sPathCmd, sActionFolder);

                                    buffer = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael(Form1.myForm.LocalHost.UserName + ": Ready to get a file on the port: " + Form1.myForm.LocalHost.intPortGetFile, salt));
                                    await networkStream.WriteAsync(buffer, 0, buffer.Length);

                                    SendGetFile getFile = new SendGetFile();
                                    getFile.GetFile(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, sActionFolder), Form1.myForm.LocalHost.intPortGetFile);

                                    doAction.RunProcess(sPathCmd);
                                }
                                else if (doAction.ActionSelected == "GET") //Prepare to get file by Server
                                {
                                    SendGetFile getFile = new SendGetFile();
                                    buffer = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael(Form1.myForm.LocalHost.UserName + ": Ready to get a file on the port: " + Form1.myForm.LocalHost.intPortGetFile, salt));
                                    await networkStream.WriteAsync(buffer, 0, buffer.Length);  //Добавить cancelationtoken
                                    getFile.GetFile(Form1.myForm.strFolderGet, Form1.myForm.LocalHost.intPortGetFile);
                                }
                                else if (doAction.ActionSelected.Length > 0)
                                {
                                    cts = new System.Threading.CancellationTokenSource();
                                    bool messageIsCommand = false;
                                    foreach (string str in actionWord) //
                                    {
                                        if (doAction.ActionSelected== str)
                                        {
                                            messageIsCommand = true;                                            
                                            break;
                                        }
                                    }
                                    if (messageIsCommand!=true && Form1.myForm.controlled!= "uncontrolcheck")
                                    {
                                        System.Threading.Tasks.Task.Run(() => MakeDelayForHide(10000), cts.Token); //отмена задачи если будет   cts.Cancel
                                    }
                                    
                                    buffer = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael(doAction.message, salt));
                                    await networkStream.WriteAsync(buffer, 0, buffer.Length);
                                }
                                else if (doAction.ActionSelected == "")
                                {
                                    buffer = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael(Form1.myForm.RemoteHost.UserName + ": said  -= " + message.ToUpper(), salt));
                                    await networkStream.WriteAsync(buffer, 0, buffer.Length);
                                }
                                else
                                {
                                    buffer = System.Text.Encoding.UTF8.GetBytes(encryptDecrypt.EncryptRijndael(Form1.myForm.RemoteHost.UserName + ": said nothing", salt));
                                    await networkStream.WriteAsync(buffer, 0, buffer.Length);
                                }
                                await networkStream.WriteAsync(buffer, 0, buffer.Length);
                            }
                            catch// (Exception expt)
                            {
                             //   Form1.myForm._AppendTextToFile(Form1.myForm.FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": Listen().tcpClient.GetStream(): " + expt.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception expt)
            { Form1.myForm._AppendTextToFile(Form1.myForm.FileLog, DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": Listen(): " + expt.Message); }
            finally { tcpListener.Stop(); }
        }

        private async void MakeDelayForHide(int delayms)
        {
            await System.Threading.Tasks.Task.Delay(delayms);
            Form1.myForm._ShowForm(false);
        }
    }
}
