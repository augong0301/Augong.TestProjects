using System.Windows;
using System.Windows.Input;

namespace Augong.WPFTests
{
	public class FocusBehavior
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(FocusBehavior),
                                                                                            new UIPropertyMetadata(false, FocusChangedCallback));

        private static void FocusChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as UIElement;
            if ((bool)e.NewValue)
            {
                sender.Focus();
                Keyboard.Focus(sender);
            }
        }
    }
}
