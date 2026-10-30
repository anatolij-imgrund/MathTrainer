using Avalonia.Controls;
using Avalonia.Interactivity;
using MsBox.Avalonia;
using MathTrainer.Model;
using MathTrainer.Persistence;
using MathTrainer.Model.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace MathTrainer.View
{
    public partial class ChildRewardsWindow : Window
    {
        private Child _currentChild;
        private RewardPersistenceService _rewardService;
        private RewardRequestPersistenceService _requestService;
        private ChildPersistenceService _childService;

        private TextBlock? _childNameTextBlock;
        private TextBlock? _medalsTextBlock;
        private ItemsControl? _availableRewardsItemsControl;
        private ListBox? _myRequestsListBox;

        public ChildRewardsWindow(Child child)
        {
            InitializeComponent();
            _currentChild = child;

            _rewardService = new RewardPersistenceService();
            _requestService = new RewardRequestPersistenceService();
            _childService = new ChildPersistenceService();

            _childNameTextBlock = this.FindControl<TextBlock>("ChildNameTextBlock");
            _medalsTextBlock = this.FindControl<TextBlock>("MedalsTextBlock");
            _availableRewardsItemsControl = this.FindControl<ItemsControl>("AvailableRewardsItemsControl");
            _myRequestsListBox = this.FindControl<ListBox>("MyRequestsListBox");

            UpdateDisplay();
        }

        public ChildRewardsWindow()
        {
            InitializeComponent();
            _currentChild = new Child();
            _rewardService = new RewardPersistenceService();
            _requestService = new RewardRequestPersistenceService();
            _childService = new ChildPersistenceService();
        }

        private void UpdateDisplay()
        {
            if (_childNameTextBlock != null)
            {
                _childNameTextBlock.Text = _currentChild.Name;
            }
            if (_medalsTextBlock != null)
            {
                _medalsTextBlock.Text = $"Medalien: {_currentChild.Medals}";
            }

            LoadAvailableRewards();
            LoadMyRequests();
        }

        private void LoadAvailableRewards()
        {
            var allRewards = _rewardService.LoadRewards();
            var availableRewards = allRewards.Select(r => new RewardView(r, _currentChild.Medals))
                                             .OrderBy(r => r.Price).ToList();
            if (_availableRewardsItemsControl != null)
            {
                _availableRewardsItemsControl.ItemsSource = availableRewards;
            }
        }

        private void LoadMyRequests()
        {
            var allRequests = _requestService.LoadRequests();
            var allRewards = _rewardService.LoadRewards();

            var myRequests = allRequests.Where(request => request.ChildId == _currentChild.Id)
                                        .Select(request =>
                                        {
                                            request.RewardName = allRewards.FirstOrDefault(r => r.Id == request.RewardId)?.Name ?? "Unbekannt";
                                            return request;
                                        }).OrderByDescending(request => request.RequestDate).ToList();
            if (_myRequestsListBox != null)
            {
                _myRequestsListBox.ItemsSource = myRequests;
            }
        }

        private async void RequestButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RewardView rewardV)
            {
                if (_currentChild.Medals < rewardV.Price)
                {
                    await ShowMessage("Ups!", "Du hast nicht genug Medalien =(");
                    return;
                }

                bool confirmed = await ShowConfirmation($"Sicher?", $"Willst Du {rewardV.Name} für {rewardV.Price} 🏅 bekommen?");

                if (confirmed)
                {
                    _currentChild.Medals -= rewardV.Price;
                    _childService.UpdateChild(_currentChild);

                    var newRequest = new RewardRequest
                    {
                        ChildId = _currentChild.Id,
                        RewardId = rewardV.Id,
                        Status = RequestStatus.Pendung
                    };
                    _requestService.AddRequests(newRequest);

                    System.Console.WriteLine($"Kind {_currentChild.Name} hat {rewardV.Name} bestellt.");

                    UpdateDisplay();
                    await ShowMessage("Klasse!", "Deine Bestellung wurde an Eltern geschickt.");
                }
            }
        }

        private async Task<bool> ShowConfirmation(string title, string message)
        {
            try
            {
                var box = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(title, message, MsBox.Avalonia.Enums.ButtonEnum.YesNo, MsBox.Avalonia.Enums.Icon.Question);
                var result = await box.ShowWindowDialogAsync(this);
                return result == MsBox.Avalonia.Enums.ButtonResult.Yes;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Fehler: {ex.Message}");
                return true;
            }
        }

        private async Task ShowMessage(string titel, string message)
        {
            try
            {
                var box = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(titel, message);
                await box.ShowWindowDialogAsync(this);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Fehler: {ex.Message}");
            }
        }
    }
}