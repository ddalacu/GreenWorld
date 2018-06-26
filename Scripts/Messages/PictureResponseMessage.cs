namespace GreenProject.Messages
{
    public class PictureResponseMessage : INetworkMessage
    {
        private byte[] _imageBytes;

        public PictureResponseMessage(byte[] picturBytes)
        {
            _imageBytes = picturBytes;
        }

        public PictureResponseMessage()
        {
            
        }

        public byte[] ImageBytes
        {
            get { return _imageBytes; }
        }

        public byte[] SerializeData()
        {
            return _imageBytes;
        }

        public void DeserializeData(byte[] data)
        {
            _imageBytes = data;
        }
    }
}