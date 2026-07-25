# Gameplay Foundation

The playable scene is generated at `Assets/_Project/Gameplay/Scenes/JarLoop.unity`.

Open it and enter Play Mode. Drag the orange lid horizontally by at least 20% of
the screen width, type an item, then press Enter or the **Drop** button. Escape,
Android back, or **Cancel** restores the lid without spawning.

The local catalog contains the twelve vertical-slice items plus a safe
idea-object fallback. Runtime item visuals are mapped from `VisualArchetype`;
prompt text is never used as an asset or prefab path. Pair and environment
actions are derived by `CapabilityAffordanceResolver`.

Editor menu:

- `Human Glass Watcher > Build Playable Jar Scene`
- `Human Glass Watcher > Validate Playable Jar Scene`
