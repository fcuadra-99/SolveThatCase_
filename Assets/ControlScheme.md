# Solve That Case - Control Scheme Documentation

## Overview
This document outlines the comprehensive control scheme for the "Solve That Case" detective game, based on the Unity scripts in the project. The game features multiple control systems for investigation, dialogue, cross-examination, and UI management.

## Core Control Systems

### 1. Main Input Controls (`Controls.cs`)
**Primary Input Handler** - Manages all player input and interaction systems.

#### Input Methods:
- **Mouse Input**: Left click for selection, drag for camera movement
- **Touch Input**: Single touch for mobile devices, drag for camera movement
- **Double-click/Tap**: Quick selection of objects

#### Key Features:
- **Crosshair System**: Visual cursor that scales when hovering over interactable objects
- **Drag Threshold**: Configurable distance before drag input is registered
- **UI Detection**: Prevents input when cursor is over UI elements
- **Camera Clamping**: Keeps camera within scene boundaries

#### Object Interaction:
- **Items** (Tag: "Items"): Collect evidence and items
- **Doors** (Tag: "Doors"): Navigate between scenes
- **Characters** (Tag: "Characters"): Talk to NPCs

### 2. Background Control (`BGControl.cs`)
**Scene Management System** - Handles scene transitions and door interactions.

#### Features:
- **Scene Switching**: Manages multiple background scenes
- **Door System**: Clickable doors that trigger scene transitions
- **Fade Transitions**: Smooth visual transitions between scenes
- **Audio Integration**: Door sound effects during transitions

#### Configuration:
- Multiple scenes can be defined
- Each door has a target scene index
- Fade duration and overlay settings
- Audio source for sound effects

### 3. Dialogue Controls (`DialogControls.cs` / `DialogManager.cs`)
**Conversation System** - Manages all dialogue interactions.

#### Input Controls:
- **Spacebar**: Skip typing animation or advance dialogue
- **Mouse/Touch**: Click "Next" button to continue
- **Choice Selection**: Click dialogue choice buttons

#### Features:
- **Typewriter Effect**: Text appears character by character
- **Voice Lines**: Audio playback for character speech
- **Character Switching**: Shows active character during dialogue
- **Choice System**: Multiple dialogue options with consequences
- **Meter Integration**: Dialogue choices can affect logic meter

#### Dialogue Events:
- Character name display
- Text content with typing animation
- Voice line audio
- Character activation/deactivation
- Jump indices for branching dialogue
- Choice arrays for player decisions

### 4. Cross-Examination Controls (`CrossControl.cs`)
**Trial System** - Manages courtroom cross-examination sequences.

#### Controls:
- **Present Evidence Button**: Submit evidence during testimony
- **Present Profile Button**: Submit character profiles
- **Testimony Loop**: Automatic cycling through witness statements

#### Features:
- **Logic Meter Integration**: Correct/wrong evidence affects meter
- **Evidence Validation**: Checks if presented evidence matches requirements
- **Penalty System**: Wrong evidence reduces logic points
- **Reward System**: Correct evidence increases logic points

#### Testimony System:
- Looping witness statements
- Evidence presentation during specific testimony points
- Correct evidence identification
- Dialogue responses for correct/incorrect evidence

### 5. File Collection Controls (`FileCollection.cs`)
**Evidence Management** - Handles collection and presentation of evidence.

#### Controls:
- **Item Collection**: Double-click/tap items to collect
- **File Browser**: View collected evidence
- **Evidence Presentation**: Present items during cross-examination

#### Features:
- **Item Database**: Predefined evidence with descriptions
- **Collection Tracking**: Prevents duplicate collection
- **UI Integration**: Creates buttons for collected items
- **Focus System**: Camera focuses on collected items
- **Dialogue Integration**: Items can trigger dialogue events

### 6. Profile Collection Controls (`ProfileCollection.cs`)
**Character Management** - Handles character interactions and profiles.

#### Controls:
- **Character Interaction**: Click characters to talk
- **Profile Viewing**: View character information
- **Profile Presentation**: Present character profiles during trials

