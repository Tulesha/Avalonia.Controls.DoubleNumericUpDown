using System.Globalization;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Utilities;

namespace Avalonia.Controls.DoubleNumericUpDown.Controls;

/// <summary>
/// Custom implementation of NumericUpDown with double values and double.NaN support
/// </summary>
public partial class DoubleNumericUpDown : TemplatedControl
{
    private const string NanString = nameof(double.NaN);

    private IDisposable? _textBoxTextChangedSubscription;
    private bool _internalValueSet;
    private bool _isSyncingTextAndValueProperties;
    private bool _isTextChangedFromUi;
    private bool _isFocused;

    /// <summary>
    /// Gets the Spinner template part.
    /// </summary>
    private Spinner? Spinner { get; set; }

    /// <summary>
    /// Gets the TextBox template part.
    /// </summary>
    private TextBox? TextBox { get; set; }

    /// <summary>
    /// Initializes static members of the <see cref="DoubleNumericUpDown"/> class.
    /// </summary>
    static DoubleNumericUpDown()
    {
        NumberFormatProperty.Changed.Subscribe(OnNumberFormatChanged);
        FormatStringProperty.Changed.Subscribe(FormatStringChanged);
        IncrementProperty.Changed.Subscribe(IncrementChanged);
        IsReadOnlyProperty.Changed.Subscribe(OnIsReadOnlyChanged);
        MaximumProperty.Changed.Subscribe(OnMaximumChanged);
        MinimumProperty.Changed.Subscribe(OnMinimumChanged);
        TextProperty.Changed.Subscribe(OnTextChanged);
        TextConverterProperty.Changed.Subscribe(OnTextConverterChanged);
        ValueProperty.Changed.Subscribe(OnValueChanged);

        FocusableProperty.OverrideDefaultValue<DoubleNumericUpDown>(true);
        IsTabStopProperty.OverrideDefaultValue<DoubleNumericUpDown>(false);
    }

    /// <summary>
    /// Initializes new instance of <see cref="DoubleNumericUpDown"/> class.
    /// </summary>
    public DoubleNumericUpDown()
    {
        Initialized += (_, _) =>
        {
            if (!_internalValueSet && IsInitialized)
            {
                SyncTextAndValueProperties(false, null, true);
            }

            SetValidSpinDirection();
        };
    }

