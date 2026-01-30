# Neural Network Implementation Guide

## Overview
This implementation allows Unity to call the neural network executable (`predict.exe`), similar to how the C++ radiation calculation module works.

## Files Created

1. **NeuralNetworkRunner.cs** - C# script that handles UI and calls the predict.exe executable
2. **predict.exe** - Neural network executable (located in StreamingAssets/NeuralNetwork/)
3. **radiation_NN_model.pth** - Trained neural network model
4. **scaler_d.pkl** - Scaler for d (debljina štita)
5. **scaler_Z.pkl** - Scaler for Z (redni broj elementa)
6. **SceneManager.cs** - Updated with `LoadNNPredikcija()` method

## Input Parameters

The neural network requires 4 input parameters:
1. **r/c** - String: "r" (rub) or "c" (centar)
2. **Z** - Float: Redni broj elementa (atomic number)
3. **d** - Float: Debljina štita (shield thickness)
4. **E** - Float: Energija gama zrake (gamma ray energy)

## Setup Instructions

### 1. Create the "NN Predikcija" Scene

In Unity Editor:
1. Open the "RayPlaneIntersection" scene as a reference
2. Create a new scene: File → New Scene → Basic (Built-in)
3. Save it as "NN Predikcija" in `Assets/Scenes/`

### 2. Set Up the UI Canvas

Copy the UI structure from RayPlaneIntersection scene:

1. **Create Canvas:**
   - Right-click in Hierarchy → UI → Canvas
   - Name it "NNCanvas"
   - Set Canvas Scaler: UI Scale Mode = Scale With Screen Size, Reference Resolution = 1920x1080

2. **Create Panel:**
   - Right-click on Canvas → UI → Panel
   - Name it "Panel"
   - Set size: 1040x600, centered

3. **Create UI Elements:**
   - **Title Text** (TMP_Text): Name = "NNTitle", Text = "NN Predikcija"
   
   - **Input Fields** (TMP_InputField):
     - **RCInput**: Name = "RCInput", Label = "r/c (rub ili centar)", Content Type = Standard, Character Limit = 1
     - **ZInput**: Name = "ZInput", Label = "Z (redni broj elementa)", Content Type = Decimal Number
     - **DInput**: Name = "DInput", Label = "d (debljina štita)", Content Type = Decimal Number
     - **EInput**: Name = "EInput", Label = "E (energija gama zrake)", Content Type = Decimal Number
   
   - **Run Button** (Button): Name = "RunButton", Text = "Run"
   - **Back Button** (Button): Name = "BackButton", Text = "Back"
   - **Status Text** (TMP_Text): Name = "StatusText", Text = "Status: Ready"
   - **Output Text** (TMP_Text): Name = "OutputText", Text = ""
     - Set as Scrollable (add ScrollRect if needed)

4. **Add NeuralNetworkRunner Script:**
   - Create empty GameObject, name it "NeuralNetworkRunner"
   - Add component: NeuralNetworkRunner
   - In Inspector, assign all UI elements to the script fields:
     - Title Text → NNTitle
     - RC Input → RCInput
     - Z Input → ZInput
     - D Input → DInput
     - E Input → EInput
     - Run Button → RunButton
     - Back Button → BackButton
     - Status Text → StatusText
     - Output Text → OutputText

5. **Add EventSystem:**
   - Right-click in Hierarchy → UI → Event System
   - (Unity usually creates this automatically)

### 3. Add Scene to Build Settings

1. File → Build Settings
2. Click "Add Open Scenes" (or drag "NN Predikcija.unity" into the list)
3. Ensure it's checked/enabled

### 4. File Structure

Ensure the following files are in `Assets/StreamingAssets/NeuralNetwork/`:
- `predict.exe` - The neural network executable
- `radiation_NN_model.pth` - Trained model file
- `scaler_d.pkl` - Scaler for d parameter
- `scaler_Z.pkl` - Scaler for Z parameter

The executable expects these files to be in the same directory (or parent directory) when running.

### 5. Testing

1. Open the "NN Predikcija" scene in Unity
2. Enter values:
   - r/c: "r" or "c"
   - Z: A number (e.g., 82 for Lead)
   - d: A number (e.g., 1.5)
   - E: A number (e.g., 0.662)
3. Click "Run"
4. Check the output text area for the prediction result

## How It Works

1. User enters 4 input values in the UI fields
2. Unity validates the inputs (r/c must be "r" or "c", others must be valid numbers)
3. Unity calls `predict.exe` with arguments: `r/c Z d E`
4. The executable processes the inputs using the neural network model
5. Output from the executable (stdout) is captured and displayed in the OutputText area

## Example Call

From Unity, the executable is called like:
```
predict.exe r 82 1.5 0.662
```

Where:
- `r` = rub
- `82` = Z (Lead)
- `1.5` = d (debljina štita)
- `0.662` = E (energija gama zrake)

## Notes

- The executable is expected in `StreamingAssets/NeuralNetwork/predict.exe`
- Model files (`.pth` and `.pkl`) should be in the same directory
- The executable prints results to stdout, which Unity captures and displays
- Errors are printed to stderr and also displayed in the output area
- The script automatically finds input fields by name if not assigned in Inspector
