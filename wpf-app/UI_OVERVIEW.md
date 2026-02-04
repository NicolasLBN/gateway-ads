# WPF Application UI Overview

This document describes the user interface of the WPF Gateway ADS application.

## Application Layout

### Main Window
- **Title**: "Gateway ADS - WPF Application"
- **Size**: 1200x700 pixels
- **Layout**: Vertical stack with Header, Content, Footer

## Header Section
**Appearance**: Blue gradient background (#1a5490)

**Content**:
- **Left Side**:
  - Title: "Gateway ADS - WPF Application" (24pt, Bold, White)
  - Subtitle: "Industrial Recipe Management and PLC Control" (12pt, Light gray)
  
- **Right Side**:
  - Status Message: Dynamic text showing connection status (14pt, Bold, White)
  - Examples: "Not Connected", "Connected to Mixing Unit A", "Process Running"

## Main Content - Tab Control

### Tab 1: Machine Connection

#### Machine Selection Section (GroupBox)
- **Dropdown**: List of available machines
  - Mixing Unit A (127.0.0.1.1.1:851)
  - Mixing Unit B (127.0.0.1.1.1:852)
  - Mixing Unit C (127.0.0.1.1.1:853)
  
- **Buttons**:
  - "Connect" (Green button, 120px wide)
  - "Disconnect" (Red button, 120px wide)

#### Machine Status Section (GroupBox)
*Visible only when connected*

Three columns showing real-time data:

**Column 1: Motor Temperature**
- Label: "Motor Temperature" (Bold, 12pt)
- Value: Large number (24pt) + "°C" (18pt)
- Example: "28.5°C"
- Warning indicator if temperature too high

**Column 2: Oil Pressure**
- Label: "Oil Pressure" (Bold, 12pt)
- Value: Large number (24pt) + "bar" (18pt)
- Example: "3.25 bar"
- Warning indicator if pressure abnormal

**Column 3: Motor Speed**
- Label: "Motor Speed" (Bold, 12pt)
- Value: Large number (24pt) + "RPM" (18pt)
- Example: "1485 RPM"
- Warning indicator if speed out of range

#### Process Status Section (GroupBox)
*Visible only when connected*

**Current Step Display**:
- Label: "Current Step" (Bold, 12pt)
- Step Name: Large text (18pt) showing current process step
- Examples: "Idle", "Preparation", "Dosing Ingredient A", "Mixing"

**Progress Bar**:
- Label: "Overall Progress" (Bold, 12pt)
- Progress bar (30px height) showing 0-100%
- Percentage display below: "42%"

### Tab 2: Recipe Management

#### New Recipe Section (GroupBox)

**Recipe Form Fields**:
1. **Recipe Name**:
   - Label: "Recipe Name" (Bold)
   - Text input field (30px height)
   
2. **Preparation Volume**:
   - Label: "Preparation Volume (ml)" (Bold)
   - Number input field (30px height)
   
3. **Preparation Concentration**:
   - Label: "Preparation Concentration" (Bold)
   - Number input field (30px height)

#### Ingredients Section (GroupBox)
*Placeholder for future ingredient management*
- Message: "Ingredients management - Add ingredients functionality here"
- Centered, gray text

#### Actions Section (GroupBox)

**Three Buttons (Centered, Horizontal)**:
1. "Send Recipe to PLC" (Green, 150px wide)
2. "Start Process" (Blue, 150px wide)
3. "Reset Process" (Red, 150px wide)

### Tab 3: History

*Placeholder for future features*
- Message: "Recipe history and reports - Coming soon"
- Centered, gray text, 16pt

## Footer Section
**Appearance**: Light gray background (#f0f0f0), 1px border on top

**Content**:
- Text: "Gateway ADS WPF Application - Industrial Automation with Python Reports"
- Centered, gray color (#666666)

## Color Scheme

### Primary Colors
- **Header Blue**: #1a5490 (Dark blue)
- **Accent Blue**: #2a75bb (Medium blue)
- **Hover Blue**: #3d8fd1 (Light blue)

### Button Colors
- **Primary**: #2a75bb (Blue)
- **Success**: #28a745 (Green)
- **Danger**: #dc3545 (Red)

### Text Colors
- **Headers**: #2a75bb (Blue)
- **Normal Text**: #000000 (Black)
- **Disabled Text**: #666666 (Gray)
- **Footer Text**: #666666 (Gray)

### Background Colors
- **Window**: White
- **Header**: #1a5490 (Dark blue)
- **Footer**: #f0f0f0 (Light gray)
- **GroupBox Border**: #2a75bb (Blue)

## Typography

### Font Sizes
- **Main Title**: 24pt, Bold
- **Subtitle**: 12pt, Regular
- **Large Values**: 24pt, Regular
- **Unit Labels**: 18pt, Regular
- **Section Headers**: 16pt, Bold
- **Field Labels**: 12pt, Bold
- **Status Text**: 14pt, Bold
- **Normal Text**: 11-14pt, Regular

### Font Family
- Default WPF system font (Segoe UI on Windows 10/11)

## Interactive Elements

### Buttons
- **Style**: Rounded corners (4px radius)
- **Hover Effect**: Slightly lighter/brighter color
- **Disabled State**: Gray background, gray text
- **Cursor**: Hand cursor on hover

### Input Fields
- **Height**: 30-35px
- **Border**: 1px solid gray
- **Focus**: Blue outline
- **Font Size**: 14pt

### Dropdown
- **Height**: 35px
- **Border**: 1px solid gray
- **Font Size**: 14pt
- **Disabled**: Gray background when connected

### Progress Bar
- **Height**: 30px
- **Color**: Blue (#2a75bb)
- **Background**: Light gray
- **Animation**: Smooth fill

### GroupBox
- **Border**: 2px solid blue (#2a75bb)
- **Padding**: 10px
- **Margin**: 5px
- **Header**: Bold text

## Responsiveness

- Minimum window size: 800x600
- Maximum: No limit
- Tabs stack vertically on narrow screens
- GroupBoxes maintain padding and spacing

## Accessibility

- High contrast text
- Clear visual hierarchy
- Keyboard navigation supported
- Tab order follows logical flow
- Button states clearly visible

## Visual Feedback

### Connection States
- **Disconnected**: Gray status, Connect button enabled
- **Connecting**: Loading indicator (future enhancement)
- **Connected**: Green status, Disconnect button enabled, real-time data visible
- **Error**: Red status message

### Process States
- **Idle**: Step name "Idle", 0% progress
- **Running**: Current step name, progress bar moving, values updating
- **Complete**: Step name "Done", 100% progress
- **Error**: Error message displayed in red

## Icons and Indicators

### Warning Indicators
When machine parameters exceed thresholds:
- Temperature warning: Red indicator next to temperature
- Pressure warning: Red indicator next to pressure
- Speed warning: Red indicator next to speed

### Status Indicators
- Connected: Green indicator/text
- Disconnected: Gray indicator/text
- Error: Red indicator/text
- Processing: Blue indicator/text

## Layout Grid

### Main Window Grid
```
Row 0 (Auto): Header (Blue background)
Row 1 (*): TabControl (Main content, fills remaining space)
Row 2 (Auto): Footer (Gray background)
```

### Machine Connection Tab Grid
```
Row 0 (Auto): Machine Selection GroupBox
Row 1 (Auto): Machine Status GroupBox
Row 2 (*): Process Status GroupBox (fills remaining space)
```

### Recipe Management Tab Grid
```
Row 0 (Auto): Recipe Form GroupBox
Row 1 (*): Ingredients GroupBox (fills remaining space)
Row 2 (Auto): Actions GroupBox
```

## Future UI Enhancements

Planned additions:
- Ingredient list with add/remove functionality
- Real-time charts showing parameter trends
- Recipe history table with filters
- PDF report preview
- Machine configuration dialog
- Settings panel
- About dialog

## Professional Industrial Theme

The UI follows industrial application design principles:
- **Clean Layout**: Organized sections with clear boundaries
- **High Readability**: Large fonts, high contrast
- **Status Visibility**: Real-time updates clearly displayed
- **Action Clarity**: Buttons clearly labeled and colored
- **Professional Colors**: Blue theme suggesting reliability and precision
- **Functional Design**: Form follows function, no unnecessary decoration

---

This WPF application provides a professional, industrial-grade user interface suitable for manufacturing and process control environments.
