using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicJournal.Services;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string message, string title = "Подтверждение");
}
