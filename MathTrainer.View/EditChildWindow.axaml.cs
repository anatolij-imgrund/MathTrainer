using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MathTrainer.Model;
using MathTrainer.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Input;
using Avalonia.Controls.Presenters;

namespace MathTrainer.View;

public partial class EditChildWindow : Window
{
    private ChildPersistenceService _persistenceService;
    private Child _editingChild;
    private bool _isEditMode;

    private List<string> _avatarUris = new List<string>();
    private string? _selectedAvatarUri;
    private string _selectedColorHex = "#FFFFFFFF";

    private ItemsControl? _avatarsItemsControl;
    private TextBox? _nameTextBox;
    private Panel? _colorButtonsPanel;

    public EditChildWindow(Child? childToEdit = null)
    {
        InitializeComponent();
        _persistenceService = new ChildPersistenceService();
        _editingChild = childToEdit ?? new Child();
        _isEditMode = childToEdit != null;

        _nameTextBox = this.FindControl<TextBox>("NameTextBox");
        _avatarsItemsControl = this.FindControl<ItemsControl>("AvatarsItemsControl");
        _colorButtonsPanel = this.FindControl<Panel>("ColorButtonsPanel");

        LoadAvailableavatars();
        PopulateForm();

        if (_isEditMode)
        {
            this.Title = "Profil bearbeiten";
        }
        else
        {
            this.Title = "Neues Profil erstellen";
        }
    }

    public EditChildWindow()
    {
        InitializeComponent();
        _persistenceService = new ChildPersistenceService();
        _editingChild = new Child();
    }

    private void LoadAvailableavatars()
    {
        try
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var baseUri = $"avares://{assemblyName}/Assets/Avatars/";
            var assets = AssetLoader.GetAssets(new Uri(baseUri), null);

            _avatarUris = assets.Select(uri => uri.ToString()).Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg")).ToList();

            if (_avatarsItemsControl != null)
            {
                _avatarsItemsControl.ItemsSource = _avatarUris.Select(uri => new Bitmap(AssetLoader.Open(new Uri(uri)))).ToList();
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Fehler bem Avatarshochladen. {ex.Message}");
        }
    }

    private void PopulateForm()
    {
        if (_nameTextBox != null)
        {
            _nameTextBox.Text = _editingChild.Name;
        }

        _selectedAvatarUri = _editingChild.AvatarImagePath;
        _selectedColorHex = _editingChild.Backgroundcolor;

        UpdateAvatarSelection();
        UpdateColorSelection();
    }

    private void Avatar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Bitmap bitmap)
        {
            var items = _avatarsItemsControl?.ItemsSource as List<Bitmap>;
            var index = items?.IndexOf(bitmap);

            if (index.HasValue && index.Value >= 0 && index.Value < _avatarUris.Count)
            {
                _selectedAvatarUri = _avatarUris[index.Value];
                UpdateAvatarSelection();
                System.Console.WriteLine($"Avatar [{_selectedAvatarUri}] wurde ausgewählt.");
            }
        }
    }

    private void UpdateAvatarSelection()
    {
        if (_avatarsItemsControl == null || _avatarsItemsControl.ItemsSource == null)
        {
            return;
        }

        int selectedIndex = _avatarUris.IndexOf(_selectedAvatarUri ?? "");

        foreach (var container in _avatarsItemsControl.GetRealizedContainers())
        {
            if (container is ContentPresenter presenter && presenter.Child is Border border)
            {
                int currentIndex = _avatarsItemsControl.IndexFromContainer(container);

                if (currentIndex == selectedIndex)
                {
                    border.Classes.Add("Selected");
                }
                else
                {
                    border.Classes.Remove("Selected");
                }
            }
        }
    }

    private void ColorButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string colorHex)
        {
            _selectedColorHex = colorHex;
            UpdateColorSelection();
            System.Console.WriteLine($"Farbe {_selectedColorHex} wurde ausgewählt.");
        }
    }

    private void UpdateColorSelection()
    {
        if (_colorButtonsPanel == null)
        {
            return;
        }

        foreach (var child in _colorButtonsPanel.Children)
        {
            if (child is Button button)
            {
                if (button.Tag?.ToString() == _selectedColorHex)
                {
                    button.Classes.Add("Selected");
                }
                else
                {
                    button.Classes.Remove("Selected");
                }
            }
        }
    }

    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_nameTextBox == null || string.IsNullOrWhiteSpace(_nameTextBox.Text))
        {
            System.Console.WriteLine("Name darf nicht leer sein");
            return;
        }

        if (string.IsNullOrEmpty(_selectedAvatarUri))
        {
            System.Console.WriteLine("Avatarbild wurde nicht gewählt");
            return;
        }

        _editingChild.Name = _nameTextBox.Text.Trim();
        _editingChild.AvatarImagePath = _selectedAvatarUri;
        _editingChild.Backgroundcolor = _selectedColorHex;

        if (_isEditMode)
        {
            _persistenceService.UpdateChild(_editingChild);
        }
        else
        {
            _persistenceService.AddChild(_editingChild);
        }
        Close();
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}