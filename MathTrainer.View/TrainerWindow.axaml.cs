using Avalonia.Controls;
using Avalonia.Interactivity;
using MathTrainer.Model;
using MathTrainer.Persistence;
using MathTrainer.Model.Enums;
using MsBox.Avalonia;
using Avalonia.Threading;
using Avalonia.Media;
using System;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;

namespace MathTrainer.View;

public partial class TrainerWindow : Window
{
    private ChildPersistenceService _persistenceService;
    private Child _currentChild;
    private Random _random = new Random();

    private int _minRange;
    private int _maxRange;
    private OperationType _currentOperation;
    private EquationMode _currentMode;

    private int _operand1;
    private int _operand2;
    private int _result;
    private string? _problemText;
    private int _correctAnswer;

    private int _score = 0;
    private int _loadedScore = 0;
    private int _medals = 0;
    private int _loadedMedals = 0;
    private int _correctAnswersInRowForMedal = 0;
    private int _loadedCorrectAnswersInRowForMedal = 0;

    private string _currentAnswerString = "";
    private const int MaxAnswerLength = 3;
    private const int ProgressBarValue = 100;

    private DispatcherTimer _countdownTimer;
    private const int TimeLimitInSeconds = 10;
    private int _remainingSeconds;
    private TextBlock? _timerTextBlock;

    private TextBlock? _problemTextBlock;
    private TextBox? _answerTextBox;
    private TextBlock? _feedbackTextBlock;
    private ProgressBar? _correctAnswerProgressBar;
    private TextBlock? _scoreTextBlock;
    private Button? _selectedRangeButton;
    private Button? _selectedOperationButton;
    private Button? _selectedModeButton;

    public TrainerWindow(Child child)
    {
        InitializeComponent();
        InitializedSettingsButton();
        _currentChild = child;
        _persistenceService = new ChildPersistenceService();
        this.Title = $"Hallo, {_currentChild.Name}!";

        _score = _currentChild.TotalScore;
        _medals = _currentChild.Medals;
        _loadedScore = _currentChild.TotalScore;
        _loadedMedals = _currentChild.Medals;
        _correctAnswersInRowForMedal = _currentChild.AnswersInRowForMedal;
        _loadedCorrectAnswersInRowForMedal = _currentChild.AnswersInRowForMedal;

        _problemTextBlock = this.FindControl<TextBlock>("ProblemTextBlock");
        _answerTextBox = this.FindControl<TextBox>("AnswerTextBox");
        _feedbackTextBlock = this.FindControl<TextBlock>("FeedbackTextBlock");
        _correctAnswerProgressBar = this.FindControl<ProgressBar>("CorrectAnswersProgressBar");
        _scoreTextBlock = this.FindControl<TextBlock>("ScoreTextBlock");
        _timerTextBlock = this.FindControl<TextBlock>("TimerTextBlock");
        var trainingArea = this.FindControl<Border>("TrainingArea");
        var settingArea = this.FindControl<Border>("SettingArea");

        if (trainingArea != null && settingArea != null)
        {
            trainingArea.IsVisible = false;
            settingArea.IsVisible = true;
        }

        if (_answerTextBox != null)
        {
            _answerTextBox.Text = "";
        }

        try
        {
            var bgColor = Color.Parse(_currentChild.Backgroundcolor);
            this.Background = new SolidColorBrush(bgColor);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Fehler beim Hintergrundfarbeladen. {ex.Message}");
            this.Background = Brushes.White;
        }

        UpdateScoreDisplay();

        _countdownTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _countdownTimer.Tick += Timer_Tick;

        var submitButton = this.FindControl<Button>("SubmitAnswerButton");

        if (submitButton != null)
        {
            submitButton.Click += SubmitAnswerButton_Click;
        }

        var stopButton = this.FindControl<Button>("StopTrainingButton");

        if (stopButton != null)
        {
            stopButton.Click += StopTrainingButton_Click;
        }

        this.Closing += TrainerWindow_Closing;
    }

    public TrainerWindow()
    {
        InitializeComponent();
        _persistenceService = new ChildPersistenceService();
        _currentChild = new Child();
        _countdownTimer = new DispatcherTimer();
    }

    private void InitializedSettingsButton()
    {
        var rangePanel = this.FindControl<Panel>("RangeButtonsPanel");

        if (rangePanel?.Children.FirstOrDefault() is Button firstRangeButton)
        {
            SelectSettingButton(firstRangeButton, "Range");
        }

        var operationPanel = this.FindControl<Panel>("OperationButtonsPanel");

        if (operationPanel?.Children.FirstOrDefault() is Button firstOptionButton)
        {
            SelectSettingButton(firstOptionButton, "Operation");
        }

        var modePanel = this.FindControl<Panel>("ModeButtonsPanel");
        if (modePanel?.Children.FirstOrDefault() is Button firstModeButton)
        {
            SelectSettingButton(firstModeButton, "Mode");
        }
    }