    /// <inheritdoc />
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        FocusChanged(IsKeyboardFocusWithin);
    }

    /// <inheritdoc />
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        CommitInput(true);
        base.OnLostFocus(e);
        FocusChanged(IsKeyboardFocusWithin);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (TextBox != null)
        {
            TextBox.PointerPressed -= TextBoxOnPointerPressed;
            _textBoxTextChangedSubscription?.Dispose();
        }

        TextBox = e.NameScope.Find<TextBox>("PART_TextBox");
        if (TextBox != null)
        {
            TextBox.Text = Text;
            TextBox.PointerPressed += TextBoxOnPointerPressed;
            _textBoxTextChangedSubscription =
                TextBox.GetObservable(TextBox.TextProperty).Subscribe(_ => TextBoxOnTextChanged());
        }

        if (Spinner != null)
        {
            Spinner.Spin -= OnSpinnerSpin;
        }

        Spinner = e.NameScope.Find<Spinner>("PART_Spinner");

        if (Spinner != null)
        {
            Spinner.Spin += OnSpinnerSpin;
        }

        SetValidSpinDirection();
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                var commitSuccess = CommitInput();
                e.Handled = !commitSuccess;
                break;
        }
    }

    /// <summary>
    /// Called to update the validation state for properties for which data validation is
    /// enabled.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="state">The current data binding state.</param>
    /// <param name="error">The current data binding error, if any.</param>
    protected override void UpdateDataValidation(
        AvaloniaProperty property,
        BindingValueType state,
        Exception? error)
    {
        if (property == TextProperty || property == ValueProperty)
        {
            DataValidationErrors.SetError(this, error);
        }
    }

    /// <summary>
    /// Raises the OnSpin event when spinning is initiated by the end-user.
    /// </summary>
    /// <param name="e">The event args.</param>
    protected virtual void OnSpin(SpinEventArgs e)
    {
        if (e == null)
        {
            throw new ArgumentNullException(nameof(e));
        }

        if (e.Direction == SpinDirection.Increase)
        {
            DoIncrement();
        }
        else
        {
            DoDecrement();
        }

        Spinned?.Invoke(this, e);
    }

    /// <summary>
    /// Converts the formatted text to a value.
    /// </summary>
    private double? ConvertTextToValue(string? text)
    {
        double? result = null;

        if (string.IsNullOrEmpty(text))
        {
            return result;
        }

        // Since the conversion from Value to text using a FormatString may not be parsable,
        // we verify that the already existing text is not the exact same value.
        var currentValueText = ConvertValueToText();
        if (Equals(currentValueText, text))
        {
            return Value;
        }

        result = ConvertTextToValueCore(currentValueText, text);

        if (ClipValueToMinMax && result.HasValue)
        {
            return MathUtilities.Clamp(result.Value, Minimum, Maximum);
        }

        ValidateMinMax(result);

        return result;
    }

    /// <summary>
    /// Converts the value to formatted text.
    /// </summary>
    /// <returns></returns>
    private string? ConvertValueToText()
    {
        if (TextConverter != null)
        {
            return TextConverter.ConvertBack(Value, typeof(string), null, CultureInfo.CurrentCulture)?.ToString();
        }

        //Manage FormatString of type "{}{0:N2} °" (in xaml) or "{0:N2} °" in code-behind.
        if (FormatString.Contains("{0"))
        {
            return string.Format(NumberFormat, FormatString, Value);
        }

        if (Value != null &&
            double.IsNaN(Value.Value))
        {
            return NanString;
        }

        return Value?.ToString(FormatString, NumberFormat);
    }

    /// <summary>
    /// Called by OnSpin when the spin direction is SpinDirection.Increase.
    /// </summary>
    private void OnIncrement()
    {
        double result;
        if (Value.HasValue)
        {
            if (double.IsNaN(Value.Value))
            {
                // if Minimum is set we set value to Minimum on Increment. 
                // otherwise we set value to 0. It ill be clamped to be between Minimum and Maximum later, so we don't need to do it here. 
                result = IsSet(MinimumProperty) ? Minimum : 0;
            }
            else
            {
                result = Value.Value + Increment;
            }
        }
        else
        {
            // if Minimum is set we set value to Minimum on Increment. 
            // otherwise we set value to 0. It ill be clamped to be between Minimum and Maximum later, so we don't need to do it here. 
            result = IsSet(MinimumProperty) ? Minimum : 0;
        }

        SetCurrentValue(ValueProperty, MathUtilities.Clamp(result, Minimum, Maximum));
    }

    /// <summary>
    /// Called by OnSpin when the spin direction is SpinDirection.Decrease.
    /// </summary>
    private void OnDecrement()
    {
        double result;

        if (Value.HasValue)
        {
            if (double.IsNaN(Value.Value))
            {
                // if Maximum is set we set value to Maximum on decrement. 
                // otherwise we set value to 0. It ill be clamped to be between Minimum and Maximum later, so we don't need to do it here. 
                result = IsSet(MaximumProperty) ? Maximum : 0;
            }
            else
            {
                result = Value.Value - Increment;
            }
        }
        else
        {
            // if Maximum is set we set value to Maximum on decrement. 
            // otherwise we set value to 0. It ill be clamped to be between Minimum and Maximum later, so we don't need to do it here. 
            result = IsSet(MaximumProperty) ? Maximum : 0;
        }

        SetCurrentValue(ValueProperty, MathUtilities.Clamp(result, Minimum, Maximum));
    }

    /// <summary>
    /// Sets the valid spin directions.
    /// </summary>
    private void SetValidSpinDirection()
    {
        var validDirections = ValidSpinDirections.None;

        // Zero increment always prevents spin.
        if (Increment != 0 && !IsReadOnly)
        {
            if (!Value.HasValue)
            {
                validDirections = ValidSpinDirections.Increase | ValidSpinDirections.Decrease;
            }

            if (Value.HasValue && double.IsNaN(Value.Value))
            {
                validDirections = ValidSpinDirections.Increase | ValidSpinDirections.Decrease;
            }

            if (Value < Maximum)
            {
                validDirections = validDirections | ValidSpinDirections.Increase;
            }

            if (Value > Minimum)
            {
                validDirections = validDirections | ValidSpinDirections.Decrease;
            }
        }

        if (Spinner != null)
        {
            Spinner.ValidSpinDirection = validDirections;
        }
    }

    private void SetValueInternal(double? value)
    {
        _internalValueSet = true;
        try
        {
            SetCurrentValue(ValueProperty, value);
        }
        finally
        {
            _internalValueSet = false;
        }
    }

    private void TextBoxOnTextChanged()
    {
        try
        {
            _isTextChangedFromUi = true;
            if (TextBox != null)
            {
                SetCurrentValue(TextProperty, TextBox.Text);
            }
        }
        finally
        {
            _isTextChangedFromUi = false;
        }
    }

    private void OnSpinnerSpin(object? sender, SpinEventArgs e)
    {
        if (AllowSpin && !IsReadOnly)
        {
            var spin = !e.UsingMouseWheel;
            spin |= ((TextBox != null) && TextBox.IsFocused);

            if (spin)
            {
                e.Handled = true;
                OnSpin(e);
            }
        }
    }

    private void DoDecrement()
    {
        if (Spinner == null ||
            (Spinner.ValidSpinDirection & ValidSpinDirections.Decrease) == ValidSpinDirections.Decrease)
        {
            OnDecrement();
        }
    }

    private void DoIncrement()
    {
        if (Spinner == null ||
            (Spinner.ValidSpinDirection & ValidSpinDirections.Increase) == ValidSpinDirections.Increase)
        {
            OnIncrement();
        }
    }

    private void TextBoxOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Captured != Spinner)
        {
            Dispatcher.UIThread.InvokeAsync(() => { e.Pointer.Capture(Spinner); }, DispatcherPriority.Input);
        }
    }

    private bool CommitInput(bool forceTextUpdate = false)
    {
        return SyncTextAndValueProperties(true, Text, forceTextUpdate);
    }

    /// <summary>
    /// Synchronize <see cref="Text"/> and <see cref="Value"/> properties.
    /// </summary>
    /// <param name="updateValueFromText">If value should be updated from text.</param>
    /// <param name="text">The text.</param>
    private bool SyncTextAndValueProperties(bool updateValueFromText, string? text)
    {
        return SyncTextAndValueProperties(updateValueFromText, text, false);
    }

    /// <summary>
    /// Synchronize <see cref="Text"/> and <see cref="Value"/> properties.
    /// </summary>
    /// <param name="updateValueFromText">If value should be updated from text.</param>
    /// <param name="text">The text.</param>
    /// <param name="forceTextUpdate">Force text update.</param>
    private bool SyncTextAndValueProperties(bool updateValueFromText, string? text, bool forceTextUpdate)
    {
        if (_isSyncingTextAndValueProperties)
            return true;

        _isSyncingTextAndValueProperties = true;
        var parsedTextIsValid = true;
        try
        {
            if (updateValueFromText)
            {
                try
                {
                    var newValue = ConvertTextToValue(text);
                    if (!Equals(newValue, Value))
                    {
                        SetValueInternal(newValue);
                    }
                }
                catch
                {
                    parsedTextIsValid = false;
                }
            }

            // Do not touch the ongoing text input from user.
            if (!_isTextChangedFromUi)
            {
                if (forceTextUpdate)
                {
                    var newText = ConvertValueToText();
                    if (!Equals(Text, newText))
                    {
                        SetCurrentValue(TextProperty, newText);
                    }
                }

                // Sync Text and textBox
                if (TextBox != null)
                {
                    TextBox.Text = Text;
                }
            }

            if (_isTextChangedFromUi && !parsedTextIsValid)
            {
                // Text input was made from the user and the text
                // represents an invalid value. Disable the spinner in this case.
                if (Spinner != null)
                {
                    Spinner.ValidSpinDirection = ValidSpinDirections.None;
                }
            }
            else
            {
                SetValidSpinDirection();
            }
        }
        finally
        {
            _isSyncingTextAndValueProperties = false;
        }

        return parsedTextIsValid;
    }

    private double? ConvertTextToValueCore(string? currentValueText, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        if (TextConverter != null)
        {
            var valueFromText = TextConverter.Convert(text, typeof(double?), null, CultureInfo.CurrentCulture);
            return (double?)valueFromText;
        }

        double? result;
        if (IsNaN(text))
        {
            result = double.NaN;
        }
        else if (IsPercent(FormatString))
        {
            result = ParsePercent(text, NumberFormat);
        }
        else
        {
            // Problem while converting new text
            if (!double.TryParse(text, ParsingNumberStyle, NumberFormat, out var outputValue))
            {
                var shouldThrow = true;

                // Check if CurrentValueText is also failing => it also contains special characters. ex : 90°
                if (!string.IsNullOrEmpty(currentValueText) &&
                    !double.TryParse(currentValueText, ParsingNumberStyle, NumberFormat, out var _))
                {
                    // extract non-digit characters
                    var currentValueTextSpecialCharacters = currentValueText.Where(c => !char.IsDigit(c));
                    var textSpecialCharacters = text.Where(c => !char.IsDigit(c)).ToArray();
                    // same non-digit characters on currentValueText and new text => remove them on new Text to parse it again.
                    if (!currentValueTextSpecialCharacters.Except(textSpecialCharacters).Any())
                    {
                        foreach (var character in textSpecialCharacters)
                        {
                            text = text.Replace(character.ToString(), string.Empty);
                        }

                        // if without the special characters, parsing is good, do not throw
                        if (double.TryParse(text, ParsingNumberStyle, NumberFormat, out outputValue))
                        {
                            shouldThrow = false;
                        }
                    }
                }

                if (shouldThrow)
                {
                    throw new InvalidDataException("Input string was not in a correct format.");
                }
            }

            result = outputValue;
        }

        return result;
    }

    private void ValidateMinMax(double? value)
    {
        if (!value.HasValue)
        {
            return;
        }

        if (value < Minimum)
        {
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Value must be greater than Minimum value of {Minimum}");
        }
        else if (value > Maximum)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Value must be less than Maximum value of {Maximum}");
        }
    }

    /// <summary>
    /// Parse percent format text
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <param name="cultureInfo">The culture info.</param>
    private static double ParsePercent(string text, IFormatProvider? cultureInfo)
    {
        var info = NumberFormatInfo.GetInstance(cultureInfo);
        text = text.Replace(info.PercentSymbol, null);
        var result = double.Parse(text, NumberStyles.Any, info);
        result = result / 100;
        return result;
    }

    private bool IsPercent(string stringToTest)
    {
        var pIndex = stringToTest.IndexOf("P", StringComparison.Ordinal);
        if (pIndex >= 0)
        {
            //stringToTest contains a "P" between 2 "'", it's considered as text, not percent
            var isText = stringToTest.Substring(0, pIndex).Contains('\'')
                         && stringToTest.Substring(pIndex, FormatString.Length - pIndex).Contains('\'');

            return !isText;
        }

        return false;
    }

    private static bool IsNaN(string text)
    {
        return string.Equals(text.Trim(), NanString, StringComparison.OrdinalIgnoreCase);
    }

    private void FocusChanged(bool hasFocus)
    {
        // The OnGotFocus & OnLostFocus are asynchronously and cannot
        // reliably tell you that have the focus.  All they do is let you
        // know that the focus changed sometime in the past.  To determine
        // if you currently have the focus you need to do consult the
        // FocusManager.

        bool wasFocused = _isFocused;
        _isFocused = hasFocus;

        if (hasFocus)
        {
            if (!wasFocused && TextBox != null)
            {
                TextBox.Focus();
                TextBox.SelectAll();
            }
        }
    }
}