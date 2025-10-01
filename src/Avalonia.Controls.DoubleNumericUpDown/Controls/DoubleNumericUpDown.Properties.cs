using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Utilities;

namespace Avalonia.Controls.DoubleNumericUpDown.Controls;

/// <summary>
/// Custom implementation of NumericUpDown with double values and double.NaN support
/// </summary>
public partial class DoubleNumericUpDown
{
    #region AllowSpin Property

    /// <summary>
    /// Defines the <see cref="AllowSpin"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> AllowSpinProperty =
        ButtonSpinner.AllowSpinProperty.AddOwner<DoubleNumericUpDown>();

    /// <summary>
    /// Gets or sets the ability to perform increment/decrement operations via the keyboard, button spinners, or mouse wheel.
    /// </summary>
    public bool AllowSpin
    {
        get => GetValue(AllowSpinProperty);
        set => SetValue(AllowSpinProperty, value);
    }

    #endregion

    #region ButtonSpinnerLocation Property

    /// <summary>
    /// Defines the <see cref="ButtonSpinnerLocation"/> property.
    /// </summary>
    public static readonly StyledProperty<Location> ButtonSpinnerLocationProperty =
        ButtonSpinner.ButtonSpinnerLocationProperty.AddOwner<DoubleNumericUpDown>();

    /// <summary>
    /// Gets or sets current location of the <see cref="ButtonSpinner"/>.
    /// </summary>
    public Location ButtonSpinnerLocation
    {
        get => GetValue(ButtonSpinnerLocationProperty);
        set => SetValue(ButtonSpinnerLocationProperty, value);
    }

    #endregion

    #region ShowButtonSpinner Property

    /// <summary>
    /// Defines the <see cref="ShowButtonSpinner"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowButtonSpinnerProperty =
        ButtonSpinner.ShowButtonSpinnerProperty.AddOwner<DoubleNumericUpDown>();

    /// <summary>
    /// Gets or sets a value indicating whether the spin buttons should be shown.
    /// </summary>
    public bool ShowButtonSpinner
    {
        get => GetValue(ShowButtonSpinnerProperty);
        set => SetValue(ShowButtonSpinnerProperty, value);
    }

    #endregion

    #region ClipValueToMinMax Property

    /// <summary>
    /// Defines the <see cref="ClipValueToMinMax"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ClipValueToMinMaxProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, bool>(nameof(ClipValueToMinMax));

    /// <summary>
    /// Gets or sets if the value should be clipped when minimum/maximum is reached.
    /// </summary>
    public bool ClipValueToMinMax
    {
        get => GetValue(ClipValueToMinMaxProperty);
        set => SetValue(ClipValueToMinMaxProperty, value);
    }

    #endregion

    #region NumberFormat Property

    /// <summary>
    /// Defines the <see cref="NumberFormat"/> property.
    /// </summary>
    public static readonly StyledProperty<NumberFormatInfo?> NumberFormatProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, NumberFormatInfo?>(nameof(NumberFormat),
            NumberFormatInfo.CurrentInfo);

    /// <summary>
    /// Gets or sets the current NumberFormatInfo
    /// </summary>
    public NumberFormatInfo? NumberFormat
    {
        get => GetValue(NumberFormatProperty);
        set => SetValue(NumberFormatProperty, value);
    }

