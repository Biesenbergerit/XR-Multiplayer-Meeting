# XR Multiplayer Meeting

XR Multiplayer Meeting is a Unity-based Quest 3 prototype for collaborative meetings in mixed reality.

The app focuses on a shared virtual meeting room with a local Reality Window that lets users briefly look back into their physical environment through Quest passthrough. The project is built for Android/OpenXR on Meta Quest hardware.

## Core Features

- Collaborative XR meeting room
- Local Reality Window with Quest passthrough
- Movable and resizable in-world Reality Window
- Local room hosting for headset testing
- Quest 3 Android/OpenXR configuration

## Unity Setup

Unity version: 6000.4.0f1

Open the main scene:

`Assets/Scenes/SampleScene.unity`

The Reality Window prefab is located at:

`Assets/VRMPAssets/Prefabs/RealityWindow/Reality Window.prefab`

The main scripts are located at:

`Assets/VRMPAssets/Scripts/Gameplay/RealityWindow`

## Reality Window

The Reality Window is spawned by the `RealityWindowManager` in the scene. It appears in front of the player and uses passthrough composition so the real world is visible only inside the window area.

Controls:

- Use `+` and `-` to resize the window.
- Use `R` to reset it in front of the player.
- Use `X` to close it.

## Quest Setup

The project is configured for Quest 3 through Android/OpenXR settings. If settings need to be restored, run:

`XR Multiplayer Meeting > Configure Quest Passthrough`

This configures the Android package settings, OpenXR/Meta Quest support, and Quest passthrough requirements.

## Build And Install

Target platform: Android

To install on a Quest headset:

1. Enable Developer Mode on the headset.
2. Connect the headset to the PC.
3. Build and run from Unity for Android.

The installed app appears on the Quest under Unknown Sources.
