using System.Text;
using GreenProject.Messages;
using UnityEngine;

namespace GreenProject.Controllers
{
    public class ControllerProgressMessage : INetworkMessage
    {
        [SerializeField]
        private float _progress;
        [SerializeField]
        private int _controllerIdentifier;

        public ControllerProgressMessage(float progress, int controllerIdentifier)
        {
            _progress = progress;
            _controllerIdentifier = controllerIdentifier;
        }

        public ControllerProgressMessage()
        {

        }

        public float GetProgress()
        {
            return _progress;
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