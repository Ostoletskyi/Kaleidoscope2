# Second Display Output

`SecondDisplayOutputModule` is an output-only module for showing the director final output texture on Unity display index 1.

## Responsibility

- Read `KaleidoscopeDirector.FinalOutputTexture`.
- Activate `Display.displays[1]` when the state requests second-display output.
- Render the final output through a module-owned overlay canvas on the second display.

## Boundaries

- Input emits `ToggleSecondDisplayOutput`.
- Director updates `KaleidoscopeState.SecondDisplayOutputEnabled`.
- The module reads state and final output only.

The module does not control Mirror, Source, Camera, Recording, or any render processor internals.
