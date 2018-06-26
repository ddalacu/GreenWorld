using GreenProject.Messages;

namespace GreenProject.Sensors.Temperature
{
    public class GetTemperatureMessage : INetworkMessage
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