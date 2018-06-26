using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using GreenProject.Messages;
using JetBrains.Annotations;
using UnityEngine;

public partial class GreenWorld : MonoBehaviour
{
    public int Port = 5634;//set in inspector
    private const int HeaderLength = 12;

    private List<AdapterListener> _adapterListeners;

    private readonly List<ReceiveListener> _messageListeners = new List<ReceiveListener>();

    public delegate void MessageReceived<T>(AdapterListener adapter, T networkMessage) where T : class, INetworkMessage;

    public class ReceiveListener
    {
        public Delegate MessageReceived;
        public Type MessageType;
        public long MessageUniqueIdentifier;
    }

    private readonly MemoryStream _bufferStream = new MemoryStream();

    [UsedImplicitly]
    private void Start()
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

        _adapterListeners = new List<AdapterListener>(ipHostInfo.AddressList.Length);

        for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
        {
            if (ipHostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
            {
                Debug.Log("Adapter ipv4 " + ipHostInfo.AddressList[i]);
                AdapterListener adapterListener = new AdapterListener(ipHostInfo.AddressList[i], Port);
                adapterListener.StartListenThread();
                _adapterListeners.Add(adapterListener);
            }
        }

    }

    [UsedImplicitly]
    private void OnDestroy()
    {
        foreach (var item in _adapterListeners)
        {
            item.Close();
        }
    }

    /// <summary>
    /// Returns 8 bytes wich contain the message typeIdentifier
    /// </summary>
    /// <param name="messageTypeIdentifier"></param>
    /// <param name="messageLength"></param>
    /// <returns></returns>
    private byte[] GetHeaderData(long messageTypeIdentifier, int messageLength)
    {
        byte[] headerBuffer = new byte[HeaderLength];
        byte index = 0;
        headerBuffer[index++] = (byte)messageTypeIdentifier;
        headerBuffer[index++] = (byte)(messageTypeIdentifier >> 8);
        headerBuffer[index++] = (byte)(messageTypeIdentifier >> 16);
        headerBuffer[index++] = (byte)(messageTypeIdentifier >> 24);
        headerBuffer[index++] = (byte)(messageTypeIdentifier >> 32);
        headerBuffer[index++] = (byte)(messageTypeIdentifier >> 40);
        headerBuffer[index++] = (byte)(messageTypeIdentifier >> 48);
        headerBuffer[index++] = (byte)(messageTypeIdentifier >> 56);
        headerBuffer[index++] = (byte)messageLength;
        headerBuffer[index++] = (byte)(messageLength >> 8);
        headerBuffer[index++] = (byte)(messageLength >> 16);
        headerBuffer[index++] = (byte)(messageLength >> 24);
        return headerBuffer;
    }

    public void SendMessage<T>(AdapterListener adapterListener, T message) where T : INetworkMessage
    {
        byte[] serializeData = message.SerializeData();
        byte[] headerBytes = GetHeaderData(MessageExtensions.GetMessageUniqueIdentifier<T>(), serializeData.Length);
        adapterListener.WriteData(headerBytes);
        adapterListener.WriteData(serializeData);
    }


    /// <summary>
    /// Removes a listener
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool RemoveMessageListener<T>(MessageReceived<T> listener) where T : class, INetworkMessage, new()
    {
        int index = _messageListeners.FindIndex((a) => a.MessageReceived == (Delegate)listener);
        if (index != -1)
        {
            _messageListeners.RemoveAt(index);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Adds a listener
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void AddMessageListener<T>(MessageReceived<T> listener) where T : class, INetworkMessage, new()
    {
        ReceiveListener entry = new ReceiveListener
        {
            MessageReceived = listener,
            MessageUniqueIdentifier = MessageExtensions.GetMessageUniqueIdentifier<T>(),
            MessageType = typeof(T)
        };

        _messageListeners.Add(entry);
    }

    /// <summary>
    /// Will read all the bytes from a <see cref="NetworkStream"/>
    /// </summary>
    /// <param name="networkStream"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    private static void GetStreamData(NetworkStream networkStream, MemoryStream output)
    {
        int tempBufferSize = 1024;
        byte[] tempBuffer = new byte[tempBufferSize];
        int readBytes;
        while ((readBytes = networkStream.Read(tempBuffer, 0, tempBufferSize)) > 0)
        {
            output.Write(tempBuffer, 0, readBytes);
            if (readBytes < tempBufferSize)
            {
                break;
            }
        }
    }

    private static void ReadHeader(byte[] buffer, out long typeIdentifier, out int messageLength)
    {
        int cPos = 0;
        uint lo = (uint)(buffer[cPos++] | buffer[cPos++] << 8 | buffer[cPos++] << 16 | buffer[cPos++] << 24);
        uint hi = (uint)(buffer[cPos++] | buffer[cPos++] << 8 | buffer[cPos++] << 16 | buffer[cPos++] << 24);
        typeIdentifier = (long)((ulong)hi) << 32 | lo;
        messageLength = (buffer[cPos++] | buffer[cPos++] << 8 | buffer[cPos++] << 16 | buffer[cPos++] << 24);
    }

    //independent of framerate
    [UsedImplicitly]
    private void FixedUpdate()
    {

        foreach (var adapterListener in _adapterListeners)
        {

            NetworkStream networkStream = adapterListener.GetStream();

            if (networkStream == null)
                continue;

            if (networkStream.DataAvailable)
            {
                GetStreamData(networkStream, _bufferStream);
            }

            while (_bufferStream.Length >= HeaderLength)
            {
                byte[] buffer = _bufferStream.GetBuffer();
                long typeIdentifier;
                int messageLength;

                ReadHeader(buffer, out typeIdentifier, out messageLength);

                int end = HeaderLength + messageLength;

                if (_bufferStream.Length <= end)
                {
                    byte[] message = new byte[messageLength];

                    int remaining = (int)_bufferStream.Length - end;
                    byte[] remainingBytes = new byte[remaining];

                    _bufferStream.Position = HeaderLength;
                    _bufferStream.Read(message, 0, messageLength);
                    _bufferStream.Read(remainingBytes, 0, remaining);

                    _bufferStream.Position = 0;
                    _bufferStream.SetLength(0);
                    _bufferStream.Write(remainingBytes, 0, remaining);
                    bool handled = false;

                    int providersCount = _messageListeners.Count;
                    for (int i = 0; i < providersCount; i++)
                    {
                        if (_messageListeners[i].MessageUniqueIdentifier == typeIdentifier)
                        {
                            INetworkMessage networkMessage = Activator.CreateInstance(_messageListeners[i].MessageType) as INetworkMessage;
                            networkMessage.DeserializeData(message);
                            _messageListeners[i].MessageReceived.DynamicInvoke(adapterListener,networkMessage);
                            handled = true;
                        }
                    }
                    if (handled == false)
                    {
                        Debug.LogError($"Message with id {typeIdentifier} have no listener!");
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

}
