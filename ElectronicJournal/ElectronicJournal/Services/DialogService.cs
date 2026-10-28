using Avalonia.Controls;
using ElectronicJournal.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicJournal.Services;

public class DialogService : IDialogService
{
    private readonly Window _owner;

    public DialogService(Window owner) => _owner = owner;

    public async Task<bool> ConfirmAsync(string message, string title = "Подтверждение")
    {
        var dialog = new ConfirmDialog(message) { Title = title };
        return await dialog.ShowAsync(_owner);
    }
}
