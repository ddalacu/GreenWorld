using System.Text;
using UnityEngine;

namespace GreenProject.Messages
{
    public class ControllerResultMessage : INetworkMessage
    {
        [SerializeField]
        private ControllerResult _result;
        [SerializeField]
        private int _controllerIdentifier;

        public ControllerResultMessage(ControllerResult result, int controllerIdentifier)
        {
            _result = result;
            _controllerIdentifier = controllerIdentifier;
        }

        public ControllerResultMessage()
        {

        }

        public ControllerResult GetResult()
        {
            return _result;
        }

        public int GetControllerIdentifier()
        {
            return _controllerIdentifier;
        }

        public byte[] SerializeData()
        {
            return Encoding.ASCII.GetBytes(JsonUtility.ToJson(this));
        }

        public void DeserializeData(byte[] data)
        {
            string json = Encoding.ASCII.GetString(data);
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}