#### Features:
- **Character Database**: Predefined characters with profiles
- **Meeting Tracking**: Tracks which characters have been met
- **Dialogue System**: Different dialogue for first meeting vs. subsequent meetings
- **Focus Integration**: Camera focuses on characters during interaction

### 7. UI Controls (`UIControl.cs`)
**Interface Management** - Controls all UI panel visibility and state.

#### Controls:
- **File Panel**: Toggle evidence collection interface
- **Options Panel**: Access settings and configuration
- **Log Panel**: View dialogue history
- **Crosshair**: Show/hide based on UI state

#### State Management:
- **Drag Control**: Disables when UI panels are open
- **Crosshair Visibility**: Hidden during dialogue and UI interactions
- **Panel Toggles**: Independent control of each UI panel

### 8. Spotlight Control (`SpotlightControl.cs`)
**Logic Meter System** - Manages the game's logic/confidence meter.

#### Features:
- **Visual Meter**: Slider showing current logic points
- **Point System**: Gain/lose points based on correct/incorrect evidence
- **Visual Feedback**: Color changes and pulsing effects
- **Position Animation**: Meter moves during dialogue
- **Fluctuation Effect**: Subtle visual movement for realism

### 9. Sequence Control (`SequenceControl.cs`)
**Game Flow Management** - Controls the overall game progression.

#### Game Phases:
- **Dialogue**: Story conversations
- **Investigation**: Evidence collection phase
- **Post-Investigation Dialogue**: Post-collection conversations
- **Trial**: Cross-examination sequences
- **Complete**: Game completion

#### Features:
- **Phase Progression**: Automatic advancement through game phases
- **Music Management**: Background music changes per phase
- **System Integration**: Coordinates all other control systems

### 10. Settings Controls (`Settings.cs`)
**Configuration Management** - Handles all game settings and preferences.

#### Settings Categories:
- **Gameplay**: Text speed, crosshair speed
- **Audio**: Master volume, BGM volume, SFX volume
- **Visual**: Brightness adjustment

#### Features:
- **Persistent Storage**: Settings saved using PlayerPrefs
- **UI Integration**: Slider controls for all settings
- **Default Reset**: Restore default settings option

## Input Mapping Summary

### Primary Controls:
- **Mouse/Touch**: Move crosshair and interact with objects
- **Double-click/Tap**: Select and collect items
- **Spacebar**: Advance dialogue or skip typing
- **UI Buttons**: Navigate menus and present evidence

### Secondary Controls:
- **Drag**: Move camera and crosshair
- **Hover**: Scale crosshair over interactable objects
- **Panel Toggles**: Open/close UI panels

## Control Flow

### Investigation Phase:
1. Use crosshair to explore scenes
2. Double-click items to collect evidence
3. Click characters to talk and collect profiles
4. Use UI panels to review collected information

### Trial Phase:
1. Listen to witness testimony
2. Present evidence at appropriate moments
3. Present character profiles when relevant
4. Monitor logic meter for feedback

### UI Navigation:
1. Toggle file panel to view evidence
2. Toggle options for settings
3. Toggle logs to review dialogue history
4. Use crosshair when UI is closed

## Technical Implementation

### Input Detection:
- Uses Unity's EventSystem for UI detection
- Physics2D raycasting for object interaction
- Touch and mouse input handling
- Drag threshold calculations

### State Management:
- UI state controls input availability
- Dialogue state affects crosshair visibility
- Phase progression manages game flow
- Settings persistence across sessions

### Integration Points:
- All systems communicate through events
- Shared data structures for consistency
- Centralized control through main input handler
- Modular design for easy extension

## Accessibility Features

### Visual Feedback:
- Crosshair scaling on hover
- Logic meter visual effects
- Color-coded feedback for correct/incorrect actions
- Smooth animations and transitions

### Audio Integration:
- Voice lines for dialogue
- Sound effects for interactions
- Background music management
- Volume controls for all audio

### Customization:
- Adjustable text speed
- Configurable crosshair speed
- Volume controls for different audio types
- Brightness adjustment

This control scheme provides a comprehensive detective game experience with intuitive controls for investigation, dialogue, and courtroom sequences.