    /// <summary>
    /// Called when the <see cref="NumberFormat"/> property value changed.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void OnNumberFormatChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is DoubleNumericUpDown upDown)
        {
            var oldValue = (NumberFormatInfo?)e.OldValue;
            var newValue = (NumberFormatInfo?)e.NewValue;
            upDown.OnNumberFormatChanged(oldValue, newValue);
        }
    }

    /// <summary>
    /// Called when the <see cref="NumberFormat"/> property value changed.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnNumberFormatChanged(NumberFormatInfo? oldValue, NumberFormatInfo? newValue)
    {
        if (IsInitialized)
        {
            SyncTextAndValueProperties(false, null);
        }
    }

    #endregion

    #region FormatString Property

    /// <summary>
    /// Defines the <see cref="FormatString"/> property.
    /// </summary>
    public static readonly StyledProperty<string> FormatStringProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, string>(nameof(FormatString), string.Empty);

    /// <summary>
    /// Gets or sets the display format of the <see cref="Value"/>.
    /// </summary>
    public string FormatString
    {
        get => GetValue(FormatStringProperty);
        set => SetValue(FormatStringProperty, value);
    }

    /// <summary>
    /// Called when the <see cref="FormatString"/> property value changed.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void FormatStringChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is DoubleNumericUpDown upDown)
        {
            var oldValue = (string?)e.OldValue;
            var newValue = (string?)e.NewValue;
            upDown.OnFormatStringChanged(oldValue, newValue);
        }
    }

    /// <summary>
    /// Called when the <see cref="FormatString"/> property value changed.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnFormatStringChanged(string? oldValue, string? newValue)
    {
        if (IsInitialized)
        {
            SyncTextAndValueProperties(false, null, true);
        }
    }

    #endregion

    #region Increment Property

    /// <summary>
    /// Defines the <see cref="Increment"/> property.
    /// </summary>
    public static readonly StyledProperty<double> IncrementProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, double>(nameof(Increment), 1.0d, coerce: OnCoerceIncrement);

    /// <summary>
    /// Gets or sets the amount in which to increment the <see cref="Value"/>.
    /// </summary>
    public double Increment
    {
        get => GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    private static double OnCoerceIncrement(AvaloniaObject instance, double value)
    {
        if (instance is DoubleNumericUpDown upDown)
        {
            return upDown.OnCoerceIncrement(value);
        }

        return value;
    }

    /// <summary>
    /// Called when the <see cref="Increment"/> property has to be coerced.
    /// </summary>
    /// <param name="baseValue">The value.</param>
    protected virtual double OnCoerceIncrement(double baseValue)
    {
        return baseValue;
    }

    /// <summary>
    /// Called when the <see cref="Increment"/> property value changed.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void IncrementChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is DoubleNumericUpDown upDown)
        {
            var oldValue = (double)e.OldValue!;
            var newValue = (double)e.NewValue!;
            upDown.OnIncrementChanged(oldValue, newValue);
        }
    }

    /// <summary>
    /// Called when the <see cref="Increment"/> property value changed.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnIncrementChanged(double oldValue, double newValue)
    {
        if (IsInitialized)
        {
            SetValidSpinDirection();
        }
    }

    #endregion

    #region IsReadOnly Property

    /// <summary>
    /// Defines the <see cref="IsReadOnly"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, bool>(nameof(IsReadOnly));

    /// <summary>
    /// Gets or sets if the control is read only.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Called when the <see cref="IsReadOnly"/> property value changed.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void OnIsReadOnlyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is DoubleNumericUpDown upDown)
        {
            var oldValue = (bool)e.OldValue!;
            var newValue = (bool)e.NewValue!;
            upDown.OnIsReadOnlyChanged(oldValue, newValue);
        }
    }

    /// <summary>
    /// Called when the <see cref="IsReadOnly"/> property value changed.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnIsReadOnlyChanged(bool oldValue, bool newValue)
    {
        SetValidSpinDirection();
    }

    #endregion

    #region Maximum Property

    /// <summary>
    /// Defines the <see cref="Maximum"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, double>(nameof(Maximum), double.MaxValue,
            coerce: OnCoerceMaximum);

    /// <summary>
    /// Gets or sets the maximum allowed value.
    /// </summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private static double OnCoerceMaximum(AvaloniaObject instance, double value)
    {
        if (instance is DoubleNumericUpDown upDown)
        {
            return upDown.OnCoerceMaximum(value);
        }

        return value;
    }

    /// <summary>
    /// Called when the <see cref="Maximum"/> property has to be coerced.
    /// </summary>
    /// <param name="baseValue">The value.</param>
    protected virtual double OnCoerceMaximum(double baseValue)
    {
        return Math.Max(baseValue, Minimum);
    }

    /// <summary>
    /// Called when the <see cref="Maximum"/> property value changed.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void OnMaximumChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is DoubleNumericUpDown upDown)
        {
            var oldValue = (double)e.OldValue!;
            var newValue = (double)e.NewValue!;
            upDown.OnMaximumChanged(oldValue, newValue);
        }
    }

    /// <summary>
    /// Called when the <see cref="Maximum"/> property value changed.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnMaximumChanged(double oldValue, double newValue)
    {
        if (IsInitialized)
        {
            SetValidSpinDirection();
        }

        if (ClipValueToMinMax && Value.HasValue)
        {
            SetCurrentValue(ValueProperty, MathUtilities.Clamp(Value.Value, Minimum, Maximum));
        }
    }

    #endregion

    #region Minimum Property

    /// <summary>
    /// Defines the <see cref="Minimum"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, double>(nameof(Minimum), double.MinValue,
            coerce: OnCoerceMinimum);

    /// <summary>
    /// Gets or sets the minimum allowed value.
    /// </summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    private static double OnCoerceMinimum(AvaloniaObject instance, double value)
    {
        if (instance is DoubleNumericUpDown upDown)
        {
            return upDown.OnCoerceMinimum(value);
        }

        return value;
    }

    /// <summary>
    /// Called when the <see cref="Minimum"/> property has to be coerced.
    /// </summary>
    /// <param name="baseValue">The value.</param>
    protected virtual double OnCoerceMinimum(double baseValue)
    {
        return Math.Min(baseValue, Maximum);
    }

    /// <summary>
    /// Called when the <see cref="Minimum"/> property value changed.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void OnMinimumChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is DoubleNumericUpDown upDown)
        {
            var oldValue = (double)e.OldValue!;
            var newValue = (double)e.NewValue!;
            upDown.OnMinimumChanged(oldValue, newValue);
        }
    }

    /// <summary>
    /// Called when the <see cref="Minimum"/> property value changed.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnMinimumChanged(double oldValue, double newValue)
    {
        if (IsInitialized)
        {
            SetValidSpinDirection();
        }

        if (ClipValueToMinMax && Value.HasValue)
        {
            SetCurrentValue(ValueProperty, MathUtilities.Clamp(Value.Value, Minimum, Maximum));
        }
    }

    #endregion

    #region ParsingNumberStyle Property

    /// <summary>
    /// Defines the <see cref="ParsingNumberStyle"/> property.
    /// </summary>
    public static readonly StyledProperty<NumberStyles> ParsingNumberStyleProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, NumberStyles>(nameof(ParsingNumberStyle), NumberStyles.Any);

    /// <summary>
    /// Gets or sets the parsing style (AllowLeadingWhite, Float, AllowHexSpecifier, ...). By default, Any.
    /// Note that Hex style does not work with double. 
    /// For hexadecimal display, use <see cref="TextConverter"/>.
    /// </summary>
    public NumberStyles ParsingNumberStyle
    {
        get => GetValue(ParsingNumberStyleProperty);
        set => SetValue(ParsingNumberStyleProperty, value);
    }

    #endregion

    #region Text Property

    /// <summary>
    /// Defines the <see cref="Text"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, string?>(nameof(Text),
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    /// <summary>
    /// Gets or sets the formatted string representation of the value.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Called when the <see cref="Text"/> property value changed.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void OnTextChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is DoubleNumericUpDown upDown)
        {
            var oldValue = (string?)e.OldValue;
            var newValue = (string?)e.NewValue;
            upDown.OnTextChanged(oldValue, newValue);
        }
    }

    /// <summary>
    /// Called when the <see cref="Text"/> property value changed.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnTextChanged(string? oldValue, string? newValue)
    {
        if (IsInitialized)
        {
            SyncTextAndValueProperties(true, Text);
        }
    }

    #endregion

    #region TextConverter Property

    /// <summary>
    /// Defines the <see cref="TextConverter"/> property.
    /// </summary>
    public static readonly StyledProperty<IValueConverter?> TextConverterProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, IValueConverter?>(nameof(TextConverter),
            defaultBindingMode: BindingMode.OneWay);

    /// <summary>
    /// Gets or sets the custom bidirectional Text-Value converter.
    /// Non-null converter overrides <see cref="ParsingNumberStyle"/>, providing finer control over 
    /// string representation of the underlying value.
    /// </summary>
    public IValueConverter? TextConverter
    {
        get => GetValue(TextConverterProperty);
        set => SetValue(TextConverterProperty, value);
    }

    /// <summary>
    /// Called when the <see cref="TextConverter"/> property value changed.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void OnTextConverterChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is DoubleNumericUpDown upDown)
        {
            var oldValue = (IValueConverter?)e.OldValue;
            var newValue = (IValueConverter?)e.NewValue;
            upDown.OnTextConverterChanged(oldValue, newValue);
        }
    }

    /// <summary>
    /// Called when the <see cref="Text"/> property value changed.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnTextConverterChanged(IValueConverter? oldValue, IValueConverter? newValue)
    {
        if (IsInitialized)
        {
            SyncTextAndValueProperties(false, null);
        }
    }

    #endregion

    #region Value Property

    /// <summary>
    /// Defines the <see cref="Value"/> property.
    /// </summary>
    public static readonly StyledProperty<double?> ValueProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, double?>(nameof(Value),
            coerce: (s, v) => ((DoubleNumericUpDown)s).OnCoerceValue(v),
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public double? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Called when the <see cref="Value"/> property has to be coerced.
    /// </summary>
    /// <param name="baseValue">The value.</param>
    protected virtual double? OnCoerceValue(double? baseValue)
    {
        return baseValue;
    }

    /// <summary>
    /// Called when the <see cref="Value"/> property value changed.
    /// </summary>
    /// <param name="e">The event args.</param>
    private static void OnValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is DoubleNumericUpDown upDown)
        {
            var oldValue = (double?)e.OldValue;
            var newValue = (double?)e.NewValue;
            upDown.OnValueChanged(oldValue, newValue);
        }
    }

    /// <summary>
    /// Called when the <see cref="Value"/> property value changed.
    /// </summary>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected virtual void OnValueChanged(double? oldValue, double? newValue)
    {
        if (!_internalValueSet && IsInitialized)
        {
            SyncTextAndValueProperties(false, null, true);
        }

        SetValidSpinDirection();

        RaiseValueChangedEvent(oldValue, newValue);
    }

    #endregion

    #region Watermark Property

    /// <summary>
    /// Defines the <see cref="Watermark"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> WatermarkProperty =
        AvaloniaProperty.Register<DoubleNumericUpDown, string?>(nameof(Watermark));

    /// <summary>
    /// Gets or sets the object to use as a watermark if the <see cref="Value"/> is null.
    /// </summary>
    public string? Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    #endregion

    #region HorizontalContentAlignment Property

    /// <summary>
    /// Defines the <see cref="HorizontalContentAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        ContentControl.HorizontalContentAlignmentProperty.AddOwner<DoubleNumericUpDown>();

    /// <summary>
    /// Gets or sets the horizontal alignment of the content within the control.
    /// </summary>
    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    #endregion

    #region VerticalContentAlignment Property

    /// <summary>
    /// Defines the <see cref="VerticalContentAlignment"/> property.
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        ContentControl.VerticalContentAlignmentProperty.AddOwner<DoubleNumericUpDown>();

    /// <summary>
    /// Gets or sets the vertical alignment of the content within the control.
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    #endregion

    #region TextAlignment Property

    /// <summary>
    /// Defines the <see cref="TextAlignment"/> property
    /// </summary>
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        TextBox.TextAlignmentProperty.AddOwner<DoubleNumericUpDown>();

    /// <summary>
    /// Gets or sets the <see cref="Avalonia.Media.TextAlignment"/> of the <see cref="DoubleNumericUpDown"/>
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    #endregion

    #region InnerLeftContent Property

    /// <summary>
    /// Defines the <see cref="InnerLeftContent"/> property
    /// </summary>
    public static readonly StyledProperty<object?> InnerLeftContentProperty =
        TextBox.InnerLeftContentProperty.AddOwner<DoubleNumericUpDown>();

    /// <summary>
    /// Gets or sets custom content that is positioned on the left side of the text layout box
    /// </summary>
    public object? InnerLeftContent
    {
        get => GetValue(InnerLeftContentProperty);
        set => SetValue(InnerLeftContentProperty, value);
    }

    #endregion

    #region InnerRightContent Property

    /// <summary>
    /// Defines the <see cref="InnerRightContent"/> property
    /// </summary>
    public static readonly StyledProperty<object?> InnerRightContentProperty =
        TextBox.InnerRightContentProperty.AddOwner<DoubleNumericUpDown>();

    /// <summary>
    /// Gets or sets custom content that is positioned on the right side of the text layout box
    /// </summary>
    public object? InnerRightContent
    {
        get => GetValue(InnerRightContentProperty);
        set => SetValue(InnerRightContentProperty, value);
    }

    #endregion
}