using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using WindowSelector.Common;
using WindowSelector.Common.Configuration;
using WindowSelector.Win32;

namespace WindowSelector.Converters
{
    public class HotkeyConverter : IValueConverter
    {
        private ModifierKeysConverter _innerConverter = new ModifierKeysConverter();


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           
            HotkeySetting hotkey = value as HotkeySetting;
            if (hotkey != null)
            {
                string modifierString = _innerConverter.ConvertToString(hotkey.Modifiers);
                string formattedKey = FormatKey(hotkey.Key);
                if (!string.IsNullOrWhiteSpace(modifierString))
                {
                    return $"{modifierString} + {formattedKey}";
                }
                return formattedKey;
            }

            return null;
        }

        private string FormatKey(Key key)
        {
            string keyChar = GetStringFromKey(key, false);
            if(string.IsNullOrWhiteSpace(keyChar))
            {
                return key.ToString();
            }
            if(key.InRange(Key.NumPad0, Key.Divide))
            {
                return "NUM" + keyChar;
            }
            return keyChar.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


        // todo: very bad. Breaking the pattern by using win32 api directly here, but it's difficult to inject 
        // IWin32ApiWrapper into this class. Will have to fix when I move interop to separate assembly.
        // possibly, this whole class will disappear as there is actually little advantage in separating this 
        // out to a converter vs keeping it somewhere else.
        private string GetStringFromKey(Key key, bool shift)
        {
            uint keyCode = (uint)KeyInterop.VirtualKeyFromKey(key);
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
            {
                // Avoiding importing System.Windows.Forms. 
                // 16 = System.Windows.Forms.Keys.Shift
                keyboardState[16] = 0xff;
            }
            Win32Api.ToUnicode(keyCode, 0, keyboardState, buf, 256, 0);
            return buf.ToString();
        }
    }
}
