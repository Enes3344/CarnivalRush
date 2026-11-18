# Carnival Rush

A first-person arcade shooting gallery for Unity 6. Pop every colored balloon before the clock expires. A hit on a black balloon marked with an X ends the run immediately.

## Open and play

1. In Unity Hub, open this `CarnivalRush` folder as a project with **Unity 6.0 LTS (6000.0.62f1)**. This workspace started without a Unity project, so the prototype uses the **Built-in Render Pipeline** and Unity UI.
2. Let Unity resolve packages and compile. Run **Tools > Carnival Rush > Generate Game**. The command saves and opens `Assets/CarnivalRush/Generated/CarnivalRush.unity` and registers it in the scene list.
3. Press **Play**, then click **Start game** in the Game view. Move the mouse to aim, left-click to shoot, and press **Escape** to pause or resume. The pause and Game Over menus both offer a restart.

The scene is generated on demand. Running the command again rebuilds the generated scene at the same path and updates its materials in place. Make a copy of the generated scene before customizing it if you intend to regenerate. You can also run the generator in a licensed Unity Editor with `-batchmode -quit -projectPath <project-folder> -executeMethod CarnivalRush.Editor.GenerateCarnival.GenerateGame -logFile <log-file>`.

To make a standalone build, open **File > Build Profiles**, select a desktop profile, confirm the generated scene is enabled in its scene list, and choose **Build and Run**. The project uses the legacy Input Manager. If you change Active Input Handling under **Project Settings > Player**, use **Input Manager (Old)** or **Both**.

## Round rules

- Level 1 starts with 8 colored balloons, 1 black balloon, and 20 seconds.
- The safe count grows every two levels and the black count grows every three levels, within the 25-target cap. At the cap there are 19 safe and 6 black balloons.
- The clock loses 0.75 starting seconds per level until level 17, when it reaches 8. Later rounds start at 8 displayed seconds and count down faster using a separate multiplier. The speed approaches 1.25×, so the shortest real round remains 6.4 seconds, including at the target cap.
- Each round shuffles 28 slots across the wall and adds bounded random offsets. Minimum center separation along a shared row or column is 0.80 m, which leaves clearance between the balloon meshes.
- Each safe hit adds 10 points, and clearing a level adds 100. The game records the highest level completed in the run and stores the high score and highest completed level across runs with PlayerPrefs.

## Play-mode checks

The C# round rules passed a standalone check over 100,000 generated levels and layouts. The Unity Editor was unavailable in the development environment. Use the checks below to verify scene generation and play mode on your machine.

| Check | Expected result |
| --- | --- |
| Generate twice | One generated scene, one build list entry, and one camera, HUD, manager, and target wall in the scene. |
| Start Level 1 | Eight safe balloons in five colors, one black balloon marked with an X, and a timer starting at 20 seconds. Aim over the full wall and confirm each target is reachable. |
| Shoot a safe balloon | The aimed balloon pops with confetti and sound, the toy gun flashes and recoils, score increases by 10, and the counter drops by one. Shooting the wall leaves score and counter unchanged. |
| Clear Level 1 | The clear banner is visible briefly, the score reaches 180, and Level 2 starts with a fresh layout and a reset clock. |
| Shoot black or let the clock expire | Immediate Game Over with the corresponding reason, and firing and the timer stop. |
| Pause, resume, and restart | Escape freezes the timer and releases the cursor. Resume restores aim controls. Restart returns to Level 1 and zero score. |
| Leave and reopen Play mode | The high score and highest completed level remain in the HUD. |
| Play or debug into late levels | From Level 17 the clock resets to 8 displayed seconds. Speed increases from Level 18 onward, layouts keep changing, and the wall stays at or below 25 targets. |
| Run a desktop build | Buttons, relative mouse aim, audio, materials, and target collisions work outside the editor. |

## Source map

- `Runtime/CarnivalGame.cs` controls the game flow, countdown, score, pause, and persistence.
- `Runtime/RoundRules.cs` contains difficulty and layout rules.
- `Runtime/BalloonWall.cs` and `Runtime/Balloon.cs` handle target placement, hit colliders, and popping.
- `Runtime/ToyShooter.cs` handles mouse aim, camera-center raycasts, toy-gun animation, and synthesized audio.
- `Runtime/CarnivalHud.cs` builds and updates the Canvas.
- `Editor/GenerateCarnival.cs` assembles the scene using primitive meshes and generated materials.
