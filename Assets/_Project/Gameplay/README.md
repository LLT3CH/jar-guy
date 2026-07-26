# Gameplay Foundation

The playable scene is generated at `Assets/_Project/Gameplay/Scenes/JarLoop.unity`.

Open it and enter Play Mode. Drag the orange lid horizontally by at least 20% of
the screen width, type an item, then press Enter or the **Drop** button. Escape,
Android back, or **Cancel** restores the lid without spawning.

The local catalog contains the twelve vertical-slice items plus a safe
idea-object fallback. Each authored entry has a recognizable low-poly composite
visual with a simple authoritative physics collider; prompt text is never used
as an asset or prefab path. Pair and environment actions are derived by
`CapabilityAffordanceResolver`.

Juniper shows an immediate badge and an explicit Character presentation pose
for food, play, comfort, tools, light, and hazards. A banner calls out newly
discovered item-to-item actions. Gameplay calls the public
`ResidentPresentationController.SetReaction` seam; a short motion/color cue is
retained only as a missing-controller fallback. Full Character appraisal, needs,
memory, and persistent consequences remain integration work.

Procedural colors use the two checked-in material templates under
`Resources/ProceduralMaterials`. Runtime code clones and caches those referenced
materials and does not use `Shader.Find`; do not add URP Lit to Graphics
Settings' always-included shaders.

Editor menu:

- `Human Glass Watcher > Build Playable Jar Scene`
- `Human Glass Watcher > Validate Playable Jar Scene`
- `Human Glass Watcher > Start Live Jar Demo`
- `Human Glass Watcher > Rebuild Procedural Material Assets`
