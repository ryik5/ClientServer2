using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace myClientServer
{

    public partial class Form1 : System.Windows.Forms.Form
    {

https://metanit.com/sharp/net/4.4.php

        /////��� ����������� ����
        //C�����
        private ServerObject server;
        private Thread listenThread; // ����� ��� �������������
                                     // Client
        private string userName;
        private const string host = "127.0.0.1";
        private const int port = 8888;
        private TcpClient client;
        private NetworkStream stream;

        public Form1()
        {
            InitializeComponent();
        }

        //���������� ���
        private void ChatClient()
        {
            userName = Environment.MachineName;
            client = new TcpClient();
            try
            {
                client.Connect(host, port); //����������� �������
                stream = client.GetStream(); // �������� �����

                string message = userName;
                byte[] data = System.Text.Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // ��������� ����� ����� ��� ��������� ������
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //����� ������
                _richTextBoxEchoAdd("����� ����������, " + userName);
                SendMessage();
            }
            catch (Exception ex)
            { _richTextBoxEchoAdd(ex.Message); }
            finally
            { Disconnect(); }
        }

        private void SendMessage()   // �������� ���������
        {
            while (true)
            {
                string message = DecryptRijndael(_textBoxMessageText(), salt);
                byte[] data = System.Text.Encoding.Unicode.GetBytes(message);

                stream.Write(data, 0, data.Length);
            }
        }

        private void ReceiveMessage()    // ��������� ���������
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // ����� ��� ���������� ������
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(System.Text.Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = EncryptRijndael(builder.ToString(), salt);
                    _richTextBoxEchoAdd(message);//����� ���������
                }
                catch
                {
                    _richTextBoxEchoAdd("����������� ��������!"); //���������� ���� ��������
                    Disconnect();
                }
            }
        }

        private void Disconnect()
        {
            if (stream != null)
            { stream.Close(); } //���������� ������
            if (client != null)
            { client.Close(); } //���������� �������
                                // Environment.Exit(0); //���������� ��������
        }
        private void SetServer1()
        {
            /////��� ����������� ����
            server = new ServerObject();
            try
            {
                listenThread = new Thread(new ParameterizedThreadStart((obj) => server.Listen()));
                listenThread.Start(); //����� ������
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
            /////
        }

    }

    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        string userName;
        TcpClient client;
        ServerObject server; // ������ �������


        

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();   // �������� ��� ������������
             
                string message = GetMessage();
                userName = message;
                message = userName + " ����� � ���";
                
                // �������� ��������� � ����� � ��� ���� ������������ �������������
                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message);

                // � ����������� ����� �������� ��������� �� �������
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = String.Format("{0}: {1}", userName, message);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                    }
                    catch
                    {
                        message = String.Format("{0}: ������� ���", userName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {            
                server.RemoveConnection(this.Id);    // � ������ ������ �� ����� ��������� �������
                Close();
            }
        }

        // ������ ��������� ��������� � �������������� � ������
        private string GetMessage()
        {
            byte[] data = new byte[64]; // ����� ��� ���������� ������
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(System.Text.Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        protected internal void Close()   // �������� �����������
        {
            if (Stream != null)
            { Stream.Close(); }
            if (client != null)
            { client.Close(); }
        }
    }

    public class ServerObject
    {
        private TcpListener tcpListener; // ������ ��� �������������
        private System.Collections.Generic.List<ClientObject> clients = new System.Collections.Generic.List<ClientObject>(); // ��� �����������

        protected internal void AddConnection(ClientObject clientObject)
        { clients.Add(clientObject); }

        protected internal void RemoveConnection(string id)
        {         
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);   // �������� �� id �������� �����������
          
            if (client != null)  // � ������� ��� �� ������ �����������
            { clients.Remove(client); }
        }
      
        protected internal void Listen()  // ������������� �������� �����������
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("������ �������. �������� �����������...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal void BroadcastMessage(string message, string id)  // ���������� ��������� ������������ ��������
        {
            byte[] data = System.Text.Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id) // ���� id ������� �� ����� id �������������
                {
                    clients[i].Stream.Write(data, 0, data.Length); //�������� ������
                }
            }
        }
     
        protected internal void Disconnect()   // ���������� ���� ��������
        {
            tcpListener.Stop(); //��������� �������

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //���������� �������
            }
           // Environment.Exit(0); //���������� ��������
        }
    }
}