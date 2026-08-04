# Dungeon Generator – Unity Editor Tool

Practical component of the thesis **"Analysis and Evaluation of the Use of In-House Proprietary Tools in Game Development, with a Focus on Environment Tools"**.

A Unity Editor tool that lets game designers - **without any programming knowledge** - create and configure procedurally generated dungeons. The tool uses **Binary Space Partitioning (BSP)** to generate rooms, connects them via corridors, and automatically classifies rooms by type.

## Features

- **Custom Editor Window** under `Praxisarbeit → DungeonGenerator` with two tabs: *Dungeon Settings* and *Room Settings*
- **BSP-based room layout** – recursive subdivision of the dungeon area into rectangular regions
- **Automatic room classification** into six room types: Spawn, Boss, Treasure, Puzzle, Shop, NPC (+ Standard fallback)
- **ScriptableObject-based configuration**:
  - `RoomData` – prefabs (Floor/Wall/Ceiling/Door), wall height, spawnable content rules and spawn probability per room type
  - `Room Type Library` – collection of all RoomData objects, referenced by the generator
- **Configurable generation parameters**: Dungeon Width/Length, Min Room Size, Max Split Depth, Corridor Width (Small/Medium/Large), Fixed Seed for reproducible results, Random Room Sizes
- **JSON import/export** – room configuration and dungeon layout can be saved and restored
- **Automatic validation** with clear error messages (e.g. missing Dungeon Parent, invalid dungeon size, split depth too high)
- **Live updates** – changes to RoomData are applied to an existing dungeon in the scene immediately, without needing to regenerate

## Technical Background

This tool serves as the practical proof-of-concept for the concepts examined in the theoretical thesis on in-house environment tools - including analyses of *Binding of Isaac* (parametric room-count formula, BFS expansion, seed-based determinism) and *Skyrim* (kit-based combination rules, "high bang-for-buck" principle).

## Usage

A detailed guide for game designers is included in the repository as [`Anleitung.pdf`](./Anleitung.pdf) (German) [`Guide_EN.pdf`](./Guide_EN.pdf) (English) and covers:

1. Opening the tool (`Praxisarbeit → DungeonGenerator`)
2. Assigning Dungeon Parent and Room Type Library
3. Configuring size, corridors and seed
4. Setting up room types in *Room Settings*
5. Generating, reviewing and saving the dungeon

### Recommended Workflow (Short Version)

1. Assign Dungeon Parent & Room Type Library
2. Set Dungeon Width/Length, Min Room Size and Max Split Depth
3. Set Corridor Width to *Medium* (recommended default)
4. Enable Fixed Seed if needed
5. Configure room types in *Room Settings*
6. Click **Generate Dungeon**
7. Save the current state via **Export**

## Author

Philip Nitschke – Game Design, Macromedia Akademie München
Supervised by: Herr Kohl

## License
for educational purposes only
