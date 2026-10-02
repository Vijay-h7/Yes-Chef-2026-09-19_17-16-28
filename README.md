# Yes Chef

A cozy, Overcooked-style top-down cooking game built in Unity 6000.6.2f1.

## Gameplay loop

1. Walk up to the **Fridge** and press **E** to open it.
2. Pick one of 3 ingredients - **Vegetable**, **Cheese**, or **Meat** - from the popup menu (or press 1 / 2 / 3).
3. The ingredient appears in the chef's hand. While your hands are full, the fridge won't open again.
4. Prepare it at the **Table** (chop) or **Stove** (cook), then deliver it to the matching customer.
5. Use the **Trash** to discard a mistake and free up your hands.
6. Score as many correct orders as you can before the 180-second round ends.

## Project structure

- `Assets/Scripts/` - all gameplay code (Chef, Station, FridgeStation, FridgeMenu, GameManager, CustomerWindow, etc.)
- `Assets/Models/` - character and prop models
- `Assets/Prefabs/` - station and environment prefabs
- `Assets/Scenes/` - the main playable scene

## Stations (current scene)

- Fridge - ingredient source, opens the 3-choice picker
- Table - chopping/prep station
- Stove - cooking station
- Trash - discard station

## Notes for contributors

- `Assets/TutorialInfo/` contains Unity's default template welcome asset. It's been stubbed out (no popup, no GUI) and is safe to ignore or delete entirely.
- Station transforms are easy to accidentally drag out of place in the Scene view - if a station stops responding to interaction, check that its position wasn't nudged, then verify the fix is actually saved (File > Save, or Ctrl+S) since unsaved edits are lost on Editor/domain reload.
