# WPF Features Test Plan

## Favorites Functionality
1. Launch the application
2. Go to Recipe Management tab
3. Click "Manage Favorites" - should open Favorites window
4. Verify 3 pre-existing favorites are displayed:
   - Standard Buffer Solution
   - Saline Solution  
   - Tris-HCl Buffer
5. Click "Use Recipe" on one favorite - should load it into the main form
6. Enter a new recipe in the main form
7. Click "Save as Favorite" - should save successfully
8. Click "Add Favorite" in Favorites window - should open edit dialog
9. Add a new favorite with components
10. Verify it appears in the favorites list
11. Delete a favorite - should show confirmation and remove it

## Real-time Charts
1. Launch the application
2. Go to Machine Connection tab
3. Select a machine and connect
4. Verify "Real-time Machine Data" section appears
5. Check three charts are displayed:
   - Temperature (°C)
   - Pressure (bar)
   - Speed (RPM)
6. When connected to PLC, charts should update in real-time
7. Verify data points accumulate (max 50 points)
8. Charts should show smooth line graphs

## UI Styling
1. Verify buttons use Blazor-like colors:
   - Primary buttons: Blue (#228be6)
   - Success buttons: Green (#51cf66)
   - Danger buttons: Red (#fa5252)
   - Warning buttons: Orange (#f59f00)
2. Check card-based layout for favorites
3. Verify modern rounded corners and shadows
