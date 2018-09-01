using System;

namespace GreenProject.Messages
{
    public class LightResponseMessage : INetworkMessage
    {
        public enum LightStatus
        {
            Light = 0,
            Dark = 1
        }

        public LightStatus Status;

        public LightResponseMessage(LightStatus status)
        {
            Status = status;
        }

        public byte[] SerializeData()
        {
            return BitConverter.GetBytes((int)Status);
        }

        public void DeserializeData(byte[] data)
        {
            int status = BitConverter.ToInt32(data, 0);
            Status = (LightStatus)status;
        }
    }
}
;