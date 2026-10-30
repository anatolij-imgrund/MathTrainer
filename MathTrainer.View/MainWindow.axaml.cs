using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MathTrainer.Model;
using MathTrainer.Persistence;
using MathTrainer.View;
using System.Collections.Generic;
using System.Linq;
using MsBox.Avalonia;
using System.Threading.Tasks;

namespace MathTrainer.View;

public partial class MainWindow : Window
{
    private ChildPersistenceService _persistenceService;
    private List<Child> _childrenList;
    private Child? _selectedChild;
    private ListBox? _childrenListBox;

    public MainWindow()
    {
        InitializeComponent();
        _persistenceService = new ChildPersistenceService();
        _childrenList = new List<Child>();
        _childrenListBox = this.FindControl<ListBox>("ChildrenListBox");
        LoadAndDisplayChildren();
    }

    private void AddChildButton_Click(object? sender, RoutedEventArgs e)
    {
        OpenEditChildWindow(null);
    }

    private async void AddRewardButton_Click(object? sender, RoutedEventArgs e)
    {
        var passwordService = new PasswordService();
        bool accessGranted = false;

        while (!accessGranted)
        {
            var promptDialog = new PasswordPromptDialog();
            var dialogResult = await promptDialog.ShowDialog<bool?>(this);

            if (dialogResult == true)
            {
                if (passwordService.CheckPassword(promptDialog.EnteredPassword))
                {
                    accessGranted = true;
                    OpenEditRewardWindow(null);
                }
                else
                {
                    var errorBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Fehler", "Das Passwort ist falsh. Probiere noch Mal.");
                    await errorBox.ShowWindowDialogAsync(this);
                }
            }
            else
            {
                break;
            }
        }        
    }

    private void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Child childToEdit)
        {
            OpenEditChildWindow(childToEdit);
        }
    }

    private async void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Child childToDelete)
        {
            System.Console.WriteLine($"Kindlöschung: {childToDelete.Name}. Bestätigung später basteln))");
            var messageBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Bestätigung", $"Sind Sie sicher, dass Kind {childToDelete.Name} muss gelöscht werden?", MsBox.Avalonia.Enums.ButtonEnum.YesNo);
            var result = await messageBox.ShowWindowDialogAsync(this);
            if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
            {
                _persistenceService.DeleteChild(childToDelete.Id);
                LoadAndDisplayChildren();
                if (_selectedChild?.Id == childToDelete.Id)
                {
                    _selectedChild = null;
                    var childrenListBox = this.FindControl<ListBox>("ChildrenListBox");
                    if (childrenListBox != null)
                    {
                        childrenListBox.SelectedItem = null;
                    }
                    UpdateLoginButtonState();
                }
            }
            else
            {
                return;
            }
        }
    }

    private void OpenEditChildWindow(Child? child)
    {
        var editWindow = new EditChildWindow(child);
        editWindow.ShowDialog(this).ContinueWith(task =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                LoadAndDisplayChildren();
            });
        });
    }

    private void OpenEditRewardWindow(Reward? reward)
    {
        var editWindow = new EditRewardWindow(reward);
        editWindow.ShowDialog(this);
    }

    private void RewardsButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Child child)
        {
            var rewardsWindow = new ChildRewardsWindow(child);
            rewardsWindow.ShowDialog(this).ContinueWith(task =>
            {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LoadAndDisplayChildren();
                });
            });
        }
    }

    private void ShowProfilesButton_Click(object sender, RoutedEventArgs e)
    {
        var profilesPanel = this.FindControl<StackPanel>("ProfilesPanel");
        var childrenListBox = this.FindControl<ListBox>("ChildrenListBox");
        var loginButton = this.FindControl<Button>("LoginButton");

        if (profilesPanel != null)
        {
            profilesPanel.IsVisible = !profilesPanel.IsVisible;
            if (profilesPanel.IsVisible)
            {
                LoadAndDisplayChildren();
            }
            else
            {
                if (childrenListBox != null)
                {
                    childrenListBox.SelectedItem = null;
                }
                if (loginButton != null)
                {
                    loginButton.IsEnabled = false;
                }
                _selectedChild = null;
            }
        }
    }

    private void LoadAndDisplayChildren()
    {
        _childrenList = _persistenceService.LoadChildren();

        if (_childrenListBox != null)
        {
            _childrenListBox.ItemsSource = _childrenList;
            _childrenListBox.SelectedItem = _selectedChild;
        }
    }

    private void ChildrenListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;

        if (listBox != null && listBox.SelectedItem is Child selected)
        {
            _selectedChild = selected;
            System.Console.WriteLine($"Kind {_selectedChild.Name} wurde gewählt");
        }
        else
        {
            _selectedChild = null;
        }
        UpdateLoginButtonState();
    }

    private void UpdateLoginButtonState()
    {
        var loginButton = this.FindControl<Button>("LoginButton");

        if (loginButton != null)
        {
            loginButton.IsEnabled = _selectedChild != null;
        }
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Child childToLogin)
        {
            _selectedChild = childToLogin;
            System.Console.WriteLine($"Log In für {_selectedChild.Name} {_selectedChild.Id}");
            var TrainerWindow = new TrainerWindow(_selectedChild);

            await TrainerWindow.ShowDialog(this).ContinueWith(task =>
            {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LoadAndDisplayChildren();
                });
            });

            var profilesPanel = this.FindControl<StackPanel>("ProfilesPanel");

            if (profilesPanel != null)
            {
                profilesPanel.IsVisible = true;
            }
        }
        else
        {
            System.Console.WriteLine("Kind wurde nicht ausgewählt");
        }
    }
}