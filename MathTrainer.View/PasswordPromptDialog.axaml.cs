using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using MathTrainer.Persistence;

namespace MathTrainer.View
{
    public partial class PasswordPromptDialog : Window
    {
        private PasswordService _passwordService;
        public string EnteredPassword { get; private set; } = string.Empty;
        private TextBlock? _errorTextBlock;
        private TextBox? _passwordInputBox;
        private TextBox? _newPasswordTextBox;
        private TextBox? _confirmNewPasswortTextBox;
        private TextBlock? _changePasswordStatusTextBlock;

        public PasswordPromptDialog()
        {
            InitializeComponent();
            _passwordService = new PasswordService();

            _errorTextBlock = this.FindControl<TextBlock>("ErrorTextBlock");
            _passwordInputBox = this.FindControl<TextBox>("PasswordInputBox");
            _newPasswordTextBox = this.FindControl<TextBox>("NewPasswordTextBox");
            _confirmNewPasswortTextBox = this.FindControl<TextBox>("ConfirmNewPasswordTextBox");
            _changePasswordStatusTextBlock = this.FindControl<TextBlock>("ChangePasswordStatusTextBlock");

            if (_passwordInputBox != null)
                {
                    _passwordInputBox.KeyDown += (s, e) =>
                    {
                        if (e.Key == Avalonia.Input.Key.Enter)
                        {
                            OkButton_Click(s, e);
                        }
                    };
                }
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            EnteredPassword = _passwordInputBox?.Text ?? string.Empty;
            this.Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            this.Close(false);
        }

        private void ChangePasswordButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_newPasswordTextBox == null || _confirmNewPasswortTextBox == null || _changePasswordStatusTextBlock == null || _passwordInputBox == null)
            {
                return;
            }

            string newPassword = _newPasswordTextBox.Text ?? "";
            string confirmPassword = _confirmNewPasswortTextBox.Text ?? "";
            string password = _passwordInputBox.Text ?? "";

            _changePasswordStatusTextBlock.Text = "";

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                _changePasswordStatusTextBlock.Text = "Neues Password darf nicht leer sein.";
                _changePasswordStatusTextBlock.Foreground = Brushes.Red;
                return;
            }

            if (newPassword.Length < 4)
            {
                _changePasswordStatusTextBlock.Text = "Das Password muss mindestens 4 Zeichen haben.";
                _changePasswordStatusTextBlock.Foreground = Brushes.Red;
                return;
            }

            if (newPassword != confirmPassword)
            {
                _changePasswordStatusTextBlock.Text = "Die Passwörter stimmen nicht zusammen.";
                _changePasswordStatusTextBlock.Foreground = Brushes.Red;
                return;
            }

            if (!_passwordService.CheckPassword(password))
            {
                _changePasswordStatusTextBlock.Text = "Um Passwort zu ändern, gib ein gültiges Passwort ein.";
                _changePasswordStatusTextBlock.Foreground = Brushes.Red;
                return;
            }

            _passwordService.SetPassword(newPassword);
            _changePasswordStatusTextBlock.Foreground = Brushes.Green;
            _changePasswordStatusTextBlock.Text = "Passwort wurde geändert!";         

            _newPasswordTextBox.Text = "";
            _confirmNewPasswortTextBox.Text = "";
        }

        public void ShowError(string message)
        {
            if (_errorTextBlock != null)
            {
                _errorTextBlock.Text = message;
                _errorTextBlock.IsVisible = true;
            }

            if (_passwordInputBox != null)
            {
                _passwordInputBox.Text = "";
            }
        }
    }
}