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

## Reality Window Setup

Open the project in Unity and run:

`XR Multiplayer Meeting > Install Reality Window`

The setup adds a small local XR menu and a movable personal Reality Window to the multiplayer template scenes. Selecting the menu button opens a grabbable window in front of the player. The window uses passthrough/transparent camera composition so the real world can be visible through that panel on supported XR hardware.

Controls:

- Select the small `Reality Window` menu button to open or hide the window.
- Grab the window frame to move it around.
- Use `+` and `-` on the window to resize it.
- Use `R` to place it back in front of the player.
- Use `X` to close it.

## Quest Passthrough Setup

The project includes Unity OpenXR: Meta for Quest passthrough support. If the XR settings ever need to be restored, run:

`XR Multiplayer Meeting > Configure Quest Passthrough`

This selects the Meta Quest OpenXR feature set, enables Meta Quest Session and Camera (Passthrough), keeps the multiplayer internet permission, and sets the Android minimum SDK to 32 for Quest passthrough.

To install on a Quest, enable Developer Mode on the headset, connect it over USB, approve USB debugging in the headset, then use Unity's Android `Build And Run`. The app appears on the Quest under Unknown Sources.
