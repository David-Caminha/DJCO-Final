using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Object = System.Object;

public delegate void TcpClientEventHandler();
public delegate void TcpClientMessageEventHandler(String subject, Object data);

public class MessageClient
{
    private readonly TcpClient _client;

    private readonly NetworkStream _clientConnection;
    private readonly StreamReader _clientStreamReader;
    private readonly StreamWriter _clientStreamWriter;

    private readonly Queue<Action> _actions = new Queue<Action>();

    public event TcpClientMessageEventHandler MessageReceived = delegate { };
    //public event TcpClientEventHandler ClientConnected = delegate { };
    public event TcpClientEventHandler ClientDisconnected = delegate { };

    public MessageClient(string hostname, int port)
    {
        _client = new TcpClient(hostname, port);
        
        _clientConnection = _client.GetStream();
        _clientStreamReader = new StreamReader(_clientConnection);
        _clientStreamWriter = new StreamWriter(_clientConnection);

        new Thread(ProcessMessages).Start();
    }

    public bool Connected
    {
        get { return _client != null && _client.Connected; }
    }


    private void ProcessMessages()
    {
        try
        {
            while (true)
            {
                string readLine = _clientStreamReader.ReadLine();
                if (readLine != null)
                {
                    TcpMessage message = JsonSerialization.FromString<TcpMessage>(readLine);
                    lock (_actions)
                    {
                        _actions.Enqueue(() => MessageReceived(message.Subject, message.Data));
                    }
                }
            }
        }
        //when the connection is closed, this exception will be thrown, ending the threaded function
        catch (IOException)
        {
        }
    }

    /// <summary>
    /// Executes the events in the main thread.
    /// </summary>
    public void Synchonize()
    {
        lock (_actions)
        {
            while (_actions.Count > 0)
            {
                _actions.Dequeue().Invoke();
            }
        }
    }


    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="data"></param>
    public void SendMessage(String subject, Object data)
    {
        if (_client.Connected)
        {
            //var message = new {subject, data};
            string message = JsonSerialization.ToString(new TcpMessage(subject, data));
            //JsonSerialization.ToString(message);

            _clientStreamWriter.WriteLine(message);
            _clientStreamWriter.Flush();
        }

    }


    /// <summary>
    /// Closes the connection.
    /// </summary>
    public void Disconnect()
    {
        _clientStreamWriter.Close();
        _clientStreamReader.Close();
        _clientConnection.Close();
        _client.Close();

        ClientDisconnected();
    }
}
