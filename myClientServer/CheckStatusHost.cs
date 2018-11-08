using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace myClientServer
{
    class CheckStatusHost
    {
        //outbox settings
        protected internal string hostIP { get; set; }
        protected internal string hostName { get; set; }
        protected internal string hostGroup { get; set; }
        protected internal int hostPort { get; set; }
        protected internal int hostTimeOut { get; set; }

        //internal 
        protected internal string ResultPing { get; private set; }
        private bool statusPing = false;
        protected internal string ResultCheckPort { get; private set; }
        private bool statusPort = false;
        protected internal bool status { get; private set; }
        protected internal string hostCheckTime { get; private set; }

        protected internal void GetStatusHost()
        {
            string openPort = "";
            status = false;
            CheckValidRemoteIPName();
            if (hostPort < 10) hostPort = 1200;
            if (hostTimeOut < 10) hostTimeOut = 200; //ms
            if (hostGroup == null) hostGroup = "Common";
            if (hostIP != null)
            {
                hostCheckTime = DateTime.Now.ToShortTimeString();
                GetPingHost(hostIP, hostTimeOut);
                GetOpenPortHost(hostIP, hostPort);
                do
                {
                    System.Threading.Tasks.Task.Delay(200);
                }
                while (!statusPort);

                statusPort = false;
                if (ResultCheckPort != "Listen")
                {
                    GetOpenPortHost(hostIP, 20900); //search videoserver's open port1
                    do
                    {
                        System.Threading.Tasks.Task.Delay(200);
                    }
                    while (!statusPort);

                    statusPort = false;
                    if (ResultCheckPort != "Listen")
                    {
                        GetOpenPortHost(hostIP, 21111); //search videoserver's open port2
                        do
                        {
                            System.Threading.Tasks.Task.Delay(200);
                        }
                        while (!statusPort);

                        statusPort = false;
                        if (ResultCheckPort != "Listen")
                        {
                            GetOpenPortHost(hostIP, 1433); //search videoserver's open port2
                            do
                            {
                                System.Threading.Tasks.Task.Delay(200);
                            }
                            while (!statusPort);

                            statusPort = false;
                            if (ResultCheckPort != "Listen")
                            {
                                GetOpenPortHost(hostIP, 3389); //search videoserver's open port2
                                do
                                {
                                    System.Threading.Tasks.Task.Delay(200);
                                }
                                while (!statusPort);
                                if (ResultCheckPort == "Listen")
                                { openPort = "RDP"; }
                            }
                            else { openPort = "SQLServer"; }
                        }
                        else { openPort = "VideoServer"; }
                    }
                    else { openPort = "VideoServer"; }
                }
                else { openPort = "Listen"; }
            }
            else { openPort = "Wrong"; status = true; }

            if (openPort!= "Wrong" && openPort != "Listen")
            { ResultCheckPort = openPort; }
        }

        private void CheckValidRemoteIPName()
        {
            if (hostName == null) try { hostName = Dns.GetHostEntry(hostIP).HostName; } catch { hostName = IPAddress.Parse(hostIP).ToString(); }
            if (hostIP == null) try { hostIP = IPAddress.Parse(hostName).ToString(); }
            catch
            {
                try
                {
                    IPAddress[] addresslist = Dns.GetHostAddresses(hostIP);
                    foreach (IPAddress address in addresslist)
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address) &&
                                    (address.ToString().StartsWith("192.168.") || address.ToString().StartsWith("172.") || address.ToString().StartsWith("10.")))
                        {
                            hostIP = address.ToString();
                            break;
                        }
                    }
                }
                catch { }
            }
            if (hostName == null) try { hostName = Dns.GetHostEntry(hostIP).HostName; } catch { hostName = IPAddress.Parse(hostIP).ToString(); }
            hostName = hostName.ToUpper();
       }

        private void GetPingHost(string host, int timeOut)
        {
            AutoResetEvent waiter = new AutoResetEvent(false);
            System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions(64, true);
            string data = "aaaaa";
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(data);
            System.Net.NetworkInformation.Ping pingCheck = new System.Net.NetworkInformation.Ping();
            pingCheck.PingCompleted += new System.Net.NetworkInformation.PingCompletedEventHandler(PingCompletedCallback);
            pingCheck.SendAsync(host, timeOut, buffer, options, waiter);
        }

        private void PingCompletedCallback(object sender, System.Net.NetworkInformation.PingCompletedEventArgs e)
        {
            if (e.Cancelled)            // If the operation was canceled, display a message to the user.
            {
                ResultPing = "Ping canceled";  // UserToken is the AutoResetEvent object that the main thread  is waiting for.
                ((AutoResetEvent)e.UserState).Set();
            }

            if (e.Error != null)    // If an error occurred, display the exception to the user.
            {
                ResultPing = "Ping failed: " + e.Error.ToString();
                ((AutoResetEvent)e.UserState).Set(); // Let the main thread resume. 
            }

            System.Net.NetworkInformation.PingReply reply = e.Reply;
            ((AutoResetEvent)e.UserState).Set();   // Let the main thread resume.

            statusPing = true;
            ResultPing = DisplayReply(reply);
            if (statusPort == true) status = true;
        }

        private string DisplayReply(System.Net.NetworkInformation.PingReply reply)
        {
            string status = "";

            if (reply == null)
            { return status; }
            else
            {
                status = "Ping: " + reply.Status;
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    // status += "Address: " + reply.Address.ToString();
                    //  status += " time: " + reply.RoundtripTime;
                }
                return status;
            }
        }

        private void GetOpenPortHost(string host, int port)
        {
            string result = "";
            try
            {
                IPAddress[] IPs = Dns.GetHostAddresses(host);
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    s.Connect(IPs[0], port);
                    if (s.Connected == true)
                    {
                        s.Close();
                        result = "Listen";
                    }
                    else
                    {
                        s.Close();
                        result = "Problem";
                    }
                }
                catch { s.Close(); result = "Closed"; }
                s = null; IPs = null;
            }
            catch { result = "Wrong"; }
 
            statusPort = true;
            ResultCheckPort = result;
            if (statusPing==true) status = true;
            result = null;
        }
    }
}
