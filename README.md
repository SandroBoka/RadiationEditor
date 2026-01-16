# Radiation Geometry Editor

Radiation Geometry Editor is a lightweight 3D tool for creating and exporting geometric scenes used in **radiation simulation workflows**.

The editor is designed to help simulation and research teams define:
- simple 3D bodies,
- materials,
- exact spatial transformations,
- and sensor locations,

and export all relevant data into a **CSV file** for further processing.

> **Note**  
> This tool does **not** perform calculations.  

---

<img width="1512" height="982" alt="Screenshot 2026-01-11 at 21 59 45" src="https://github.com/user-attachments/assets/7b21b9c7-f9f5-45cf-917f-c01650512f61" />

## Getting Started

The editor will be distributed as part of a **standalone desktop application**.

### Supported Platforms
- macOS
- Windows
- Linux
---

## Camera Controls

The editor uses Unity-style free-fly camera controls:

| Action | Control |
|------|--------|
| Move forward / backward | `W` / `S` |
| Move left / right | `A` / `D` |
| Move up | `E` |
| Move down | `Q` |
| Rotate camera | Right Mouse Button + Mouse Move |
| Faster movement | `Left Shift` |

---

## Creating Geometry

The following object types can be added:

- Cube
- Sphere
- Cylinder
- Sensor (treated as a small sphere)

Objects:
- can overlap freely,
- can be placed inside other objects,
- are not restricted by physics or collisions.

---

## Object Selection

- Left-click an object to select it
- When selected:
  - a transform gizmo appears,
  - the Selection Panel becomes visible
- Left-click in empty space to deselect

---

## Transform Gizmo (Move Tool)

When an object is selected:
- a 3-axis gizmo (X / Y / Z) appears,
- handles are positioned at the object’s outer bounds,
- dragging a handle moves the object along that axis only.

The gizmo remains active while the object is selected and does not interfere with selection.

---

## Manual Transform Editing (HUD)

When an object is selected, the HUD displays editable numeric fields.

### Position (X / Y / Z)
- World-space position
- Updates live when using the gizmo
- Can be edited numerically

### Scale (X / Y / Z)
- Non-uniform scaling supported

### Rotation (X / Y / Z)
- Euler angles (degrees)
- Applied in world space

---

## Materials

- Each object can be assigned a material from a predefined list
- Currently that is Air, Lead, Concreate, Radioactive, Sensor
- Material changes are applied immediately
- Material names are exported to the CSV file

---

## Deleting Objects

- When an object is selected, a **Delete** button is shown
- Clicking Delete:
  - removes the object,
  - clears the current selection,
  - hides the gizmo and selection panel

---

## CSV Export

### Exporting

- Click **Export CSV**
- A CSV file is generated automatically
- The file is saved to the user’s **Desktop**

Example filename:
radiation_shapes_20260111_154233.csv


---

## CSV Format

Each row in the CSV represents one object in the scene.

### CSV Header example for the image given

```csv
id,type,material,px,py,pz,sx,sy,sz,rx,ry,rz,radius,radiusX,radiusZ,height
0,Cylinder,Concrete,0,1.705905,-3.170371,1,1,1,0,0,0,0,0.5,0.5,2
1,Sphere,Radioactive,0.058658,1.641646,-1.828835,0.5,0.5,0.5,0,0,0,0.25,0,0,0
2,Cylinder,Lead,0,1.570197,0,1,1,1,0,45,45,0,0.5,0.5,2
```

# Script Guide

### `Assets/Scripts/Core`

- `SelectionManager` (`RadiationEditor/Assets/Scripts/Core/SelectionManager.cs`)
  - Handles click selection using raycasts.
  - Ignores clicks on the gizmo layer so selection does not change while dragging.
  - Updates `TransformGizmo` and `TransformHud` target when selection changes.
  - Uses `gizmoLayer` and `selectableLayers` to filter raycasts.

- `ShapeManager` (`RadiationEditor/Assets/Scripts/Core/ShapeManager.cs`)
  - Singleton (`ShapeManager.I`) that owns the runtime list of shapes.
  - Creates primitives (cube, sphere, cylinder). Sensor is a small sphere.
  - Assigns shapes to the `Shapes` layer.
  - Attaches `ShapeData` and sets material and material name.

### `Assets/Scripts/Data`

- `ShapeType` (`RadiationEditor/Assets/Scripts/Data/ShapeType.cs`)
  - Enum: Cube, Sphere, Cylinder, Sensor.

- `ShapeData` (`RadiationEditor/Assets/Scripts/Data/ShapeData.cs`)
  - Stores shape type and selected material name.
  - Computes derived values (radius, radiusX, radiusZ, height) based on transform scale.
  - For cubes, derived fields remain zero and are still exported.

