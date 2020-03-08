namespace Ryujinx.Common.Configuration.Hid
{
    public class ControllerConfig : InputConfig
    {
        /// <summary>
        /// Controller Left Analog Stick Deadzone
        /// </summary>
        public float DeadzoneLeft;

        /// <summary>
        /// Controller Right Analog Stick Deadzone
        /// </summary>
        public float DeadzoneRight;

        /// <summary>
        /// Controller Trigger Threshold
        /// </summary>
        public float TriggerThreshold;

        /// <summary>
        /// Left JoyCon Controller Bindings
        /// </summary>
        public NpadControllerLeft LeftJoycon;

        /// <summary>
        /// Right JoyCon Controller Bindings
        /// </summary>
        public NpadControllerRight RightJoycon;
    }
}