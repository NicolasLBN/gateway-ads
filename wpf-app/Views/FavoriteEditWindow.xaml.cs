using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.Views;

public partial class FavoriteEditWindow : Window
{
    private readonly FavoritesService _favoritesService;
    private readonly List<IngredientControl> _ingredientControls = new();
    
    public FavoriteEditWindow(FavoritesService favoritesService)
    {
        InitializeComponent();
        _favoritesService = favoritesService;
        
        // Add initial ingredient
        AddIngredientControl();
    }

    private void AddIngredient_Click(object sender, RoutedEventArgs e)
    {
        AddIngredientControl();
    }

    private void AddIngredientControl()
    {
        var control = new IngredientControl(_ingredientControls.Count + 1);
        control.OnRemove += () =>
        {
            IngredientsPanel.Children.Remove(control);
            _ingredientControls.Remove(control);
            RenumberIngredients();
        };
        
        _ingredientControls.Add(control);
        IngredientsPanel.Children.Add(control);
    }

    private void RenumberIngredients()
    {
        for (int i = 0; i < _ingredientControls.Count; i++)
        {
            _ingredientControls[i].UpdateNumber(i + 1);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            MessageBox.Show("Please enter a recipe name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!double.TryParse(VolumeTextBox.Text, out var volume) || volume <= 0)
        {
            MessageBox.Show("Please enter a valid volume.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!double.TryParse(ConcentrationTextBox.Text, out var concentration) || concentration <= 0)
        {
            MessageBox.Show("Please enter a valid concentration.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var ingredients = new List<Ingredient>();
        foreach (var control in _ingredientControls)
        {
            var ingredient = control.GetIngredient();
            if (ingredient != null)
            {
                ingredients.Add(ingredient);
            }
        }

        if (ingredients.Count == 0)
        {
            MessageBox.Show("Please add at least one component.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var favorite = new FavoriteRecipe
        {
            Name = NameTextBox.Text,
            PreparationVolume = volume,
            PreparationConcentration = concentration,
            Ingredients = ingredients
        };

        if (_favoritesService.AddFavorite(favorite))
        {
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("Failed to save favorite.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

public class IngredientControl : Border
{
    private const double DEFAULT_VOLUME_ML = 100.0;
    private const double DEFAULT_MOLAR_MASS_G_MOL = 100.0;
    
    private readonly TextBox _nameTextBox;
    private readonly TextBox _volumeTextBox;
    private readonly TextBox _molarMassTextBox;
    private readonly TextBlock _numberLabel;
    
    public event Action? OnRemove;

    public IngredientControl(int number)
    {
        Background = new SolidColorBrush(Color.FromRgb(0xf8, 0xf9, 0xfa));
        BorderBrush = new SolidColorBrush(Color.FromRgb(0xe9, 0xec, 0xef));
        BorderThickness = new Thickness(2);
        CornerRadius = new CornerRadius(6);
        Padding = new Thickness(15);
        Margin = new Thickness(0, 0, 0, 10);

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Header
        var headerPanel = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
        
        _numberLabel = new TextBlock
        {
            Text = $"Component {number}",
            FontWeight = FontWeights.SemiBold,
            FontSize = 14
        };
        DockPanel.SetDock(_numberLabel, Dock.Left);
        headerPanel.Children.Add(_numberLabel);

        var removeButton = new Button
        {
            Content = "🗑",
            Padding = new Thickness(8, 4, 8, 4),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = new SolidColorBrush(Color.FromRgb(0xfa, 0x52, 0x52)),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        removeButton.Click += (s, e) => OnRemove?.Invoke();
        DockPanel.SetDock(removeButton, Dock.Right);
        headerPanel.Children.Add(removeButton);

        Grid.SetRow(headerPanel, 0);
        grid.Children.Add(headerPanel);

        // Name field
        var nameStack = new StackPanel { Margin = new Thickness(0, 0, 0, 8) };
        nameStack.Children.Add(new TextBlock { Text = "Chemical Name", FontWeight = FontWeights.SemiBold, FontSize = 12, Margin = new Thickness(0, 0, 0, 4) });
        _nameTextBox = new TextBox { Height = 28, Padding = new Thickness(6), FontSize = 13 };
        nameStack.Children.Add(_nameTextBox);
        Grid.SetRow(nameStack, 1);
        grid.Children.Add(nameStack);

        // Fields grid
        var fieldsGrid = new Grid();
        fieldsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        fieldsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
        fieldsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var volumeStack = new StackPanel();
        volumeStack.Children.Add(new TextBlock { Text = "Volume (mL)", FontWeight = FontWeights.SemiBold, FontSize = 12, Margin = new Thickness(0, 0, 0, 4) });
        _volumeTextBox = new TextBox { Height = 28, Padding = new Thickness(6), FontSize = 13, Text = DEFAULT_VOLUME_ML.ToString() };
        volumeStack.Children.Add(_volumeTextBox);
        Grid.SetColumn(volumeStack, 0);
        fieldsGrid.Children.Add(volumeStack);

        var molarMassStack = new StackPanel();
        molarMassStack.Children.Add(new TextBlock { Text = "Molar Mass (g/mol)", FontWeight = FontWeights.SemiBold, FontSize = 12, Margin = new Thickness(0, 0, 0, 4) });
        _molarMassTextBox = new TextBox { Height = 28, Padding = new Thickness(6), FontSize = 13, Text = DEFAULT_MOLAR_MASS_G_MOL.ToString() };
        molarMassStack.Children.Add(_molarMassTextBox);
        Grid.SetColumn(molarMassStack, 2);
        fieldsGrid.Children.Add(molarMassStack);

        Grid.SetRow(fieldsGrid, 2);
        grid.Children.Add(fieldsGrid);

        Child = grid;
    }

    public void UpdateNumber(int number)
    {
        _numberLabel.Text = $"Component {number}";
    }

    public Ingredient? GetIngredient()
    {
        if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            return null;

        if (!double.TryParse(_volumeTextBox.Text, out var volume))
            volume = 0;

        if (!double.TryParse(_molarMassTextBox.Text, out var molarMass))
            molarMass = 0;

        return new Ingredient
        {
            Name = _nameTextBox.Text,
            Volume = volume,
            MolarMass = molarMass
        };
    }
}
