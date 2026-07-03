using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace GestionCommerciale.Shared.Behaviors;

/// <summary>
/// Attaches a command to a <see cref="ListBox"/> and runs it on double-tap (e.g. open selected row).
/// </summary>
public static class ListBoxOpenOnDoubleClick
{
    public static readonly AttachedProperty<ICommand?> OpenCommandProperty =
        AvaloniaProperty.RegisterAttached<ListBox, ICommand?>(
            "OpenCommand",
            typeof(ListBoxOpenOnDoubleClick));

    public static void SetOpenCommand(ListBox listBox, ICommand? value) =>
        listBox.SetValue(OpenCommandProperty, value);

    public static ICommand? GetOpenCommand(ListBox listBox) =>
        listBox.GetValue(OpenCommandProperty);

    static ListBoxOpenOnDoubleClick()
    {
        OpenCommandProperty.Changed.AddClassHandler<ListBox>(OnOpenCommandChanged);
    }

    private static void OnOpenCommandChanged(ListBox listBox, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is not null)
            listBox.DoubleTapped -= OnDoubleTapped;
        if (e.NewValue is not null)
            listBox.DoubleTapped += OnDoubleTapped;
    }

    private static void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not ListBox listBox) return;
        var cmd = listBox.GetValue(OpenCommandProperty);
        if (cmd is null || !cmd.CanExecute(null)) return;
        cmd.Execute(null);
        e.Handled = true;
    }
}
