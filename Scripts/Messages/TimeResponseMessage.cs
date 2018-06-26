using System;

namespace GreenProject.Messages
{
    public class TimeResponseMessage : INetworkMessage
    {
        private DateTime _dateTime;

        public TimeResponseMessage(DateTime time)
        {
            _dateTime = time;
        }

        public TimeResponseMessage()
        {

        }

        public DateTime DateTime
        {
            get { return _dateTime; }
        }

        public byte[] SerializeData()
        {
            return BitConverter.GetBytes(DateTime.ToBinary());
        }

        public void DeserializeData(byte[] data)
        {
            long binary = BitConverter.ToInt64(data, 0);
            _dateTime = DateTime.FromBinary(binary);
        }
    }
}