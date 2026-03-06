using System;
using System.Windows.Input;

namespace WpfAppLab6Kanban.ViewModels
{
    // ------------------------------------------------------------------
    // RelayCommand — a reusable ICommand implementation.
    //
    // MVVM requires commands so that the ViewModel can expose actions to
    // the View without the View needing any code-behind logic.
    // RelayCommand wraps an Action (execute) and an optional Func<bool>
    // (canExecute) into an ICommand that WPF can bind to.
    // ------------------------------------------------------------------
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        // Raised by WPF's CommandManager whenever it suspects
        // CanExecute may have changed (e.g., after any UI input).
        public event EventHandler? CanExecuteChanged
        {
            add    => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        // Constructor for commands that always can execute
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute    = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Convenience constructor that accepts a parameterless action
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(_ => execute(), canExecute is null ? null : _ => canExecute()) { }

        // WPF calls this to decide whether a bound button should be enabled
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        // WPF calls this when the user triggers the command (e.g., button click)
        public void Execute(object? parameter) => _execute(parameter);

        // Call this manually to force WPF to re-evaluate CanExecute
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}
