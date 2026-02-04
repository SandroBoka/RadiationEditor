# Radiation Geometry Editor

Radiation Geometry Editor is a Unity-based toolset for building and exporting simple 3D geometry for radiation simulation workflows made for . It now includes a main 3D editor plus additional utility scenes (prediction runner, ray-plane intersection runner, and a PDF opener) available from the menu.

This toolset is designed to help simulation and research teams define:
- simple 3D bodies
- materials
- exact spatial transformations
- sensor locations

and export all relevant data into CSV for downstream processing.

Note: The 3D editor does not perform physics or simulation. External executables in `Assets/StreamingAssets` handle computation for the utility scenes.

---

<img width="1512" height="982" alt="Screenshot 2026-01-11 at 21 59 45" src="https://github.com/user-attachments/assets/7b21b9c7-f9f5-45cf-917f-c01650512f61" />

## Getting Started

- Open the Unity project at `RadiationEditor/`.
- Create a folder `Predict` inisde `RadiationEditor/Assets/StreamingAssets`
- Download this ziped file ([macOS link](https://ferhr-my.sharepoint.com/:u:/g/personal/sb53891_fer_hr/IQCyBaUXU3WKTLZSiIYTM-tqAS7yaA8hAqyu2oe6CMVanpA?e=1MD8iC) , [Windows link](https://ferhr-my.sharepoint.com/:u:/g/personal/sb53891_fer_hr/IQA2A8qyDK0iQJ2-jVQH5OBsASiDCmTtHGAp2exu7MGOdhY?e=NMWq7K) ), unzip it and place the contents inside of it in `Predict` folder

### Supported Platforms
- macOS
- Windows

---

## Scenes Overview

- `Menu`: Entry scene with buttons to open the editor, utilities, and PDF.
- `3D Editor`: Main geometry editor for creating shapes, editing transforms, and exporting/importing data.
- `NNPredict`: UI that runs an external prediction executable and shows its output.
- `RayPlaneIntersection`: UI that runs an external executable on a CSV input and shows its output.

---

## 3D Editor

### Camera Controls

| Action | Control |
|------|--------|
| Move forward / backward | `W` / `S` |
| Move left / right | `A` / `D` |
| Move up | `E` |
| Move down | `Q` |
| Rotate camera | Right Mouse Button + Mouse Move |
| Faster movement | `Left Shift` |

### Creating Geometry

The following object types can be added:
- Cube
- Sphere
- Cylinder
- Sensor (treated as a small sphere)

Objects:
- can overlap freely
- can be placed inside other objects
- are not restricted by physics or collisions

### Object Selection

- Left-click an object to select it
- When selected, a transform gizmo appears and the Selection Panel becomes visible
- Left-click in empty space to deselect

### Transform Gizmo (Move Tool)

When an object is selected:
- a 3-axis gizmo (X / Y / Z) appears
- handles are positioned at the object's outer bounds
- dragging a handle moves the object along that axis only

The gizmo remains active while the object is selected and does not interfere with selection.

### Manual Transform Editing (HUD)

When an object is selected, the HUD displays editable numeric fields.

Position (X / Y / Z):
- world-space position
- updates live when using the gizmo
- can be edited numerically

Scale (X / Y / Z):
- non-uniform scaling supported

Rotation (X / Y / Z):
- Euler angles (degrees)
- applied in world space

### Materials

- Each object can be assigned a material from a predefined list
- Current default list: Air, Lead, Concrete, Radioactive, Sensor
- Material changes are applied immediately
- Material names are exported to CSV

### Deleting Objects

- When an object is selected, a Delete button is shown
- Clicking Delete removes the object, clears selection, and hides the gizmo and panel

---

## Import and Export

### CSV Export

- Click Export CSV
- A CSV file is generated with a fixed header and one row per shape
- A save dialog opens (defaulting to the Desktop)

Example filename:
`radiation_shapes_20260111_154233.csv`

CSV header:
```csv
id,type,material,px,py,pz,sx,sy,sz,rx,ry,rz,radius,radiusX,radiusZ,height
```

### CSV Import

- Click Import CSV
- The importer validates the header and ignores invalid rows
- Optional `clearExisting` can wipe current shapes first

### QAD INP Import

- Click Import QAD
- Supported record types: `SPH` -> Sphere, `RPP` -> Cube, `RCC` -> Cylinder
- Imported shapes default to material `Concrete`
- Negative scale values are normalized into positive scale with adjusted position

---

## Utility Scenes

### NNPredict

- Runs an external executable from `Assets/StreamingAssets/Predict`
- Uses inputs: mode (`c` or `r`) and three float values
- Output is displayed in the UI (stdout or stderr)

Expected binaries:
- macOS: `Assets/StreamingAssets/Predict/Predict`
- Windows: `Assets/StreamingAssets/Predict/predict.exe`

### Ray-Plane Intersection

- Runs an external executable from `Assets/StreamingAssets/RayPlaneIntersection`
- Requires a CSV file and number of points
- Output is displayed in the UI

Expected binaries:
- macOS: `Assets/StreamingAssets/RayPlaneIntersection/appMac`
- Windows: `Assets/StreamingAssets/RayPlaneIntersection/app.exe`

Windows note:
- The runner checks for `libgcc_s_seh-1.dll`, `libstdc++-6.dll`, and `libwinpthread-1.dll` next to `app.exe`.

---

## PDF Opener

The menu includes a button that opens a PDF using the system default viewer.

Default PDF location:
- `Assets/StreamingAssets/data/pdf/Primjena_plazme_u_industriji_procisavanja_otpadnih_voda.pdf`

If the PDF is missing, a warning is logged.

---

# Script Guide

### `Assets/Scripts/Core`

- `SelectionManager` (`RadiationEditor/Assets/Scripts/Core/SelectionManager.cs`): Handles click selection using raycasts, ignores gizmo hits, and updates `TransformGizmo` and `TransformHud` targets using `gizmoLayer` and `selectableLayers`.
- `ShapeManager` (`RadiationEditor/Assets/Scripts/Core/ShapeManager.cs`): Singleton list owner that creates primitives, assigns the `Shapes` layer, and attaches `ShapeData` with material info.
- `SceneManager` (`RadiationEditor/Assets/Scripts/Core/SceneManager.cs`): Wrapper around Unity SceneManager that validates Build Settings entries before loading.

### `Assets/Scripts/Data`

- `ShapeType` (`RadiationEditor/Assets/Scripts/Data/ShapeType.cs`): Enum for Cube, Sphere, Cylinder, Sensor.
- `ShapeData` (`RadiationEditor/Assets/Scripts/Data/ShapeData.cs`): Stores shape type and material name, computes derived values used in CSV export.
- `MaterialLibrary` (`RadiationEditor/Assets/Scripts/Data/MaterialLibrary.cs`): ScriptableObject listing materials used by the HUD dropdown, asset at `RadiationEditor/Assets/ScriptableObjects/MaterialLibrary.asset`.

### `Assets/Scripts/Camera`

- `EditorFlyCamera` (`RadiationEditor/Assets/Scripts/Camera/EditorFlyCamera.cs`): Free-fly camera using the old Input system (WASD + QE, RMB look, Left Shift boost).

### `Assets/Scripts/Gizmo`

- `TransformGizmo` (`RadiationEditor/Assets/Scripts/Gizmo/TransformGizmo.cs`): Shows and positions the gizmo, raycasts against the `Gizmo` layer, and drags along one axis using a camera-aligned plane.
- `GizmoHandle` (`RadiationEditor/Assets/Scripts/Gizmo/GizmoHandle.cs`): Builds arrow meshes per axis with X/Y/Z colors.
- `GizmoAxis` (`RadiationEditor/Assets/Scripts/Gizmo/GizmoAxis.cs`): Enum X, Y, Z.

### `Assets/Scripts/UI`

- `TransformHud` (`RadiationEditor/Assets/Scripts/UI/TransformHud.cs`): Drives input fields and dropdown, applies transform/material edits, and deletes the selected object.
- `HudSpawner` (`RadiationEditor/Assets/Scripts/UI/HudSpawner.cs`): Spawns shapes from HUD buttons with the currently selected material.
- `SceneButton` (`RadiationEditor/Assets/Scripts/UI/SceneButton.cs`): UI button helper for loading scenes by name.
- `OpenPdfButton` (`RadiationEditor/Assets/Scripts/UI/OpenPdfButton.cs`): Opens the configured PDF with the system default application.
- `PdfOpener` (`RadiationEditor/Assets/Scripts/UI/PdfOpener.cs`): Same behavior as `OpenPdfButton` for other UI contexts.

### `Assets/Scripts/Export`

- `CsvExporter` (`RadiationEditor/Assets/Scripts/Export/CsvExporter.cs`): Exports all shapes to CSV with a fixed header using a save dialog and UTF-8 output.

### `Assets/Scripts/Import`

- `CsvImporter` (`RadiationEditor/Assets/Scripts/Import/CsvImporter.cs`): Opens a CSV, validates the header and numeric values, and recreates shapes (optionally clearing existing shapes).
- `QadImporter` (`RadiationEditor/Assets/Scripts/Import/QadImporter.cs`): Imports QAD INP SPH/RPP/RCC rows into basic shapes with normalized scale.

### `Assets/Scripts/NNPredict`

- `NNPredictRunner` (`RadiationEditor/Assets/Scripts/NNPredict/NNPredictRunner.cs`): Runs the prediction executable and displays output.

### `Assets/Scripts/RayPlaneIntersection`

- `RayPlaneIntersectionRunner` (`RadiationEditor/Assets/Scripts/RayPlaneIntersection/RayPlaneIntersectionRunner.cs`): Runs the ray-plane intersection executable and displays output.

---

## Runtime Flow (3D Editor)

- Spawn: `HudSpawner` calls `ShapeManager.CreateShape`.
- Select: `SelectionManager` raycasts on left click and assigns target to `TransformGizmo` and `TransformHud`.
- Move: `TransformGizmo` drags along one axis using a camera-aligned plane.
- Edit: `TransformHud` writes to transforms and updates derived fields.
- Export/Import: `CsvExporter` writes CSV, `CsvImporter` and `QadImporter` recreate shapes.
