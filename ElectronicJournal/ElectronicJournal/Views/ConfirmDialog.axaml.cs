
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;

namespace ElectronicJournal.Views;

public partial class ConfirmDialog : Window
{
    private readonly TaskCompletionSource<bool> _tcs = new();

    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public ConfirmDialog(string message) : this()
    {
        MessageText.Text = message;
    }

    public Task<bool> ShowAsync(Window owner)
    {
        _ = ShowDialog(owner);
        return _tcs.Task;
    }

    private void OnConfirm(object? sender, RoutedEventArgs e)
    {
        _tcs.TrySetResult(true);
        Close();
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        _tcs.TrySetResult(false);
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _tcs.TrySetResult(false);
        base.OnClosed(e);
    }
}