using System.Windows;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using WpfApp.Models;
using WpfApp.Services;
using WpfApp.ViewModels;
using WpfApp.Views;

namespace WpfApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly FavoritesService _favoritesService;
    private readonly AppStateService _appStateService;
    private CartesianChart? _temperatureChart;
    private CartesianChart? _pressureChart;
    private CartesianChart? _speedChart;
    private readonly List<double> _temperatureData = new();
    private readonly List<double> _pressureData = new();
    private readonly List<double> _speedData = new();
    
    public MainWindow(MainWindowViewModel viewModel, FavoritesService favoritesService, AppStateService appStateService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _favoritesService = favoritesService;
        _appStateService = appStateService;
        DataContext = viewModel;
        
        InitializeCharts();
        _appStateService.PropertyChanged += OnAppStateChanged;
    }

    private void InitializeCharts()
    {
        // Create a grid for the three charts
        var grid = new System.Windows.Controls.Grid();
        grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
        grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
        grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

        // Temperature Chart
        _temperatureChart = new CartesianChart
        {
            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = _temperatureData,
                    Name = "Temperature",
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0.5
                }
            },
            YAxes = new[] { new Axis { Name = "°C", MinLimit = 0, MaxLimit = 100 } },
            XAxes = new[] { new Axis { IsVisible = false } },
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Top
        };
        System.Windows.Controls.Grid.SetColumn(_temperatureChart, 0);
        grid.Children.Add(_temperatureChart);

        // Pressure Chart
        _pressureChart = new CartesianChart
        {
            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = _pressureData,
                    Name = "Pressure",
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0.5
                }
            },
            YAxes = new[] { new Axis { Name = "bar", MinLimit = 0, MaxLimit = 10 } },
            XAxes = new[] { new Axis { IsVisible = false } },
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Top
        };
        System.Windows.Controls.Grid.SetColumn(_pressureChart, 1);
        grid.Children.Add(_pressureChart);

        // Speed Chart
        _speedChart = new CartesianChart
        {
            Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = _speedData,
                    Name = "Speed",
                    Fill = null,
                    GeometrySize = 0,
                    LineSmoothness = 0.5
                }
            },
            YAxes = new[] { new Axis { Name = "RPM", MinLimit = 0, MaxLimit = 2500 } },
            XAxes = new[] { new Axis { IsVisible = false } },
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Top
        };
        System.Windows.Controls.Grid.SetColumn(_speedChart, 2);
        grid.Children.Add(_speedChart);

        ChartsContainer.Children.Add(grid);
    }

    private void OnAppStateChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppStateService.MachineStatus) && _appStateService.MachineStatus != null)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateCharts(_appStateService.MachineStatus);
            });
        }
    }

    private void UpdateCharts(MachineStatus status)
    {
        const int maxPoints = 50;

        // Add new data points
        _temperatureData.Add(status.MotorTemperature);
        _pressureData.Add(status.OilPressure);
        _speedData.Add(status.MotorSpeed);

        // Keep only last 50 points
        if (_temperatureData.Count > maxPoints) _temperatureData.RemoveAt(0);
        if (_pressureData.Count > maxPoints) _pressureData.RemoveAt(0);
        if (_speedData.Count > maxPoints) _speedData.RemoveAt(0);

        // Update chart data - charts update automatically through data binding
    }

    private void ManageFavorites_Click(object sender, RoutedEventArgs e)
    {
        var favoritesWindow = new FavoritesWindow(_favoritesService);
        favoritesWindow.Owner = this;
        if (favoritesWindow.ShowDialog() == true && favoritesWindow.SelectedFavorite != null)
        {
            LoadFavoriteRecipe(favoritesWindow.SelectedFavorite);
        }
    }

    private void LoadFavorite_Click(object sender, RoutedEventArgs e)
    {
        var favoritesWindow = new FavoritesWindow(_favoritesService);
        favoritesWindow.Owner = this;
        if (favoritesWindow.ShowDialog() == true && favoritesWindow.SelectedFavorite != null)
        {
            LoadFavoriteRecipe(favoritesWindow.SelectedFavorite);
        }
    }

    private void SaveAsFavorite_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.CurrentRecipe == null || string.IsNullOrWhiteSpace(_viewModel.CurrentRecipe.Name))
        {
            MessageBox.Show("Please enter a recipe name first.", "Cannot Save", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var favorite = new FavoriteRecipe
        {
            Name = _viewModel.CurrentRecipe.Name,
            PreparationVolume = _viewModel.CurrentRecipe.PreparationVolume,
            PreparationConcentration = _viewModel.CurrentRecipe.PreparationConcentration,
            Ingredients = new List<Ingredient>(_viewModel.CurrentRecipe.Ingredients)
        };

        if (_favoritesService.AddFavorite(favorite))
        {
            MessageBox.Show($"Recipe '{favorite.Name}' saved as favorite!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("Failed to save favorite.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadFavoriteRecipe(FavoriteRecipe favorite)
    {
        _viewModel.CurrentRecipe = new Recipe
        {
            Name = favorite.Name,
            PreparationVolume = favorite.PreparationVolume,
            PreparationConcentration = favorite.PreparationConcentration,
            Ingredients = new List<Ingredient>(favorite.Ingredients)
        };
        
        MessageBox.Show($"Loaded recipe '{favorite.Name}' from favorites!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    protected override void OnClosed(EventArgs e)
    {
        _appStateService.PropertyChanged -= OnAppStateChanged;
        base.OnClosed(e);
    }
}