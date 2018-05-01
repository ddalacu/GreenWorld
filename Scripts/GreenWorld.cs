using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public partial class GreenWorld : MonoBehaviour
{
    public int Port = 5634;//set in inspector

    private List<AdapterListener> adapterListeners;

    [SerializeField]
    private List<WorldMessageProvider> messageProviders = new List<WorldMessageProvider>();

    private void Start()
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

        adapterListeners = new List<AdapterListener>(ipHostInfo.AddressList.Length);

        for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
        {
            if (ipHostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
            {
                Debug.Log("Adapter ipv4 " + ipHostInfo.AddressList[i]);
                AdapterListener adapterListener = new AdapterListener(ipHostInfo.AddressList[i], Port);
                adapterListener.StartListenThread();
                adapterListeners.Add(adapterListener);
            }
        }

    }

    private void OnDestroy()
    {
        foreach (var item in adapterListeners)
        {
            item.Close();
        }
    }

    /// <summary>
    /// Returns 8 bytes wich contain the message typeIdentifier
    /// </summary>
    /// <param name="stream"></param>
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
    /// <param name="worldMessage"></param>
    /// <param name="messageTypeIdentifier"></param>
    public void SendWorldMessage(AdapterListener adapterListener, byte[] worldMessage, int messageTypeIdentifier)
    {
        byte[] headerBytes = GetHeaderData(messageTypeIdentifier, worldMessage.Length);
        adapterListener.WriteData(headerBytes);
        adapterListener.WriteData(worldMessage);
    }

    private List<KeyValuePair<int, byte[]>> GetMessageData(byte[] buffer, long length)
    {
        List<KeyValuePair<int, byte[]>> messageDatas = new List<KeyValuePair<int, byte[]>>();
        int cPos = 0;

        while (cPos < length)
        {
            if (cPos + 8 >= length)
            {
                Debug.Log("Bad data!");
            }

            int typeIdentifier = (buffer[cPos++] | buffer[cPos++] << 8 | buffer[cPos++] << 16 | buffer[cPos++] << 24);
            int messageLength = (buffer[cPos++] | buffer[cPos++] << 8 | buffer[cPos++] << 16 | buffer[cPos++] << 24);

            if (length >= cPos + messageLength)
            {
                byte[] data = new byte[messageLength];
                Array.Copy(buffer, cPos, data, 0, messageLength);
                cPos += messageLength;
                messageDatas.Add(new KeyValuePair<int, byte[]>(typeIdentifier, data));
            }
            else
            {
                Debug.Log("Bad data!");
            }
        }

        return messageDatas;
    }

    //independent of framerate
    private void FixedUpdate()
    {
        foreach (var item in adapterListeners)
        {
            long length = 0;
            byte[] data = item.ReadData(ref length);

            foreach (var message in GetMessageData(data, length))
            {
                bool handled = false;
                int providersCount = messageProviders.Count;
                for (int i = 0; i < providersCount; i++)
                {
                    if (messageProviders[i].GetTypeIdentifier() == message.Key)
                    {
                        messageProviders[i].HandleMessage(this, item, message.Value);
                        handled = true;
                        break;
                    }
                }

                if (handled == false)
                {
                    Debug.LogError($"Message with id {message.Key} have no listener!");
                }
            }

        }

    }

}
