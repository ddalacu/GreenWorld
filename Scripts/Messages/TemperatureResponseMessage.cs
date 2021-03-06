﻿using System;

namespace GreenProject.Messages
{
    public class TemperatureResponseMessage : INetworkMessage
    {
        private float _temperature;

        public float Temperature
        {
            get
            {
                return _temperature;
            }
        }

        public TemperatureResponseMessage(float temperature)
        {
            this._temperature = temperature;
        }

        public TemperatureResponseMessage()
        {
            
        }

        public byte[] SerializeData()
        {
            return BitConverter.GetBytes(_temperature);
        }

        public void DeserializeData(byte[] data)
        {
            _temperature = BitConverter.ToSingle(data, 0);
        }
    }
}
