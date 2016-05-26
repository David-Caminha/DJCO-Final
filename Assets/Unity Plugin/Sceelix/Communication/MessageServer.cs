using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;


    public delegate void TcpServerMessageEventHandler(MessageServer.ClientConnection clientConnection, String subject, Object data);
    public delegate void TcpServerRawMessageEventHandler(MessageServer.ClientConnection clientConnection, String message);
    public delegate void TcpServerInformationEventHandler(MessageServer.ClientConnection clientConnection);

    public class MessageServer
    {
        private readonly TcpListener _server;
        private readonly List<ClientConnection> _connections = new List<ClientConnection>();
        private readonly Queue<Action> _actions = new Queue<Action>();

        public event TcpServerMessageEventHandler MessageReceived = delegate { };
        public event TcpServerRawMessageEventHandler RawMessageReceived = delegate { };
        public event TcpServerInformationEventHandler ClientConnected = delegate { };
        public event TcpServerInformationEventHandler ClientDisconnected = delegate { };


        public MessageServer(IPAddress ipAddress, int port)
        {
            _server = new TcpListener(ipAddress, port);
            _server.Start();

            //start the new thread for collecting clients
            new Thread(AwaitClients){IsBackground = true}.Start();
        }


        public void Synchronize()
        {
            lock (_connections)
            {
                foreach (ClientConnection clientConnection in Connections.Where(x => !x.IsConnected))
                    ClientDisconnected(clientConnection);
                
                //remove dead connections
                _connections.RemoveAll(x => !x.IsConnected);
            }

            //execute the actions in the main thread
            lock (_actions)
            {
                while (_actions.Count > 0)
                {
                    _actions.Dequeue().Invoke();
                }
            }
        }


        private void AwaitClients()
        {
            try{

                while (true)
                {
                    //this blocks here
                    TcpClient client = _server.AcceptTcpClient();

                    var clientConnection = new ClientConnection(client, this);

                    //save a reference of the connection
                    _connections.Add(clientConnection);

                    //run this in the main thread
                    _actions.Enqueue(() => ClientConnected(clientConnection));
                }
            }
            catch{}
        }


        public void DisconnectAll()
        {
            lock (_connections)
            {
                foreach (ClientConnection clientConnection in _connections)
                    clientConnection.Disconnect();

                _connections.Clear();
            }
        }


        /// <summary>
        /// Disconnects all the clients and closes the server socket.
        /// </summary>
        public void Close()
        {
            DisconnectAll();

            _server.Stop();
        }
        

        /// <summary>
        /// Sends messages to all the Clients.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="data"></param>
        public void SendMessage(String subject, Object data)
        {
            lock (_connections)
            {
                //remove dead connections
                _connections.RemoveAll(x => !x.IsConnected);

                foreach (ClientConnection clientConnection in _connections)
                {
                    clientConnection.SendMessage(subject, data);
                }
            }
        }


        public List<ClientConnection> Connections
        {
            get { return _connections; }
        }






        public class ClientConnection
        {
            private readonly TcpClient _client;
            private readonly MessageServer _messageServer;
            private readonly StreamReader _clientStreamReader;
            private readonly StreamWriter _clientStreamWriter;
            private readonly NetworkStream _clientSockStream;

            public ClientConnection(TcpClient client, MessageServer messageServer)
            {
                _client = client;
                _messageServer = messageServer;

                _clientSockStream = client.GetStream();

                _clientStreamReader = new StreamReader(_clientSockStream);
                _clientStreamWriter = new StreamWriter(_clientSockStream) {AutoFlush = true};

                new Thread(ProcessMessages).Start();
            }

            /// <summary>
            /// Processes messages in a separate thread
            /// </summary>
            private void ProcessMessages()
            {
                try
                {
                    while (true)
                    {
                        string readLine = _clientStreamReader.ReadLine();
                        if (readLine != null)
                        {
                            _messageServer._actions.Enqueue(() => _messageServer.RawMessageReceived(this, readLine));

                            TcpMessage message = JsonSerialization.FromString<TcpMessage>(readLine);

                            _messageServer._actions.Enqueue(() => _messageServer.MessageReceived(this, message.Subject, message.Data));
                        }
                    }
                }
                catch (IOException)
                {
                    //call client disconnected
                    /*lock (_actions)
                    {
                        _actions.Enqueue(() => _messageServer.ClientDisconnected(this));
                    }*/
                }
            }


            public void SendMessage(string subject, object data)
            {
                string message = JsonSerialization.ToString(new TcpMessage(subject, data));

                //var size = message.Length*sizeof (Char)/1024;

                _clientStreamWriter.WriteLine(message);
            }


            public void Disconnect()
            {
                _clientStreamWriter.Close();
                _clientStreamReader.Close();
                _clientSockStream.Close();
                _client.Close();
            }


            public bool IsConnected
            {
                get { return _client.Connected; }
            }
        }
    }