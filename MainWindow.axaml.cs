using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace minutnik;

public partial class MainWindow : Window
{
    private readonly List<string> _history = new();
    private readonly DispatcherTimer _timer;
    private TimeSpan _remainingTime;
    private bool _isRunning;
    private bool _isWorkSession = true;
    private int _cycles = 0;

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;

        ResetToCurrentSession();
    }

    private TimeSpan GetWorkDuration()
    {
        if (double.TryParse(WorkTimeTextBox.Text, out double minutes) && minutes > 0)
        {
            return TimeSpan.FromMinutes(minutes);
        }
        return TimeSpan.FromMinutes(25);
    }

    private TimeSpan GetBreakDuration()
    {
        if (double.TryParse(BreakTimeTextBox.Text, out double minutes) && minutes > 0)
        {
            return TimeSpan.FromMinutes(minutes);
        }
        return TimeSpan.FromMinutes(5);
    }

    private void StartPauseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            PauseTimer();
        }
        else
        {
            StartTimer();
        }
    }

    private void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        PauseTimer();
        _isWorkSession = true;
        ResetToCurrentSession();
    }

    private void StartTimer()
    {
        _isRunning = true;
        StartPauseButton.Content = "Pauza";
        _timer.Start();
    }

    private void PauseTimer()
    {
        _isRunning = false;
        StartPauseButton.Content = "Start";
        _timer.Stop();
    }

    private void ResetToCurrentSession()
    {
        _remainingTime = _isWorkSession ? GetWorkDuration() : GetBreakDuration();
        UpdateDisplay();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_remainingTime > TimeSpan.Zero)
        {
            _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
            UpdateDisplay();
        }

        if (_remainingTime <= TimeSpan.Zero)
        {
            if (_isWorkSession)
            {
                _cycles++;
                CyclesTextBlock.Text = _cycles.ToString();
                _history.Add($"{DateTime.Now:HH:mm} — cykl {_cycles}"); HistoryList.ItemsSource = _history.ToArray();
                _isWorkSession = false;
                ResetToCurrentSession();
                StartTimer();
            }
            else
            {
                _isWorkSession = true;
                ResetToCurrentSession();
                StartTimer();
            }
        }
    }

    private void UpdateDisplay()
    {
        StatusTextBlock.Text = _isWorkSession ? "Stan: Praca" : "Stan: Przerwa";
        TimerTextBlock.Text = $"{(int)_remainingTime.TotalMinutes:D2}:{_remainingTime.Seconds:D2}";
    }
}
