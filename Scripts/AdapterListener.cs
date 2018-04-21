using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public partial class GreenWorld
{
    /// <summary>
    /// This class will listen on a cerain adapter
    /// </summary>
    public class AdapterListener
    {

        private TcpListener tcpListener;

        /// <summary>
        /// Used to 'pause' the endless loop in <see cref="Listen"/>
        /// </summary>
        private ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        private TcpClient client;

        /// <summary>
        /// used to lock the <see cref="clients"/>
        /// </summary>
        private object clientLock = new object();

        /// <summary>
        /// The thread used to listen
        /// </summary>
        private Thread listenThread;

        public AdapterListener(IPAddress iPAddress, int port)
        {
            tcpListener = new TcpListener(iPAddress, port);
        }

        public void StartListenThread()
        {
            Debug.Log("Starting thread " + ToString());

            listenThread = new Thread(new ThreadStart(Listen));
            listenThread.Start();
        }

        private void Listen()
        {
            try
            {
                tcpListener.Start();//this might throw
                Debug.Log("Listening to " + ToString());
                while (true)//we have a while to allow clients to reconnect
                {
                    manualResetEvent.Reset();
                    tcpListener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), tcpListener);
                    manualResetEvent.WaitOne();
                }
            }
            catch (Exception e)
            {
                tcpListener = null;
                Debug.Log(ToString() + "  " + e.ToString());
            }
        }
        /// <summary>
        /// Accept a client
        /// </summary>
        /// <param name="ar"></param>
        public void AcceptCallback(IAsyncResult ar)
        {
            manualResetEvent.Set();
            lock (clientLock)
            {
                client = tcpListener.EndAcceptTcpClient(ar);
                if (client != null)
                {
                    Debug.Log("Client connected " + client.Client.RemoteEndPoint.ToString());
                }
            }
        }

        /// <summary>
        /// Will read all the bytes from a <see cref="NetworkStream"/>
        /// </summary>
        /// <param name="networkStream"></param>
        /// <param name="readSize">out</param>
        /// <returns></returns>
        private static byte[] GetStreamData(NetworkStream networkStream, out long readSize)
        {
            int tempBufferSize = 2048;
            byte[] tempBuffer = new byte[tempBufferSize];//we allocate a temp buffer

            //we read from the network stream into the temp buffer
            int readBytes = networkStream.Read(tempBuffer, 0, tempBufferSize);

            if (readBytes < tempBufferSize)//the read bytes are less than the temp buffer size and so we can return the temp buffer
            {
                readSize = readBytes;
                return tempBuffer;
            }

            //We crate a expanding buffer and add data from the network stream
            MemoryStream memoryStream = new MemoryStream(tempBufferSize * 2);
            memoryStream.Write(tempBuffer, 0, tempBufferSize);

            while ((readBytes = networkStream.Read(tempBuffer, 0, tempBufferSize)) > 0)
            {
                memoryStream.Write(tempBuffer, 0, tempBufferSize);
            }

            readSize = memoryStream.Length;
            return memoryStream.GetBuffer();
        }

        /// <summary>
        /// Returns data from clients, we don't care who client sent it
        /// </summary>
        /// <returns></returns>
        public byte[] ReadData(ref long length)
        {
            lock (clientLock)
            {
                if (client == null || client.Connected == false)
                {
                    return null;
                }

                NetworkStream networkStream = client.GetStream();

                if (networkStream.DataAvailable)
                {
                    return GetStreamData(networkStream, out length);
                }
            }

            return null;
        }

        public void Close()
        {
            if (listenThread != null && listenThread.IsAlive)
            {
                listenThread.Abort();
            }

            if (tcpListener != null)
            {
                tcpListener.Stop();
            }
        }
    }

}
