Spawner / Fuel / UI Setup

1) Spawner
- Add the `Spawner` component to an empty GameObject in the scene.
- Assign `prefabToSpawn` to the pickup prefab (e.g., a sprite with `FuelPickup` component).
- Set `minInterval` and `maxInterval` (default 2-6 seconds).
- Adjust `areaCenter` and `areaSize` in the inspector. Use the gizmo to visualize the spawn area.

2) Fuel System
- Add `FuelSystem` to the player GameObject (or another manager GameObject) and set `maxFuel`, `startFuel`, and `fuelDrainPerSecond`.
- Ensure the player's GameObject also has `PlayerController` so pickups detect it.

3) Fuel Pickup
- Create a pickup prefab: add a Collider2D (set isTrigger = true), sprite, and `FuelPickup` component.
- Set `fuelAmount` on the pickup prefab.
- Use `Spawner.prefabToSpawn` to reference this prefab.

4) UI
- Create a Canvas with a Slider and two Text elements.
- Assign Slider -> `UIManager.fuelSlider`, Fuel Text -> `UIManager.fuelText`, Survival Time -> `UIManager.survivalTimeText`.
- Assign `UIManager.fuelSystem` to your `FuelSystem` instance.

5) Testing
- Play the scene. Pickups will spawn randomly inside the gizmo area. When player touches a pickup, fuel increases and UI updates.
