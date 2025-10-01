using Avalonia.Controls.DoubleNumericUpDown.EventArgs;
using Avalonia.Interactivity;

namespace Avalonia.Controls.DoubleNumericUpDown.Controls;

/// <summary>
/// Custom implementation of NumericUpDown with double values and double.NaN support
/// </summary>
public partial class DoubleNumericUpDown
{
    #region ValueChanged Event

    /// <summary>
    /// Defines the <see cref="ValueChanged"/> event.
    /// </summary>
    public static readonly RoutedEvent<DoubleNumericUpDownValueChangedEventArgs> ValueChangedEvent =
        RoutedEvent.Register<DoubleNumericUpDown, DoubleNumericUpDownValueChangedEventArgs>(
            nameof(ValueChanged),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when the <see cref="Value"/> changes.
    /// </summary>
    public event EventHandler<DoubleNumericUpDownValueChangedEventArgs>? ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    /// <summary>
    /// Raises the <see cref="ValueChanged"/> event.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void RaiseValueChangedEvent(double? oldValue, double? newValue)
    {
        var e = new DoubleNumericUpDownValueChangedEventArgs(ValueChangedEvent, oldValue, newValue);
        RaiseEvent(e);
    }

    #endregion

    #region Spinned Event

    public event EventHandler<SpinEventArgs>? Spinned;

    #endregion
}