using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myClientServer
{
    public class EndPointHost
    {
        public System.Net.IPHostEntry HostName { get; set; }
        public System.Net.IPAddress ipAddress { get; set; }
        public int intPortChat { get; set; } //= 1200;
        public int intPortGetFile { get; set; } //= 1300;
        public string UserName { get; private set; } //HostName(ipAddress)
        
        public string iD { get; private set; }

        public void SetInfo()
        {
            SetId();
            SetUserName();
            SetPortChat();
            SetPortGetFile();

            try { Form1.myForm._statusLabelText(Form1.myForm.StatusLabel2, " " + Form1.myForm.RemoteHost.UserName); } catch { }
        }

        private void SetId()
        {
            iD = Guid.NewGuid().ToString();
        }

        private void SetUserName()
        {
            if (HostName != null && ipAddress != null)
            { UserName = HostName.HostName.ToString().ToUpper() + "(" + ipAddress.ToString() + ")"; }
            else if (HostName == null && ipAddress != null)
            { UserName = ipAddress.ToString() + "(" + ipAddress.ToString() + ")"; }
            else if (HostName != null && ipAddress == null)
            { UserName = HostName.HostName.ToString().ToUpper() + "(" + HostName.HostName.ToString().ToUpper() + ")"; }
            else { UserName = ""; }
        }

        private void SetPortChat()
        { intPortChat = 1200; }

        private void SetPortGetFile()
        { intPortGetFile = 1300; }

    }
}
