namespace GreenProject.Controllers
{
    [System.Serializable]
    public class ArmControllerInputData : ControllerInputData
    {
        public float X;
        public float Y;
        public float Z;

        public override string ToString()
        {
            return $"x:{X} y:{Y} z:{Z}";
        }
    }
}
