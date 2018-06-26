namespace GreenProject.Messages
{
    public class TimeRequestMessage : INetworkMessage
    {
        public byte[] SerializeData()
        {
            return new byte[0];
        }

        public void DeserializeData(byte[] data)
        {

        }
    }
}