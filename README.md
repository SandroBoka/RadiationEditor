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



