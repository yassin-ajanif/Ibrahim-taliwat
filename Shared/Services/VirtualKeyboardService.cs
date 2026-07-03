using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace GestionCommerciale.Shared.Services;

public class VirtualKeyboardService : INotifyPropertyChanged
{
    private WeakReference<InputElement>? _targetRef;
    private AutoCompleteBox? _autoCompleteTarget;
    private NumericUpDown? _numericTarget;
    private string _numBuffer = string.Empty;
    private bool _isNumeric;

    public bool IsOpen { get; private set; }
    public bool IsNumeric => _isNumeric;
    public bool IsAlphaVisible => !_isNumeric;
    public bool IsEnabled { get; set; } = true;

    private InputElement? Target => _targetRef?.TryGetTarget(out var t) == true ? t : null;

    public void Show(InputElement target, bool isNumeric)
    {
        _isNumeric = isNumeric;
        _numBuffer = string.Empty;
        _targetRef = new WeakReference<InputElement>(target);
        _autoCompleteTarget = target as AutoCompleteBox;
        _numericTarget = target as NumericUpDown;
        IsOpen = true;
        OnPropertyChanged(nameof(IsOpen));
        OnPropertyChanged(nameof(IsAlphaVisible));
        OnPropertyChanged(nameof(IsNumeric));
    }

    public void Hide()
    {
        IsOpen = false;
        _targetRef = null;
        _autoCompleteTarget = null;
        _numericTarget = null;
        _numBuffer = string.Empty;
        _isNumeric = false;
        OnPropertyChanged(nameof(IsOpen));
        OnPropertyChanged(nameof(IsAlphaVisible));
        OnPropertyChanged(nameof(IsNumeric));
    }

    public void PressKey(string ch)
    {
        if (_numericTarget != null && _isNumeric)
        {
            if (ch == "." && _numBuffer.Contains("."))
                return;
            if (_numBuffer.Length < 16)
                _numBuffer += ch;
            if (decimal.TryParse(_numBuffer, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                _numericTarget.SetCurrentValue(NumericUpDown.ValueProperty, val);
            return;
        }

        if (_autoCompleteTarget != null)
        {
            var text = _autoCompleteTarget.Text ?? string.Empty;
            _autoCompleteTarget.SetCurrentValue(AutoCompleteBox.TextProperty, text + ch);
            _autoCompleteTarget.IsDropDownOpen = true;
            return;
        }

        if (Target is TextBox tb)
        {
            var text = tb.Text ?? string.Empty;
            tb.SetCurrentValue(TextBox.TextProperty, text + ch);
            return;
        }
    }

    public void Backspace()
    {
        if (_numericTarget != null && _isNumeric)
        {
            if (_numBuffer.Length > 0)
                _numBuffer = _numBuffer[..^1];
            if (_numBuffer.Length == 0)
            {
                _numericTarget.SetCurrentValue(NumericUpDown.ValueProperty, 0m);
                return;
            }
            if (decimal.TryParse(_numBuffer, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                _numericTarget.SetCurrentValue(NumericUpDown.ValueProperty, val);
            return;
        }

        if (_autoCompleteTarget != null)
        {
            var t = _autoCompleteTarget.Text ?? string.Empty;
            if (t.Length > 0)
                _autoCompleteTarget.SetCurrentValue(AutoCompleteBox.TextProperty, t[..^1]);
            return;
        }

        if (Target is TextBox tb)
        {
            var t = tb.Text ?? string.Empty;
            if (t.Length > 0)
                tb.SetCurrentValue(TextBox.TextProperty, t[..^1]);
        }
    }

    public void Clear()
    {
        if (_numericTarget != null && _isNumeric)
        {
            _numBuffer = string.Empty;
            _numericTarget.SetCurrentValue(NumericUpDown.ValueProperty, 0m);
            return;
        }

        if (_autoCompleteTarget != null)
        {
            _autoCompleteTarget.SetCurrentValue(AutoCompleteBox.TextProperty, string.Empty);
            return;
        }

        if (Target is TextBox tb)
            tb.SetCurrentValue(TextBox.TextProperty, string.Empty);
    }

    public void Close() => Hide();

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
