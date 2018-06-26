using System.Collections;
using System.Collections.Generic;
using System.Text;
using GreenProject.Messages;
using UnityEngine;

namespace GreenProject.Controllers
{
    public class ControllerInputData : INetworkMessage
    {
        [SerializeField] public int ControllerIdentifier;

        public virtual byte[] SerializeData()
        {
            return Encoding.ASCII.GetBytes(JsonUtility.ToJson(this));
        }

        public virtual void DeserializeData(byte[] data)
        {
            string json = Encoding.ASCII.GetString(data);
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}
