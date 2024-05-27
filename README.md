
<table>
  <tr>
    <td>Authors:<br>
      &nbsp;&nbsp;&nbsp;Alexander Martinez<br>
      &nbsp;&nbsp;&nbsp;Samuel Hilfer</td>
  </tr>
</table>

<h1 align="center">Action Platformer Base Mechanics</h1>
<p align="center"><i>A repository that holds base-level mechanics for a 2D action platformer in Unity.</i></p>
<p align="center">Please share whatever feedback you have, or projects you are working on over in the <a href="https://github.com/GrowingPaigns/15-Minutes-From-Jupiter/discussions">Discussions tab</a></p>

---

:information_source: **This repo is no longer being actively updated as of (09/13/23) for two reasons**. :information_source:
- ❗ _**First**, we feel that this is a solid base that many interested C.S. students could iterate upon, especially because the physics are very basic, and can definitely be refined._
    - _With that said, **we hope you will at least credit us if you incorporate this code into your own project**_
- ❗ _**Second**, we are starting to get into more complex aspects of this game (like making better physics) which we do not necessarily want to be publicly accessible. Therefore, we are moving all work on this game over to a private repository so that we can continue to iterate upon this base in our own way_

### Implemented Objectives:
- [x] **Movement:**
  - **🏃 (Basic) Physics-Based Platforming Movement**
    - [X] Jumping
    - [X] L/R Sprinting + Walking
    - [X] Ability to Climb on specific surfaces _(walls)_
    - [X] Ability to Dash 3 times before needing to touch the ground
    - [X] Ability to Wall Jump     
  - **🎞️ Movement Animations _using Unity [Animator](https://docs.unity3d.com/Manual/AnimatorWindow.html) Parameters_.**

- [x] **Player Combat:**
    - **🏹 Basic 360° Attack**
      - [X] Player moves _(slightly)_ towards the direction of the attack
      - [X] On attacking an enemy, the screen shakes slightly 
    - **🎞️ Basic Slice Animation with Custom Hitboxes**

- [x] **Basic Artificial Intelligence _(Enemy AI)_:**
    - **👿 Close Range Melee Enemy:**
        - [x] 🏃 Automated, Random, Physics-Based Horizontal Movement.
          - [x] Wall Detection for Turning Around.
          - [ ] Floor Detection for Gap Avoidance (Turning Around).
        - [x] **👁️ Detections using _[Physics 2D raycast](https://docs.unity3d.com/ScriptReference/Physics2D.Raycast.html)_:**
          - [x] Player Detection triggers chase parameters.
            - enemy ignores terrain changes and chases after the player (does not jump gaps) 
        - [x] **🏹 Automated Attacks:**
          - [X] When a player is in range, the enemy plays a special attack animation
            - while the attack is on cooldown, enemy simply charges the player
        - [x] **💔 Health States:**
          - [X] enemy has 2 health states, injured, and not injured, with corresponding animations for each state 

- [x] **Environment:**
  - **🏕️ Basic Tile Map:**
    - [X] Tilemap System for ground, walls, ramps, and other objects in the environment
    - [X] Distinguished uses for each layer
      - Sign layer has tutorial message bubbles
      - Wall layer used to detect wall climbing/jumping functionality
      - etc...
   - **🎥 Camera:**
     - [X] Basic Camera Follow System
       - Camera lags a little behind the player while moving, but ultimately just follows the player's position   

- [X] **User Interface (UI):**
  - **🖼️ Basic Main Menu System:**
    - Screen for Options, HighScore, Info _(all work in progress builds, nothing functional)_
    - Selectable options for New Game, and Quit _(work)_
    - [X] Basic Button Animations
      - [ ] Button interaction sound fx
    - [X] Basic Background Music
  - **⚙️ Basic Pause Menu System:**
    - [X] Pressing ESC in-game pauses game systems and brings up a menu with different selectable options
  - **🔲 Basic HUD (Heads Up Display):**
    - [X] Health Hearts that update according to enemy attacks
    - [X] Stamina Crystals that update according to how many times in a row the player dashes
    - [X] Basic Timer _(just counts down, doesn't do anything else yet)_ 
---
### 🖥️ [Project Overview](https://github.com/GrowingPaigns/15-Minutes-From-Jupiter/tree/main/Code):
- Contains the in-progress Unity project files for the developed product (Complete Unity Project)
    - Assets *(Holds a mix of game assets)*
        - Player Movement, Animations, Attack Mechanics, Health, Dodge Mechanics
        - Basic Small Enemy Movement, Animations, Attack Mechanics, Death Mechanics
        - Crosshair
        - Main Menu System
        - Pause Menu
        - Timer UI
        - Health UI
        - Basic 2D Tiled Beta Level for Testing
        - Camera with Follow System
    - *(Various other files necessary to run the project)*
---
### **Project Directory**
#### 🎵 [Ambiance](https://github.com/GrowingPaigns/Unity-2D-Platformer-Prototype/tree/main/Code/15%20Minutes%20From%20Jupiter/Assets/MainMenuAssets/Menu%20Ambiance): 
- Contains Menu Music/SFX assets *(Button SFX not implemented)*

#### 🖌️ [Menu](https://github.com/GrowingPaigns/Unity-2D-Platformer-Prototype/tree/main/Code/15%20Minutes%20From%20Jupiter/Assets/MainMenuAssets)/[Game](https://github.com/GrowingPaigns/Unity-2D-Platformer-Prototype/tree/main/Code/15%20Minutes%20From%20Jupiter/Assets/LevelAssets) Art:
- Contains all assets related to menu systems and in-game elements

#### 📜 [Code/Scripts](https://github.com/GrowingPaigns/Unity-2D-Platformer-Prototype/tree/main/Code/15%20Minutes%20From%20Jupiter/Assets/Scripts):
- Contains all programmed elements 

#### 🎥 [Scenes](https://github.com/GrowingPaigns/Unity-2D-Platformer-Prototype/tree/main/Code/15%20Minutes%20From%20Jupiter/Assets/Scenes):
- Contains different loadable scenes accessible through in-game means 

---   
### General Information:

**🏭 Runtime Environment**

- Runtime: **[Windows](https://www.microsoft.com/en-us/windows) 10**
- Game Engine: **[Unity](https://unity.com/releases/editor/whats-new/2022.3.7) 2022.3.7**
- Scripts Editor: **[Visual Studio](https://visualstudio.microsoft.com) 2022**

**📚 Learning Resources I Used**

- 📕 [Unity Documentation](https://docs.unity.com)
- 📼 [Health Heart System - By BMo](https://www.youtube.com/watch?v=5NViMw-ALAo)
- 📼 [BlackThornProd](https://www.youtube.com/@Blackthornprod)
- 📼 [Brackeys](https://www.youtube.com/@Brackeys)

**📄 My Own Tutorials**
- 📼 [2D Custom Health System](https://www.youtube.com/watch?v=sAy0K5W9Woc)

---      
### Outside Asset References:
- [Julio Cacko - Input Art Assets](https://juliocacko.itch.io/free-input-prompts) 
---   
