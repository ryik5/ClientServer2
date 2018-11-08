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

        /////Для консольного чата
        //Cервер
        private ServerObject server;
        private Thread listenThread; // поток для прослушивания
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

        //Консольный чат
        private void ChatClient()
        {
            userName = Environment.MachineName;
            client = new TcpClient();
            try
            {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток

                string message = userName;
                byte[] data = System.Text.Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //старт потока
                _richTextBoxEchoAdd("Добро пожаловать, " + userName);
                SendMessage();
            }
            catch (Exception ex)
            { _richTextBoxEchoAdd(ex.Message); }
            finally
            { Disconnect(); }
        }

        private void SendMessage()   // отправка сообщений
        {
            while (true)
            {
                string message = DecryptRijndael(_textBoxMessageText(), salt);
                byte[] data = System.Text.Encoding.Unicode.GetBytes(message);

                stream.Write(data, 0, data.Length);
            }
        }

        private void ReceiveMessage()    // получение сообщений
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(System.Text.Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = EncryptRijndael(builder.ToString(), salt);
                    _richTextBoxEchoAdd(message);//вывод сообщения
                }
                catch
                {
                    _richTextBoxEchoAdd("Подключение прервано!"); //соединение было прервано
                    Disconnect();
                }
            }
        }

        private void Disconnect()
        {
            if (stream != null)
            { stream.Close(); } //отключение потока
            if (client != null)
            { client.Close(); } //отключение клиента
                                // Environment.Exit(0); //завершение процесса
        }
        private void SetServer1()
        {
            /////Для консольного чата
            server = new ServerObject();
            try
            {
                listenThread = new Thread(new ParameterizedThreadStart((obj) => server.Listen()));
                listenThread.Start(); //старт потока
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
        ServerObject server; // объект сервера


        

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
                Stream = client.GetStream();   // получаем имя пользователя
             
                string message = GetMessage();
                userName = message;
                message = userName + " вошел в чат";
                
                // посылаем сообщение о входе в чат всем подключенным пользователям
                server.BroadcastMessage(message, this.Id);
                Console.WriteLine(message);

                // в бесконечном цикле получаем сообщения от клиента
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
                        message = String.Format("{0}: покинул чат", userName);
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
                server.RemoveConnection(this.Id);    // в случае выхода из цикла закрываем ресурсы
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
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

        protected internal void Close()   // закрытие подключения
        {
            if (Stream != null)
            { Stream.Close(); }
            if (client != null)
            { client.Close(); }
        }
    }

    public class ServerObject
    {
        private TcpListener tcpListener; // сервер для прослушивания
        private System.Collections.Generic.List<ClientObject> clients = new System.Collections.Generic.List<ClientObject>(); // все подключения

        protected internal void AddConnection(ClientObject clientObject)
        { clients.Add(clientObject); }

        protected internal void RemoveConnection(string id)
        {         
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);   // получаем по id закрытое подключение
          
            if (client != null)  // и удаляем его из списка подключений
            { clients.Remove(client); }
        }
      
        protected internal void Listen()  // прослушивание входящих подключений
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

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

        protected internal void BroadcastMessage(string message, string id)  // трансляция сообщения подключенным клиентам
        {
            byte[] data = System.Text.Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id) // если id клиента не равно id отправляющего
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                }
            }
        }
     
        protected internal void Disconnect()   // отключение всех клиентов
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
           // Environment.Exit(0); //завершение процесса
        }
    }
}