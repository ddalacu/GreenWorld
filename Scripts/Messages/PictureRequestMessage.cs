namespace GreenProject.Messages
{
    public class PictureRequestMessage : INetworkMessage
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