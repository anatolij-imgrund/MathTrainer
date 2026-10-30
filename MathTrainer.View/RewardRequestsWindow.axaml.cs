using Avalonia.Controls;
using Avalonia.Interactivity;
using MathTrainer.Model;
using MathTrainer.Persistence;
using MathTrainer.Model.Enums;
using System.Collections.Generic;
using System.Linq;


namespace MathTrainer.View;

public partial class RewardRequestsWindow : Window
{
    private RewardRequestPersistenceService _requestService;
    private ChildPersistenceService _childService;
    private RewardPersistenceService _rewardService;

    private ListBox? _requestsListBox;
    private TextBlock? _noRequestsTextBlock;

    public RewardRequestsWindow()
    {
        InitializeComponent();
        _requestService = new RewardRequestPersistenceService();
        _childService = new ChildPersistenceService();
        _rewardService = new RewardPersistenceService();

        _requestsListBox = this.FindControl<ListBox>("RequestsListBox");
        _noRequestsTextBlock = this.FindControl<TextBlock>("NoRequestsTextBlock");

        LoadAndDisplayRequests();
    }

    private void LoadAndDisplayRequests()
    {
        var allRequests = _requestService.LoadRequests();
        var allChildren = _childService.LoadChildren();
        var allRewards = _rewardService.LoadRewards();

        var pendingRequests = allRequests.Where(request => request.Status == RequestStatus.Pendung)
                                         .Select(request =>
                                         {
                                             request.ChildName = allChildren.FirstOrDefault(c => c.Id == request.ChildId)?.Name ?? "Unbekantes Kind";
                                             request.RewardName = allRewards.FirstOrDefault(r => r.Id == request.RewardId)?.Name ?? "Unbekante Belohnung";
                                             return request;
                                         }).OrderBy(r => r.RequestDate).ToList();
        if (_requestsListBox != null)
        {
            _requestsListBox.ItemsSource = pendingRequests;
            bool hasRequests = pendingRequests.Any();
            _requestsListBox.IsVisible = hasRequests;

            if (_noRequestsTextBlock != null)
            {
                _noRequestsTextBlock.IsVisible = !hasRequests;
            }
        }
    }

    private void IssueButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is RewardRequest requestToIssue)
        {
            System.Console.WriteLine($"Belohnungsausgabe: {requestToIssue.RewardName}. Für: {requestToIssue.ChildName}");
            requestToIssue.Status = RequestStatus.Issued;
            _requestService.UpdateRequests(requestToIssue);
            LoadAndDisplayRequests();
        }
    }
}