# XR Multiplayer Meeting

Unity VR Multiplayer prototype for a collaborative XR meeting experience.

## Project Idea

The project extends Unity's VR Multiplayer Template with a small collaborative XR feature:
networked 3D meeting markers that users can place in the scene with an XR controller ray.

Planned scope:

- Place spatial markers in VR through XR Interaction Toolkit input.
- Synchronize markers through Netcode for GameObjects.
- Display per-user colors and optional short labels.
- Use the feature as a focused prototype for collaborative XR communication.

## Meeting Marker Setup

Open the project in Unity and run:

`XR Multiplayer Meeting > Install Meeting Markers`

The setup creates the networked marker prefab, registers it with the template NetworkManager, adds a scene manager, and attaches marker placement to the right-hand XR ray interactor.
