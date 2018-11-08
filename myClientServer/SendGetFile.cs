using System;

namespace myClientServer
{
    public class SendGetFile
    {
        private readonly byte[] btsMess1 = Convert.FromBase64String(@"OCvesunvXXsxtt381jr7vp3+UCwDbE4ebdiL1uinGi0="); //Key Encrypt
        private readonly byte[] btsMess2 = Convert.FromBase64String(@"NO6GC6Zjl934Eh8MAJWuKQ=="); //Key Decrypt
        private EncryptDecrypt encryptDecrypt = new EncryptDecrypt();
        private System.Threading.AutoResetEvent aResEvntGotFile = new System.Threading.AutoResetEvent(false);
        System.Net.Sockets.TcpClient client;

        public string sActionFile = "";

        protected internal async void SendFile(string RemoteSeverName, int intFileSendPort, string sFullPathToFile) //Send file to _textBoxClientIpText():_numericUpDownClien()
        {

            client = new System.Net.Sockets.TcpClient(RemoteSeverName, intFileSendPort);
            try
            {
                using (System.IO.FileStream inputStream = System.IO.File.OpenRead(sFullPathToFile))
                {
                    using (System.Net.Sockets.NetworkStream outputStream = client.GetStream())
                    {
                        using (var cancellationTokenSource = new System.Threading.CancellationTokenSource(5000))
                        {
                            using (cancellationTokenSource.Token.Register(() => outputStream.Close()))
                            {
                                using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(outputStream))
                                {
                                    long lenght = inputStream.Length;
                                    long totalBytes = 0;
                                    int readBytes = 0;
                                    byte[] buffer = new byte[1024];
                                    try
                                    {
                                        writer.Write(System.IO.Path.GetFileName(sFullPathToFile));
                                        writer.Write(lenght);
                                        do
                                        {
                                            readBytes = await inputStream.ReadAsync(buffer, 0, buffer.Length);
                                            await outputStream.WriteAsync(buffer, 0, readBytes, cancellationTokenSource.Token);
                                            totalBytes += readBytes;
                                        } while (client.Connected && totalBytes < lenght);
                                    }
                                    catch (TimeoutException e)
                                    {
                                        readBytes = -1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception expt)
            {
                Form1.myForm._richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " + "Can't send file to "+ RemoteSeverName+ ": "+ expt.Message);
            }
            client.Close();
        }

        protected internal async void GetFile(string sFullPathToFolder, int intFileGetPort) //Using GetFile(int iPort)
        {
            try
            {
                System.Net.Sockets.TcpListener listener = System.Net.Sockets.TcpListener.Create(intFileGetPort);
                Form1.myForm._richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " +
                    Form1.myForm.LocalHost.UserName + ": Готов принять файл");

                listener.Start();
                System.Net.Sockets.TcpClient client = await listener.AcceptTcpClientAsync();
                using (System.Net.Sockets.NetworkStream inputStream = client.GetStream())
                {
                    using (var cancellationTokenSource = new System.Threading.CancellationTokenSource(3000))
                    {
                        using (cancellationTokenSource.Token.Register(() => inputStream.Close()))
                        {
                            using (System.IO.BinaryReader reader = new System.IO.BinaryReader(inputStream))
                            {
                                sActionFile = reader.ReadString();
                                Form1.myForm.sFilePath = System.IO.Path.Combine(sFullPathToFolder, sActionFile);
                                long lenght = reader.ReadInt64();
                                using (System.IO.FileStream outputStream = System.IO.File.Open(Form1.myForm.sFilePath, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                                {
                                    long totalBytes = 0;
                                    int readBytes = 0;

                                    try
                                    {
                                        byte[] buffer = new byte[1024];

                                        do
                                        {
                                            readBytes = await inputStream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token); //cancellationtoken
                                            outputStream.Write(buffer, 0, readBytes);
                                            totalBytes += readBytes;
                                        } while (client.Connected && totalBytes < lenght);

                                        string sizeReceivedFile = "";
                                        if (totalBytes < 1024)
                                        { sizeReceivedFile = totalBytes + " B"; }
                                        else if (1024 < totalBytes && totalBytes < 1048576)
                                        { sizeReceivedFile = Math.Round((Convert.ToDouble(totalBytes) / 1024), 1).ToString() + " kB"; }
                                        else if (1048576 < totalBytes && totalBytes < 1073741824)
                                        { sizeReceivedFile = Math.Round((Convert.ToDouble(totalBytes) / 1024 / 1024), 1).ToString() + " MB"; }
                                        else
                                        { sizeReceivedFile = Math.Round((Convert.ToDouble(totalBytes) / 1024 / 1024 / 1024), 1).ToString() + " GB"; }

                                        Form1.myForm._richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " +
                                        Form1.myForm.LocalHost.UserName + ": Принят файл - " + Form1.myForm.sFilePath + " Размер - " + sizeReceivedFile);
                                    }
                                    catch (TimeoutException e)
                                    {
                                        readBytes = -1;
                                    }
                                }
                            }
                        }
                    }
                }
                
                if (System.IO.Path.GetFileName(Form1.myForm.sFilePath).ToLower() != System.IO.Path.GetFileName(System.Windows.Forms.Application.ExecutablePath).ToLower())
                {
                    if (!System.IO.Path.GetExtension(Form1.myForm.sFilePath).ToLower().Contains("top") &&
                        !System.IO.Path.GetExtension(Form1.myForm.sFilePath).ToLower().Contains("jpg") &&
                        !System.IO.Path.GetExtension(Form1.myForm.sFilePath).ToLower().Contains("png"))
                    {
                        Form1.myForm._richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " +
                    Form1.myForm.LocalHost.UserName + "copy from " + Form1.myForm.sFilePath + " to " + System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, System.IO.Path.GetFileName(Form1.myForm.sFilePath)));
                        System.IO.File.Copy(
                            Form1.myForm.sFilePath,
                        System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, System.IO.Path.GetFileName(Form1.myForm.sFilePath)), true
                        );
                    }
                //    else { System.IO.File.Delete(Form1.myForm.sFilePath); }
                }
                else
                {
                    Form1.myForm._richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " +
                    Form1.myForm.LocalHost.UserName + "get " + Form1.myForm.sFilePath);
                }

              //  aResEvntGotFile.Set();
                //close chanel after got updates
                client.Close(); //test
                listener.Stop(); //test
                listener.Server.Dispose();
            }
            catch (Exception expt)
            {
                Form1.myForm._richTextBoxEchoAdd(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": " +"Can't get file... "+ expt.Message);
            }
        }
    }
}
