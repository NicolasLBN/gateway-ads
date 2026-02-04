using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.Views;

public partial class FavoritesWindow : Window
{
    private readonly FavoritesService _favoritesService;
    public FavoriteRecipe? SelectedFavorite { get; private set; }
    
    public FavoritesWindow(FavoritesService favoritesService)
    {
        InitializeComponent();
        _favoritesService = favoritesService;
        _favoritesService.OnFavoritesChanged += LoadFavorites;
        LoadFavorites();
    }

    private void LoadFavorites()
    {
        FavoritesPanel.Children.Clear();
        var favorites = _favoritesService.GetFavorites();
        
        if (favorites.Count == 0)
        {
            EmptyMessage.Visibility = Visibility.Visible;
            return;
        }
        
        EmptyMessage.Visibility = Visibility.Collapsed;
        
        foreach (var favorite in favorites)
        {
            var card = CreateFavoriteCard(favorite);
            FavoritesPanel.Children.Add(card);
        }
    }

    private Border CreateFavoriteCard(FavoriteRecipe favorite)
    {
        var card = new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xe9, 0xec, 0xef)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(0, 0, 0, 15),
            Padding = new Thickness(20)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Content column
        var contentStack = new StackPanel();
        
        var titleBlock = new TextBlock
        {
            Text = favorite.Name,
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        contentStack.Children.Add(titleBlock);

        var metaPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
        metaPanel.Children.Add(new TextBlock
        {
            Text = $"Volume: {favorite.PreparationVolume:F1} L  |  Concentration: {favorite.PreparationConcentration:F2} mol/L",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(0x49, 0x50, 0x57))
        });
        contentStack.Children.Add(metaPanel);

        var ingredientsBlock = new TextBlock
        {
            Text = $"Components: {string.Join(", ", favorite.Ingredients.Take(3).Select(i => i.Name))}" +
                   (favorite.Ingredients.Count > 3 ? $" +{favorite.Ingredients.Count - 3} more" : ""),
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(0x86, 0x8e, 0x96)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };
        contentStack.Children.Add(ingredientsBlock);

        Grid.SetColumn(contentStack, 0);
        grid.Children.Add(contentStack);

        // Buttons column
        var buttonsStack = new StackPanel { Orientation = Orientation.Horizontal };
        
        var useButton = new Button
        {
            Content = "▶ Use Recipe",
            Padding = new Thickness(15, 8, 15, 8),
            Margin = new Thickness(5, 0, 5, 0),
            Background = new SolidColorBrush(Color.FromRgb(0x51, 0xcf, 0x66))
        };
        useButton.Click += (s, e) =>
        {
            SelectedFavorite = favorite;
            DialogResult = true;
            Close();
        };
        buttonsStack.Children.Add(useButton);

        var deleteButton = new Button
        {
            Content = "🗑 Delete",
            Padding = new Thickness(15, 8, 15, 8),
            Margin = new Thickness(5, 0, 0, 0),
            Background = new SolidColorBrush(Color.FromRgb(0xfa, 0x52, 0x52))
        };
        deleteButton.Click += (s, e) =>
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete '{favorite.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _favoritesService.DeleteFavorite(favorite.Id);
            }
        };
        buttonsStack.Children.Add(deleteButton);

        Grid.SetColumn(buttonsStack, 1);
        grid.Children.Add(buttonsStack);

        card.Child = grid;
        return card;
    }

    private void AddFavorite_Click(object sender, RoutedEventArgs e)
    {
        var editWindow = new FavoriteEditWindow(_favoritesService);
        editWindow.Owner = this;
        editWindow.ShowDialog();
    }

    protected override void OnClosed(EventArgs e)
    {
        _favoritesService.OnFavoritesChanged -= LoadFavorites;
        base.OnClosed(e);
    }
}
