namespace GreenProject.Controllers
{
    [System.Serializable]
    public class RailInputData : ControllerInputData
    {
        public enum Action
        {
            First,
            Next
        }

        public Action ToExecute;
    }
}