    private void SettingButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button clickedButton && clickedButton.Tag is string tagString)
        {
            string groupType = tagString.Split('_')[0];
            SelectSettingButton(clickedButton, groupType);
        }
    }

    private void SelectSettingButton(Button buttonToSelect, string groupType)
    {
        Panel? parentPanel = buttonToSelect.Parent as Panel;
        if (parentPanel == null)
        {
            return;
        }

        Button? currentSelectedButtonField = null;
        switch (groupType)
        {
            case "Range": currentSelectedButtonField = _selectedRangeButton; break;
            case "Operation": currentSelectedButtonField = _selectedOperationButton; break;
            case "Mode": currentSelectedButtonField = _selectedModeButton; break;
        }

        if (currentSelectedButtonField != null && currentSelectedButtonField != buttonToSelect)
        {
            currentSelectedButtonField.Classes.Remove("Selected");
        }

        buttonToSelect.Classes.Add("Selected");

        switch (groupType)
        {
            case "Range": _selectedRangeButton = buttonToSelect; break;
            case "Operation": _selectedOperationButton = buttonToSelect; break;
            case "Mode": _selectedModeButton = buttonToSelect; break;
        }

        System.Console.WriteLine($"Button: {buttonToSelect.Content} gewählt, Tag: {buttonToSelect.Tag}");
    }

    private void TrainerWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        _countdownTimer.Stop();

        var trainingArea = this.FindControl<Border>("TrainingArea");
        bool trainingWasActive = trainingArea != null && trainingArea.IsVisible;

        if (_score != _loadedScore || _medals != _loadedMedals || trainingWasActive)
        {
            if (trainingWasActive)
            {
                Debug.WriteLine("Training wurde unterbrochen. Wird geschpeichert");
                System.Console.WriteLine("Training wurde unterbrochen. Wird geschpeichert");
                SaveChanges();
            }
        }
    }

    public void StartTrainingButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!ParseSettings())
        {
            return;
        }

        _score = _currentChild.TotalScore;
        _medals = _currentChild.Medals;

        System.Console.WriteLine($"Trainingbeginn für: {_currentChild.Name}");
        System.Console.WriteLine($"Zahlenbereich: {_minRange} - {_maxRange}");
        System.Console.WriteLine($"Operator: {_currentOperation}");
        System.Console.WriteLine($"Mode: {_currentMode}");

        var trainingArea = this.FindControl<Border>("TrainingArea");

        if (trainingArea != null)
        {
            trainingArea.IsVisible = true;
        }

        var settingArea = this.FindControl<Border>("SettingArea");

        if (settingArea != null)
        {
            settingArea.IsVisible = false;
        }

        _correctAnswersInRowForMedal = _currentChild.AnswersInRowForMedal;
        UpdateScoreDisplay();

        if (_correctAnswerProgressBar != null)
        {
            _correctAnswerProgressBar.Value = _correctAnswersInRowForMedal;
        }

        if (_feedbackTextBlock != null)
        {
            _feedbackTextBlock.Text = "";
        }

        GenerateNewProblem();
    }

    private bool ParseSettings()
    {
        if (_selectedRangeButton == null || !(_selectedRangeButton.Tag is string rangeTag))
        {
            return false;
        }

        string rangeValueStr = rangeTag.Split('_')[1];

        if (!int.TryParse(rangeValueStr, out _maxRange))
        {
            return false;
        }

        _minRange = 0;

        if (_selectedOperationButton == null || !(_selectedOperationButton.Tag is string operationTag))
        {
            return false;
        }

        string operationValueStr = operationTag.Split('_')[1];

        if (operationValueStr == "Addition")
        {
            _currentOperation = OperationType.Addition;
        }
        else if (operationValueStr == "Subtraction")
        {
            _currentOperation = OperationType.Subtraction;
        }
        else
        {
            return false;
        }

        if (_selectedModeButton == null || !(_selectedModeButton.Tag is string modeTag))
        {
            return false;
        }

        string modeValueStr = modeTag.Split('_')[1];

        if (modeValueStr == "ResultUnknown")
        {
            _currentMode = EquationMode.ResultUnknown;
        }
        else if (modeValueStr == "FirstOperandUnknown")
        {
            _currentMode = EquationMode.FirstOperandUnknown;
        }
        else if (modeValueStr == "SecondOperandUnknown")
        {
            _currentMode = EquationMode.SecondOperandUnknown;
        }
        else
        {
            return false;
        }

        return true;
    }

    private void GenerateNewProblem()
    {
        bool problemGenerated = false;
        int attempts = 0;
        while (!problemGenerated && attempts < 100)
        {
            attempts++;
            try
            {
                switch (_currentOperation)
                {
                    case OperationType.Addition:
                        switch (_currentMode)
                        {
                            case EquationMode.ResultUnknown:
                                _operand1 = _random.Next(0, _maxRange + 1);
                                _operand2 = _random.Next(0, _maxRange - _operand1 + 1);
                                _result = _operand1 + _operand2;
                                _correctAnswer = _result;
                                _problemText = $"{_operand1} + {_operand2} = ?";
                                problemGenerated = true;
                                break;

                            case EquationMode.FirstOperandUnknown:
                                _operand2 = _random.Next(0, _maxRange + 1);
                                _result = _random.Next(_operand2, _maxRange + 1);
                                _operand1 = _result - _operand2;
                                _correctAnswer = _operand1;
                                _problemText = $"? + {_operand2} = {_result}";
                                problemGenerated = true;
                                break;

                            case EquationMode.SecondOperandUnknown:
                                _operand1 = _random.Next(0, _maxRange + 1);
                                _result = _random.Next(_operand1, _maxRange + 1);
                                _operand2 = _result - _operand1;
                                _correctAnswer = _operand2;
                                _problemText = $"{_operand1} + ? = {_result}";
                                problemGenerated = true;
                                break;
                        }
                        break;

                    case OperationType.Subtraction:
                        switch (_currentMode)
                        {
                            case EquationMode.ResultUnknown:
                                _operand1 = _random.Next(0, _maxRange + 1);
                                _operand2 = _random.Next(0, _operand1 + 1);
                                _result = _operand1 - _operand2;
                                _correctAnswer = _result;
                                _problemText = $"{_operand1} - {_operand2} = ?";
                                problemGenerated = true;
                                break;

                            case EquationMode.FirstOperandUnknown:
                                _operand2 = _random.Next(0, _maxRange + 1);
                                _result = _random.Next(0, _maxRange - _operand2 + 1);
                                _operand1 = _result + _operand2;
                                _correctAnswer = _operand1;
                                _problemText = $"? - {_operand2} = {_result}";
                                problemGenerated = true;
                                break;

                            case EquationMode.SecondOperandUnknown:
                                _operand1 = _random.Next(0, _maxRange + 1);
                                _result = _random.Next(0, _operand1 + 1);
                                _operand2 = _operand1 - _result;
                                _correctAnswer = _operand2;
                                _problemText = $"{_operand1} - ? = {_result}";
                                problemGenerated = true;
                                break;
                        }
                        break;
                }
            }
            catch (ArgumentOutOfRangeException aex)
            {
                System.Console.WriteLine($"Fehler beim Randomzahlgenerieren: {aex.Message}. Versuch Nr.{attempts}");
                problemGenerated = false;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Fehler {ex.Message}");
                problemGenerated = false;
            }
        }

        if (!problemGenerated)
        {
            _problemText = "Aufgabegenerierungsfehler";
            _correctAnswer = 0;
            System.Console.WriteLine($"Kritische Fehler! Nach 100 Versuchen Aufgabe wurde nicht generiert");
        }

        if (_problemTextBlock != null)
        {
            _problemTextBlock.Text = _problemText;
        }
        _currentAnswerString = "";
        if (_answerTextBox != null)
        {
            _answerTextBox.Text = _currentAnswerString;
        }
        if (_feedbackTextBlock != null)
        {
            _feedbackTextBlock.Text = "";
        }
        System.Console.WriteLine($"Neue Aufgabe: {_problemText} (Lösung: {_correctAnswer})");

        _remainingSeconds = TimeLimitInSeconds;
        if (_timerTextBlock != null)
        {
            _timerTextBlock.Text = $"Zeit: {_remainingSeconds}";
        }
        _countdownTimer.Start();
    }

    private string GetOperationSymbol()
    {
        return _currentOperation == OperationType.Addition ? "+" :
               _currentOperation == OperationType.Subtraction ? "-" :
               _currentOperation == OperationType.Multiplication ? "*" : ":";
    }

    private void KeypadButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Content is string digital)
        {
            if (_currentAnswerString.Length < MaxAnswerLength)
            {
                _currentAnswerString += digital;
                if (_answerTextBox != null)
                {
                    _answerTextBox.Text = _currentAnswerString;
                }
            }
        }
    }

    private void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_currentAnswerString))
        {
            _currentAnswerString = _currentAnswerString.Substring(0, _currentAnswerString.Length - 1);
            if (_answerTextBox != null)
            {
                _answerTextBox.Text = _currentAnswerString;
            }
        }
    }

    private void SubmitAnswerButton_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentAnswerString))
        {
            if (_feedbackTextBlock != null)
            {
                _feedbackTextBlock.Text = "Gib eine Antwort ein!";
            }
            return;
        }

        if (!int.TryParse(_currentAnswerString, out int userAnswer))
        {
            if (_feedbackTextBlock != null)
            {
                _feedbackTextBlock.Text = "Ups. Etwas stimmt nicht. Hast Du eine Zahl eingegeben?";
            }

            _currentAnswerString = "";

            if (_answerTextBox != null)
            {
                _answerTextBox.Text = _currentAnswerString;
            }
            return;
        }

        int points = 1;
        if (_maxRange == 100 && _currentMode != EquationMode.ResultUnknown)
        {
            points = 3;
        }
        else if (_maxRange >= 20 && _currentMode != EquationMode.ResultUnknown)
        {
            points = 2;
        }

        _countdownTimer.Stop();

        if (userAnswer == _correctAnswer)
        {
            if (_feedbackTextBlock != null)
            {
                _feedbackTextBlock.Text = "Richtig";
            }
            _score+=points;
            _correctAnswersInRowForMedal++;

            if (_correctAnswerProgressBar != null)
            {
                _correctAnswerProgressBar.Value = _correctAnswersInRowForMedal;
            }

            if (_correctAnswersInRowForMedal >= ProgressBarValue)
            {
                _medals++;
                _correctAnswersInRowForMedal = 0;

                if (_correctAnswerProgressBar != null)
                {
                    _correctAnswerProgressBar.Value = 0;
                }

                if (_feedbackTextBlock != null)
                {
                    _feedbackTextBlock.Text = "Klasse! Du bekommst eine Medalie!";
                }
            }
        }
        else
        {
            if (_feedbackTextBlock != null)
            {
                _feedbackTextBlock.Text = $"Leider Falsch!\nRichtige Antwort: {_correctAnswer}";
            }
        }

        UpdateScoreDisplay();
        _currentAnswerString = "";
        GenerateNewProblem();
    }

    private void StopTrainingButton_Click(object? sender, RoutedEventArgs e)
    {
        _countdownTimer.Stop();

        SaveChanges();

        var trainingArea = this.FindControl<Border>("TrainingArea");

        if (trainingArea != null)
        {
            trainingArea.IsVisible = false;
        }

        var settingsBorder = this.FindControl<Border>("SettingArea");

        if (settingsBorder != null)
        {
            settingsBorder.IsVisible = true;
        }

        if (_feedbackTextBlock != null)
        {
            _feedbackTextBlock.Text = "";
        }

        if (_answerTextBox != null)
        {
            _answerTextBox.Text = "";
        }

        Debug.WriteLine($"Training ist beendet");
    }

    private void SaveChanges()
    {
        _currentChild.TotalScore = _score;
        _currentChild.Medals = _medals;
        _currentChild.AnswersInRowForMedal = _correctAnswersInRowForMedal;
        _persistenceService.UpdateChild(_currentChild);
        _loadedScore = _score;
        _loadedMedals = _medals;
        _loadedCorrectAnswersInRowForMedal = _correctAnswersInRowForMedal;
        Debug.WriteLine($"Progress für {_currentChild.Name} wurde gespeichert. Punkte: {_currentChild.TotalScore}, Medalien: {_currentChild.Medals}");
        System.Console.WriteLine($"Progress für {_currentChild.Name} wurde gespeichert. Punkte: {_currentChild.TotalScore}, Medalien: {_currentChild.Medals}");
    }

    private void UpdateScoreDisplay()
    {
        if (_scoreTextBlock != null)
        {
            _scoreTextBlock.Text = $"Punkte: {_score}  Medalien: {_medals}";
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _remainingSeconds--;

        if (_timerTextBlock != null)
        {
            _timerTextBlock.Text = $"Zeit: {_remainingSeconds}";
        }

        if (_remainingSeconds <= 0)
            {
                _countdownTimer.Stop();

                if (_feedbackTextBlock != null)
                {
                    _feedbackTextBlock.Text = $"Die Zeit ist um! Die Antwort ist: {_correctAnswer}";
                }
                GenerateNewProblem();
            }
    }
}