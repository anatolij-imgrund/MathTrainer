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

public partial class EditRewardWindow : Window
{
    private RewardPersistenceService _persistenceRewardService;
    private List<Reward> _rewardsList;

    private List<string> _rewardsUris = new List<string>();
    private string? _selectedRewardUri;
    private Reward? _selectedReward;

    private ItemsControl? _rewardsItemsControl;
    private TextBox? _rewardTextBox;
    private NumericUpDown? _rewardCost;
    private ListBox? _rewardsListBox;

    public EditRewardWindow(Reward? rewardToEdit = null)
    {
        InitializeComponent();
        _persistenceRewardService = new RewardPersistenceService();
        _rewardsList = new List<Reward>();

        _rewardTextBox = this.FindControl<TextBox>("RewardTextBox");
        _rewardCost = this.FindControl<NumericUpDown>("RewardCostBox");
        _rewardsItemsControl = this.FindControl<ItemsControl>("RewardsItemsControl");
        _rewardsListBox = this.FindControl<ListBox>("RewardsListBox");

        LoadAvailableRewards();
        //PopulateForm();
        LoadAndDisplayRewards();
    }

    public EditRewardWindow()
    {
        InitializeComponent();
        _persistenceRewardService = new RewardPersistenceService();
        _rewardsList = new List<Reward>();
    }

    private void LoadAvailableRewards()
    {
        try
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var baseUri = $"avares://{assemblyName}/Assets/Rewards/";
            var assets = AssetLoader.GetAssets(new Uri(baseUri), null);

            _rewardsUris = assets.Select(uri => uri.ToString()).Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".jpeg")).ToList();

            if (_rewardsItemsControl != null)
            {
                _rewardsItemsControl.ItemsSource = _rewardsUris.Select(uri => new Bitmap(AssetLoader.Open(new Uri(uri)))).ToList();
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Fehler bem Belohnungsbildhochladen. {ex.Message}");
        }
    }

    private void PopulateForm(Reward? reward)
    {
        if (reward != null)
        {
            if (_rewardTextBox != null)
            {
                _rewardTextBox.Text = reward.Name;
            }
            if (_rewardCost != null)
            {
                _rewardCost.Value = reward.Price;
            }
            _selectedRewardUri = reward.ImagePath;
            UpdateRewardImgSelection();
        }
        else
        {
            ClearForm();
        }
    }

    private void ClearForm()
    {
        if (_rewardTextBox != null)
        {
            _rewardTextBox.Text = "";
        }
        if (_rewardCost != null)
        {
            _rewardCost.Value = 1;
        }
        _selectedRewardUri = null;
        _selectedReward = null;
        if (_rewardsListBox != null)
        {
            _rewardsListBox.SelectedItem = null;
        }
        UpdateRewardImgSelection();
    }

    private void RewardImg_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is Bitmap bitmap)
        {
            var items = _rewardsItemsControl?.ItemsSource as List<Bitmap>;
            var index = items?.IndexOf(bitmap);

            if (index.HasValue && index.Value >= 0 && index.Value < _rewardsUris.Count)
            {
                _selectedRewardUri = _rewardsUris[index.Value];
                UpdateRewardImgSelection();
                System.Console.WriteLine($"Belohnungsbild [{_selectedRewardUri}] wurde ausgewählt.");
            }
        }
    }

    private void UpdateRewardImgSelection()
    {
        if (_rewardsItemsControl == null || _rewardsItemsControl.ItemsSource == null)
        {
            return;
        }

        int selectedIndex = _rewardsUris.IndexOf(_selectedRewardUri ?? "");

        foreach (var container in _rewardsItemsControl.GetRealizedContainers())
        {
            if (container is ContentPresenter presenter && presenter.Child is Border border)
            {
                int currentIndex = _rewardsItemsControl.IndexFromContainer(container);

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

    private void LoadAndDisplayRewards()
    {
        _rewardsList = _persistenceRewardService.LoadRewards();
        //var rewardsListBox = this.FindControl<ListBox>("RewardsListBox");

        if (_rewardsListBox != null)
        {
            _rewardsListBox.ItemsSource = _rewardsList;
            _rewardsListBox.SelectedItem = _selectedReward;
        }
    }

    private async void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is Reward rewardToDelete)
        {
            System.Console.WriteLine($"Belohnunglöschen: {rewardToDelete.Name}. Bestätigung später basteln))");
            var messageBox = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard("Bestätigung", $"Sind Sie sicher, dass Kind {rewardToDelete.Name} muss gelöscht werden?", MsBox.Avalonia.Enums.ButtonEnum.YesNo);
            var result = await messageBox.ShowWindowDialogAsync(this);
            if (result == MsBox.Avalonia.Enums.ButtonResult.Yes)
            {
                _persistenceRewardService.DeleteReward(rewardToDelete.Id);
                
                if (_selectedReward?.Id == rewardToDelete.Id)
                {
                    ClearForm();
                }
                LoadAndDisplayRewards();
            }
            else
            {
                return;
            }
        }
    }

    private void RewardsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;

        if (listBox != null && listBox.SelectedItem is Reward selected)
        {
            _selectedReward = selected;
            PopulateForm(_selectedReward);
            System.Console.WriteLine($"Belohnung {_selectedReward.Name} wurde gewählt");
        }
        else
        {
            _selectedReward = null;
        }
    }

    private void ShowRequestsButton_Click(object? sender, RoutedEventArgs e)
    {
        var requestsWindow = new RewardRequestsWindow();
        requestsWindow.ShowDialog(this);
    }

    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_rewardTextBox == null || _rewardCost == null
            || string.IsNullOrWhiteSpace(_rewardTextBox.Text)
            || string.IsNullOrEmpty(_selectedRewardUri))
        {
            System.Console.WriteLine($"Fehler! Die felder sind nicht correct ausgefüllt.");
            return;
        }

        bool isEditMode = _selectedReward != null;

        Reward rewardToSave = _selectedReward ?? new Reward();

        rewardToSave.Name = _rewardTextBox.Text.Trim();
        rewardToSave.Price = Convert.ToInt32(_rewardCost.Value ?? 1);
        rewardToSave.ImagePath = _selectedRewardUri;

        if (isEditMode)
        {
            _persistenceRewardService.UpdateReward(rewardToSave);
        }
        else
        {
            _persistenceRewardService.AddReward(rewardToSave);
        }

        ClearForm();
        LoadAndDisplayRewards();
    }

    private void ClearButton_Click(object? sender, RoutedEventArgs e)
    {
        ClearForm();
    }
}