namespace Ryujinx.Common.Configuration.Hid
{
    public class KeyboardConfig : InputConfig
    {
        /// <summary>
        /// Left JoyCon Keyboard Bindings
        /// </summary>
        public NpadKeyboardLeft LeftJoycon;

        /// <summary>
        /// Right JoyCon Keyboard Bindings
        /// </summary>
        public NpadKeyboardRight RightJoycon;

        /// <summary>
        /// Hotkey Keyboard Bindings
        /// </summary>
        public KeyboardHotkeys Hotkeys;
    }
}