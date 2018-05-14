using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using JetBrains.Annotations;
using UnityEngine;

public partial class GreenWorld : MonoBehaviour
{
    public int Port = 5634;//set in inspector

    private List<AdapterListener> _adapterListeners;

    [SerializeField]
    private List<WorldMessageProvider> _messageProviders = new List<WorldMessageProvider>();

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
    private byte[] GetHeaderData(int messageTypeIdentifier, int messageLength)
    {
        byte[] headerBuffer = new byte[8];
        headerBuffer[0] = (byte)messageTypeIdentifier;
        headerBuffer[1] = (byte)(messageTypeIdentifier >> 8);
        headerBuffer[2] = (byte)(messageTypeIdentifier >> 16);
        headerBuffer[3] = (byte)(messageTypeIdentifier >> 24);
        headerBuffer[4] = (byte)messageLength;
        headerBuffer[5] = (byte)(messageLength >> 8);
        headerBuffer[6] = (byte)(messageLength >> 16);
        headerBuffer[7] = (byte)(messageLength >> 24);
        return headerBuffer;
    }

    /// <summary>
    /// Sends a world message with a certain 
    /// </summary>
    /// <param name="adapterListener"></param>
    /// <param name="worldMessage"></param>
    /// <param name="messageTypeIdentifier"></param>
    public void SendWorldMessage(AdapterListener adapterListener, byte[] worldMessage, int messageTypeIdentifier)
    {
        byte[] headerBytes = GetHeaderData(messageTypeIdentifier, worldMessage.Length);
        adapterListener.WriteData(headerBytes);
        adapterListener.WriteData(worldMessage);
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

    //independent of framerate
    [UsedImplicitly]
    private void FixedUpdate()
    {
        
        foreach (var item in _adapterListeners)
        {

            NetworkStream networkStream = item.GetStream();

            if(networkStream==null)
                continue;

            if (networkStream.DataAvailable)
            {
                GetStreamData(networkStream, _bufferStream);

                while (_bufferStream.Length > 8)
                {
                    byte[] buffer = _bufferStream.GetBuffer();
                    int cPos = 0;
                    int typeIdentifier = (buffer[cPos++] | buffer[cPos++] << 8 | buffer[cPos++] << 16 | buffer[cPos++] << 24);
                    int messageLength = (buffer[cPos++] | buffer[cPos++] << 8 | buffer[cPos++] << 16 | buffer[cPos++] << 24);

                    int end = cPos + messageLength;

                    if (_bufferStream.Length <= end)
                    {
                        byte[] message = new byte[messageLength];

                        int remaining = (int)_bufferStream.Length - end;
                        byte[] remainingBytes = new byte[remaining];

                        _bufferStream.Position = cPos;
                        _bufferStream.Read(message, 0, messageLength);
                        _bufferStream.Read(remainingBytes, 0, remaining);

                        _bufferStream.Position = 0;
                        _bufferStream.SetLength(0);
                        _bufferStream.Write(remainingBytes, 0, remaining);
                        bool handled = false;

                        int providersCount = _messageProviders.Count;
                        for (int i = 0; i < providersCount; i++)
                        {
                            if (_messageProviders[i].GetTypeIdentifier() == typeIdentifier)
                            {
                                _messageProviders[i].HandleMessage(this, item, message);
                                handled = true;
                                break;
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

}
