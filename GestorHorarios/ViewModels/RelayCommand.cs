using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GestorHorarios.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);
            _execute = _ => execute();
            if (canExecute != null)
                _canExecute = _ => canExecute();
        }

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object?, Task> _executeAsync;
        private readonly Predicate<object?>? _canExecute;
        private readonly Action<Exception>? _onException;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null, Action<Exception>? onException = null)
        {
            ArgumentNullException.ThrowIfNull(executeAsync);
            _executeAsync = _ => executeAsync();
            if (canExecute != null)
                _canExecute = _ => canExecute();
            _onException = onException;
        }

        public AsyncRelayCommand(Func<object?, Task> executeAsync, Predicate<object?>? canExecute = null, Action<Exception>? onException = null)
        {
            ArgumentNullException.ThrowIfNull(executeAsync);
            _executeAsync = executeAsync;
            _canExecute = canExecute;
            _onException = onException;
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                if (_isExecuting != value)
                {
                    _isExecuting = value;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter)) return;

            try
            {
                IsExecuting = true;
                await _executeAsync(parameter);
            }
            catch (Exception ex)
            {
                if (_onException != null)
                    _onException(ex);
                else
                    System.Diagnostics.Debug.WriteLine($"Error en AsyncRelayCommand: {ex}");
            }
            finally
            {
                IsExecuting = false;
            }
        }

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}

