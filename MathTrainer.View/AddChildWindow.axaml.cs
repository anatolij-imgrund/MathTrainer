using Avalonia.Controls;
using Avalonia.Interactivity;
using MathTrainer.Model;
using MathTrainer.Persistence;
using MsBox.Avalonia;

namespace MathTrainer.View;

public partial class AddChildWindow : Window
{
    private ChildPersistenceService _persistenceService;

    public AddChildWindow()
    {
        InitializeComponent();
        _persistenceService = new ChildPersistenceService();
    }

    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        var nameTextBox = this.FindControl<TextBox>("NameTextBox");

        if (nameTextBox != null && !string.IsNullOrWhiteSpace(nameTextBox.Text))
        {
            var newChild = new Child
            {
                Name = nameTextBox.Text.Trim(),
                AvatarImagePath = "avares://MathTrainer.View/Assets/Avatars/default_avatar.png"
            };

            _persistenceService.AddChild(newChild);
            Close();
        }
        else
        {
            var errorBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Falsche Eingabe!", "Der Name kann nicht leer sein", MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            errorBox.ShowWindowDialogAsync(this);
            System.Console.WriteLine("Namensfeld ist leer");
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}