- `MaterialLibrary` (`RadiationEditor/Assets/Scripts/Data/MaterialLibrary.cs`)
  - ScriptableObject listing materials used by the HUD dropdown.
  - Asset at `RadiationEditor/Assets/ScriptableObjects/MaterialLibrary.asset`.

### `Assets/Scripts/Camera`

- `EditorFlyCamera` (`RadiationEditor/Assets/Scripts/Camera/EditorFlyCamera.cs`)
  - Free-fly camera using the old Input system (WASD + QE, RMB look, Left Shift boost).
  - Applies yaw and pitch with mouse deltas while RMB is held.

### `Assets/Scripts/Gizmo`

- `TransformGizmo` (`RadiationEditor/Assets/Scripts/Gizmo/TransformGizmo.cs`)
  - Shows and positions the gizmo around the selected object.
  - Raycasts against the `Gizmo` layer to detect handle clicks.
  - Drags along the active axis using a camera-aligned plane.
  - Optional distance scaling so the gizmo stays readable.

- `GizmoHandle` (`RadiationEditor/Assets/Scripts/Gizmo/GizmoHandle.cs`)
  - Procedurally builds an arrow mesh for each axis.
  - Colors axes (X red, Y green, Z blue).
  - Requires a MeshFilter and MeshRenderer.

- `GizmoAxis` (`RadiationEditor/Assets/Scripts/Gizmo/GizmoAxis.cs`)
  - Enum: X, Y, Z.

### `Assets/Scripts/UI`

- `TransformHud` (`RadiationEditor/Assets/Scripts/UI/TransformHud.cs`)
  - Drives the HUD input fields and material dropdown.
  - Updates UI text from the selected object each frame unless the user is editing a field.
  - Applies position, scale, rotation, and material changes back to the selected object.
  - Deletes the selected object and clears selection via `SelectionManager`.
  - Populates the material dropdown from `MaterialLibrary`.

- `HudSpawner` (`RadiationEditor/Assets/Scripts/UI/HudSpawner.cs`)
  - Spawns shapes from HUD buttons.
  - Uses current material dropdown selection.
  - Spawns in front of the camera, or on the raycast hit point if the mouse is over geometry.

### `Assets/Scripts/Export`

- `CsvExporter` (`RadiationEditor/Assets/Scripts/Export/CsvExporter.cs`)
  - Exports all shapes to CSV with a fixed header.
  - Writes to the user Desktop (falls back to `Application.persistentDataPath`).
  - Uses `ShapeData.RecomputeDerived()` before exporting.

- `CsvImporter` (`RadiationEditor/Assets/Scripts/Export/CsvImporter.cs`)
  - Opens a CSV and recreates shapes from rows.
  - Uses Unity Editor file picker in editor builds, and StandaloneFileBrowser in standalone builds.
  - Validates header and numeric fields; logs warnings for invalid rows.
  - Optional `clearExisting` to wipe shapes before import.

## Runtime Flow

- Spawn:
  - `HudSpawner` calls `ShapeManager.CreateShape`.
  - The new primitive gets a `ShapeData` component and material assignment.

- Select:
  - `SelectionManager` raycasts on left click.
  - If a shape is hit, `TransformGizmo` and `TransformHud` target that shape.

- Move:
  - `TransformGizmo` detects handle clicks and drags along one axis.
  - The gizmo repositions around the object every frame.

- Edit:
  - `TransformHud` writes to transform values and updates derived fields.
  - Material changes update renderer material and `materialName`.

- Export/Import:
  - `CsvExporter` builds a row per shape and saves it.
  - `CsvImporter` reads rows, creates shapes, assigns transforms and materials.

## Architecture Diagram (High Level)

```
                         +------------------+
                         |    HUDCanvas     |
                         | (HUDPanel, UI)   |
                         +---------+--------+
                                   |
           +-----------------------+-----------------------+
           |                                               |
  +--------v--------+                           +----------v---------+
  |   HudSpawner    |                           |    TransformHud    |
  | (spawn buttons) |                           | (edit fields, del) |
  +--------+--------+                           +----------+---------+
           |                                               |
           |                                               |
  +--------v--------+                           +----------v---------+
  |   ShapeManager  |<--------------------------|  SelectionManager  |
  | (create/list)   |         select/deselect   | (raycast selection)|
  +--------+--------+                           +----------+---------+
           |                                               |
           |                         +---------------------+--------+
           |                         |  TransformGizmo (move tool)  |
           |                         +---------------------+--------+
           |                                               |
  +--------v--------+                                      |
  |    ShapeData    |<-------------------------------------+
  | (type + derived)|
  +--------+--------+
           |
           |
  +--------v--------+        file dialog       +---------------------+
  |   CsvExporter   |------------------------->|   Desktop CSV File   |
  +-----------------+                          +---------------------+
  +-----------------+<-------------------------|   CsvImporter        |
  | (read CSV)      |        file dialog       | (StandaloneFileBrowser)
  +-----------------+                          +---------------------+
```

