using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public partial class GreenWorld
{
    /// <summary>
    /// This class will listen on a cerain adapter
    /// </summary>
    public class AdapterListener
    {

        private TcpListener _tcpListener;

        /// <summary>
        /// Used to 'pause' the endless loop in <see cref="Listen"/>
        /// </summary>
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        private TcpClient _client;

        /// <summary>
        /// used to lock the <see cref="_client"/>
        /// </summary>
        private readonly object _clientLock = new object();

        /// <summary>
        /// The thread used to listen
        /// </summary>
        private Thread _listenThread;

        public AdapterListener(IPAddress iPAddress, int port)
        {
            _tcpListener = new TcpListener(iPAddress, port);
        }

        public void StartListenThread()
        {
            Debug.Log("Starting thread " + ToString());

            _listenThread = new Thread(Listen);
            _listenThread.Start();
        }

        private void Listen()
        {
            try
            {
                _tcpListener.Start();//this might throw
                Debug.Log("Listening to " + ToString());
                while (true)//we have a while to allow clients to reconnect
                {
                    _manualResetEvent.Reset();
                    _tcpListener.BeginAcceptTcpClient(AcceptCallback, _tcpListener);
                    _manualResetEvent.WaitOne();
                }
            }
            catch (Exception e)
            {
                _tcpListener = null;
                Debug.Log(ToString() + "  " + e);
            }
        }
        /// <summary>
        /// Accept a client
        /// </summary>
        /// <param name="ar"></param>
        public void AcceptCallback(IAsyncResult ar)
        {
            _manualResetEvent.Set();
            lock (_clientLock)
            {
                _client = _tcpListener.EndAcceptTcpClient(ar);
                if (_client != null)
                {
                    Debug.Log("Client connected " + _client.Client.RemoteEndPoint);
                }
            }
        }

        public NetworkStream GetStream()
        {
            lock (_clientLock)
            {
                return _client?.GetStream();
            }
        }

        public void WriteData(byte[] data)
        {
            lock (_clientLock)
            {
                if (_client == null || _client.Connected == false)
                {
                    return;
                }

                NetworkStream networkStream = _client.GetStream();
                networkStream.Write(data, 0, data.Length);
            }
        }

        public void Close()
        {
            if (_listenThread != null && _listenThread.IsAlive)
            {
                _listenThread.Abort();
            }

            if (_tcpListener != null)
            {
                _tcpListener.Stop();
            }
        }
    }

}
