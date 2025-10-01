using Avalonia.Interactivity;

namespace Avalonia.Controls.DoubleNumericUpDown.EventArgs;

public class DoubleNumericUpDownValueChangedEventArgs : RoutedEventArgs
{
    public DoubleNumericUpDownValueChangedEventArgs(RoutedEvent routedEvent, double? oldValue, double? newValue)
        : base(routedEvent)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    public double? OldValue { get; }
    public double? NewValue { get; }
}