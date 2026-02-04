# WPF Application - New Features Documentation

## Overview

The WPF application has been enhanced to match the features and styling of the Blazor application. The following major features have been added:

### 1. Favorites Management System
- Create, view, and manage favorite recipes
- Pre-existing favorites included (3 default recipes)
- Quick-load recipes from favorites
- Save current recipes as favorites

### 2. Real-time Data Visualization
- Live charts for PLC data when connected
- Three separate charts for Temperature, Pressure, and Speed
- Automatic data updates (up to 50 data points)
- Smooth line charts using LiveCharts2

## Features in Detail

### Favorites Management

#### Pre-existing Favorites
The application comes with 3 pre-configured favorite recipes:

1. **Standard Buffer Solution**
   - Volume: 1.0 L
   - Concentration: 0.1 mol/L
   - Components: Sodium Phosphate Dibasic, Sodium Phosphate Monobasic

2. **Saline Solution**
   - Volume: 1.0 L
   - Concentration: 0.9 mol/L
   - Component: Sodium Chloride (NaCl)

3. **Tris-HCl Buffer**
   - Volume: 0.5 L
   - Concentration: 0.05 mol/L
   - Components: Tris Base, Hydrochloric Acid

#### How to Use Favorites

**Viewing Favorites:**
1. Navigate to the "Recipe Management" tab
2. Click "Manage Favorites" button
3. A window will open showing all saved favorites

**Loading a Favorite Recipe:**
1. Click "Load from Favorites" in Recipe Management tab, OR
2. Click "Manage Favorites", select a favorite, and click "Use Recipe"
3. The recipe will be loaded into the current recipe form

**Saving a Recipe as Favorite:**
1. Fill in the recipe details (name, volume, concentration, ingredients)
2. Click "Save as Favorite" button
3. The recipe will be saved to the favorites database

**Adding New Favorites:**
1. Click "Manage Favorites" to open the Favorites window
2. Click "➕ Add Favorite" button
3. Fill in the recipe details:
   - Recipe name
   - Volume (L)
   - Concentration (mol/L)
   - Add components with name, volume (mL), and molar mass (g/mol)
4. Click "Save"

**Deleting Favorites:**
1. In the Favorites window, find the favorite to delete
2. Click the "🗑 Delete" button
3. Confirm the deletion when prompted

#### Data Persistence
- Favorites are stored in a LiteDB database
- Database location: `%LocalAppData%\WpfGatewayADS\favorites.db`
- Data persists across application restarts

### Real-time Charts

#### Chart Types
Three charts are displayed when connected to a PLC:

1. **Temperature Chart**
   - Displays motor temperature in °C
   - Y-axis range: 0-100°C
   - Color: Red line

2. **Pressure Chart**
   - Displays oil pressure in bar
   - Y-axis range: 0-10 bar
   - Color: Blue line

3. **Speed Chart**
   - Displays motor speed in RPM
   - Y-axis range: 0-2500 RPM
   - Color: Green line

#### Chart Features
- **Real-time Updates**: Charts update automatically as PLC data is received
- **Data Buffering**: Maintains the last 50 data points
- **Smooth Rendering**: Uses LiveCharts2 for smooth, professional visualization
- **Auto-scaling**: Y-axis scales are pre-configured for optimal viewing

#### Viewing Charts
1. Navigate to the "Machine Connection" tab
2. Select a machine and click "Connect"
3. Once connected, the charts appear in the "Process Status" section
4. Charts update automatically with live PLC data

## Technical Implementation

### Dependencies Added
- **LiteDB 5.0.17**: Database for storing favorites
- **LiveChartsCore.SkiaSharpView.WPF 2.0.0-rc2**: Charting library

### New Services
- **FavoritesService**: Manages CRUD operations for favorite recipes

### New Windows/Views
- **FavoritesWindow**: Main window for viewing and managing favorites
- **FavoriteEditWindow**: Dialog for adding new favorites
- **IngredientControl**: Reusable control for ingredient input

### Architecture
```
wpf-app/
├── Services/
│   └── FavoritesService.cs       # Favorites management logic
├── Views/
│   ├── FavoritesWindow.xaml      # Favorites list UI
│   ├── FavoritesWindow.xaml.cs   # Favorites list logic
│   ├── FavoriteEditWindow.xaml   # Add/Edit favorite UI
│   └── FavoriteEditWindow.xaml.cs # Add/Edit favorite logic
└── MainWindow.xaml.cs             # Updated with charts and favorites integration
```

## UI Styling

The WPF application now uses the same color scheme as the Blazor app:

### Button Colors
- **Primary**: #228be6 (Blue) - Standard actions
- **Success**: #51cf66 (Green) - Positive actions (send, start)
- **Danger**: #fa5252 (Red) - Destructive actions (delete, reset)
- **Warning**: #f59f00 (Orange) - Load from favorites
- **Info**: #20c997 (Teal) - Save as favorite

### Card Styling
- White background with subtle shadows
- Rounded corners (8px border radius)
- Light gray borders (#e9ecef)
- Hover effects for interactive elements

## Comparison with Blazor App

| Feature | WPF | Blazor |
|---------|-----|--------|
| Favorites Management | ✅ | ✅ |
| Pre-existing Favorites | ✅ | ✅ |
| Real-time Charts | ✅ | ✅ |
| Chart Types | 3 (Temp, Pressure, Speed) | 3 (Same) |
| Data Persistence | LiteDB | LiteDB |
| UI Styling | Matched | Original |

## Usage Examples

### Example: Loading a Pre-existing Favorite
```
1. Open WPF Application
2. Go to "Recipe Management" tab
3. Click "Load from Favorites"
4. Select "Standard Buffer Solution"
5. Click "Use Recipe"
6. Recipe details are now loaded:
   - Name: Standard Buffer Solution
   - Volume: 1.0 L
   - Concentration: 0.1 mol/L
   - Components loaded
```

### Example: Creating and Saving a Custom Favorite
```
1. In "Recipe Management" tab, enter:
   - Recipe Name: "Custom pH Buffer"
   - Volume: 2.0 L
   - Concentration: 0.15 mol/L
2. Click "Save as Favorite"
3. Success message appears
4. Recipe is now available in favorites
```

### Example: Monitoring PLC with Charts
```
1. Go to "Machine Connection" tab
2. Select "Production Line 1"
3. Click "Connect"
4. Wait for connection to establish
5. Charts appear showing:
   - Temperature rising from ambient
   - Pressure stabilizing
   - Motor speed ramping up
6. Charts update every 500ms with new data
```

## Troubleshooting

### Favorites Not Saving
- Check write permissions to `%LocalAppData%\WpfGatewayADS\`
- Ensure LiteDB.dll is present in the application directory

### Charts Not Updating
- Verify PLC connection is established
- Check that PlcPollingService is running
- Ensure MachineStatus data is being received

### UI Elements Not Visible
- Ensure you're on the correct tab
- For charts: Must be connected to a machine
- For favorites: Check that FavoritesService is registered in DI

## Future Enhancements

Potential improvements for future versions:
- Edit existing favorites
- Export/Import favorites to JSON
- Chart zoom and pan controls
- Historical data playback
- Custom chart time ranges
- More chart types (histogram, scatter)
- Favorite categories/tags
- Search and filter favorites
