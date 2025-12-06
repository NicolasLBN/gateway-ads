# TwinCAT 3 PLC Simulator

## Description

This directory contains the TwinCAT 3 PLC program that simulates an industrial mixing/dosing system.

## Structure

```
/plc-simulator
  /GVLs          - Global Variable Lists
    GVL_Recipe.TcGVL      - Recipe data (ingredients)
    GVL_Process.TcGVL     - Process state and progress
    GVL_Machine.TcGVL     - Machine sensors data
    GVL_Command.TcGVL     - Commands from external system
  /POUs          - Program Organization Units
    MAIN.TcPOU                   - Main program
    FB_ProcessStateMachine.TcPOU - Process state machine
    FB_MachineSimulation.TcPOU   - Machine simulation
  /Tasks
    PlcTask.TcTTO         - Task configuration (10ms cycle)
```

## How to Import in TwinCAT 3

1. Open TwinCAT 3 XAE (Visual Studio with TwinCAT extension)
2. Create a new TwinCAT PLC Project
3. In Solution Explorer, right-click on the PLC Project → Add → Existing Item
4. Navigate to each directory and import the files:
   - Import all GVL files into GVLs folder
   - Import all POU files into POUs folder
   - Import task configuration

5. Build the project (F7)
6. Activate configuration and run

## Process Steps

The simulator implements a 7-step process:

1. **Idle** (Step 0) - Waiting for start command
2. **Preparation** (Step 1) - 5 seconds
3. **Dosing Ingredient A** (Step 2) - 7 seconds
4. **Dosing Ingredient B** (Step 3) - 7 seconds
5. **Mixing** (Step 4) - 10 seconds
6. **Verification** (Step 5) - 4 seconds
7. **Finalizing** (Step 6) - 3 seconds
8. **Done** (Step 7) - Complete

## Variables for ADS Communication

### Read Variables (Machine Status)
- `GVL_Machine.MotorTemperature` (REAL) - Motor temperature in °C
- `GVL_Machine.OilPressure` (REAL) - Oil pressure in bar
- `GVL_Machine.MotorSpeed` (REAL) - Motor speed in RPM
- `GVL_Machine.TempWarning` (BOOL) - Temperature warning flag
- `GVL_Machine.PressureWarning` (BOOL) - Pressure warning flag
- `GVL_Machine.SpeedWarning` (BOOL) - Speed warning flag

### Read Variables (Process Status)
- `GVL_Process.CurrentStep` (INT) - Current step number (0-7)
- `GVL_Process.StepName` (STRING) - Current step name
- `GVL_Process.Progress` (REAL) - Overall progress (0.0-1.0)
- `GVL_Process.StepProgress` (REAL) - Current step progress (0.0-1.0)
- `GVL_Process.StepTime_s` (UDINT) - Time in current step (seconds)
- `GVL_Process.TotalTime_s` (UDINT) - Total process time (seconds)
- `GVL_Process.ErrorCode` (INT) - Error code (0 = no error)
- `GVL_Process.ErrorText` (STRING) - Error description
- `GVL_Process.ProcessDone` (BOOL) - Process completed flag

### Write Variables (Recipe)
- `GVL_Recipe.RecipeName` (STRING) - Name of the recipe
- `GVL_Recipe.NumIngredients` (UDINT) - Number of ingredients
- `GVL_Recipe.IngredientName[1..10]` (ARRAY OF STRING) - Ingredient names
- `GVL_Recipe.IngredientQuantity[1..10]` (ARRAY OF REAL) - Quantities in grams
- `GVL_Recipe.IngredientVolume[1..10]` (ARRAY OF REAL) - Volumes in ml
- `GVL_Recipe.IngredientMolarMass[1..10]` (ARRAY OF REAL) - Molar mass in g/L

### Write Variables (Commands)
- `GVL_Command.StartProcess` (BOOL) - Start the process
- `GVL_Command.ResetProcess` (BOOL) - Reset the process
- `GVL_Command.AdsConnected` (BOOL) - ADS connection status

## Simulation Features

- Temperature gradually increases during process and cools down when idle
- Oil pressure varies based on motor speed
- Motor speed runs at ~1500 RPM during active steps
- Random anomalies can occur (1% chance per cycle)
- Automatic warning flags for out-of-range values

## ADS Configuration

Default configuration:
- **AMS Net ID**: 127.0.0.1.1.1 (localhost)
- **AMS Port**: 851 (PlcTask port)

To change these values, modify the Node.js backend configuration.
