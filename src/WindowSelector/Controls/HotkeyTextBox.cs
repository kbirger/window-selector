using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WindowSelector.Common.Configuration;
using WindowSelector.Converters;

namespace WindowSelector.Controls
{
    // Unit tests are not effective at testing this
    [ExcludeFromCodeCoverage]
    public class HotkeyTextBox : TextBox
    {
        public HotkeyTextBox()
        {
            IsReadOnly = true;
        }
        public event EventHandler<HotkeyEventArgs> HotkeySet
        {
            add
            {
                base.AddHandler(HotkeyTextBox.HotkeySetEvent, value);
            }
            remove
            {
                base.RemoveHandler(HotkeyTextBox.HotkeySetEvent, value);
            }
        }

        private HotkeyConverter _converter = new HotkeyConverter();
        
        private static readonly RoutedEvent HotkeySetEvent = EventManager.RegisterRoutedEvent("HotkeySet", RoutingStrategy.Bubble, typeof(EventHandler<HotkeyEventArgs>), typeof(HotkeyTextBox));



        public bool IsListening
        {
            get { return (bool)GetValue(IsListeningProperty); }
            set { SetValue(IsListeningProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsListening.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsListeningProperty =
            DependencyProperty.Register("IsListening", typeof(bool), typeof(HotkeyTextBox), new PropertyMetadata(false));




        public HotkeySetting Hotkey
        {
            get { return (HotkeySetting)GetValue(HotkeyProperty); }
            set { SetValue(HotkeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Hotkey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HotkeyProperty =
            DependencyProperty.Register("Hotkey", typeof(HotkeySetting), typeof(HotkeyTextBox), new PropertyMetadata(null, HotkeyPropertyChanged));

        private static void HotkeyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textbox = (HotkeyTextBox)d;
            textbox.Text = textbox._converter.Convert(e.NewValue, typeof(HotkeySetting), null, System.Globalization.CultureInfo.CurrentCulture) as string; 
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            IsListening = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            IsListening = true;
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            IsListening = false;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (IsListening)
            {
                // event doesn't understand winkey as modifier because reasons. 
                
                // store winkey state because it doesn't get captured by modifierkeys
                var winKey = (e.KeyboardDevice.IsKeyDown(Key.LWin) || e.KeyboardDevice.IsKeyDown(Key.RWin)) ? ModifierKeys.Windows : ModifierKeys.None;

                // some keys get captured in SystemKey, so coalesce it
                var key = (e.Key == Key.System ? e.SystemKey : e.Key);

                var hotkey = new HotkeySetting(key, e.KeyboardDevice.Modifiers | winKey);
                var args = new HotkeyEventArgs(hotkey)
                {
                    RoutedEvent = HotkeyTextBox.HotkeySetEvent,
                    Handled = false
                };
                Hotkey = hotkey;
                //Text = _converter.Convert(hotkey, typeof(HotkeySetting), null, System.Globalization.CultureInfo.CurrentCulture) as string;
                base.RaiseEvent(args);
                
                IsListening = false;
            }
        }
    }

    public class HotkeyEventArgs : RoutedEventArgs
    {
        public HotkeyEventArgs(HotkeySetting hotkey)
        {
            Hotkey = hotkey;
        }

        public HotkeySetting Hotkey { get; private set; }
    }
}
