# DBR77_Aakash_Mehta_Task

Unity 6000.3.2f1 take-home task implementing a data-driven function-block graph runtime for creating and manipulating scene objects, with branching logic and JSON import/export.

## What is implemented

- Runtime graph model (`GraphAsset`, `GraphData`, `NodeData`, `LinkData`)
- Graph runner with step guard and coroutine-based delay execution
- Object workflow nodes:
  - `Start`
  - `CreatePrimitive`
  - `SetTransform`
  - `Delay`
  - `Log`
- Logic workflow nodes:
  - `SetVar`
  - `Add`
  - `Compare` (`True` / `False` ports)
- JSON serialization:
  - export/import using Unity `JsonUtility`
  - graph validation before export and before import
- Editor menu utilities for JSON workflow

## JSON tools

Select a `GraphAsset` in the Project window, then use:

- `Assets/TwinGraph/Export Selected GraphAsset To JSON`
- `Assets/TwinGraph/Import JSON Into Selected GraphAsset`
- `Assets/TwinGraph/Validate Selected GraphAsset`

Runtime files:

- `Assets/TwinGraph/Runtime/Serialization/GraphSerializer.cs`
- `Assets/TwinGraph/Runtime/Serialization/GraphValidator.cs`
- `Assets/TwinGraph/Runtime/Serialization/Editor/GraphJsonMenu.cs`

## Sample graph assets

Located in `Assets/TwinGraph/Samples/ExampleGraphs`:

- `Start_Log_SmokeSample.asset`
  - Flow: `Start -> CreatePrimitive -> Delay -> SetTransform -> Log`
  - Default output:
    - Creates `DemoCube` at position `(0, 0.5, 0)` with scale `(1, 1, 1)`
    - Waits `1.0` second
    - Transforms same cube to position `(1.5, 0.5, 0)`, rotation `(0, 30, 0)`, scale `(1.25, 1.25, 1.25)`
    - Logs: `[TwinGraph] Object workflow sample executed.`
- `Logic_Branch_Sample.asset`
  - Flow: `Start -> SetVar(score) -> SetVar(bonus) -> Add(total) -> Compare(total >= 8) -> True/False`
  - Default values: `score=5`, `bonus=3`, so `total=8`
  - Default output:
    - Takes `True` branch
    - Creates `TrueCube` at `(-1.25, 0.5, 0)`
    - Logs: `[TwinGraph] True branch executed (total >= 8).`
  - Alternate output when `total < 8`:
    - Takes `False` branch
    - Creates `FalseSphere` at `(1.25, 0.5, 0)`
    - Logs: `[TwinGraph] False branch executed (total < 8).`
- `Logic_MultiThreshold_Branch_Sample.asset`
  - Flow: `Start -> SetVar(score) -> SetVar(bonus) -> Add(total) -> CompareHigh -> CompareMid`
  - Classification:
    - High if `total >= 8`
    - Medium if `5 <= total < 8`
    - Low if `total < 5`
  - Default values: `score=4`, `bonus=3`, so `total=7`
  - Default output:
    - Takes `Medium` path
    - Creates `MidCube` at `(0, 0.5, 0)` with scale `(1.2, 1.2, 1.2)`
    - Logs: `[TwinGraph] Medium branch executed (5 <= total < 8).`
  - Alternate outputs:
    - High path creates `HighCylinder` at `(-2, 1, 0)` with scale `(1, 2, 1)`
    - Low path creates `LowSphere` at `(2, 0.5, 0)`

## Quick verification

1. Open `Assets/Scenes/SampleScene.unity`.
2. On `GraphRunnerGO`, assign one of the sample `GraphAsset`s.
3. Press Play and confirm logs and created primitives.
4. Select the same `GraphAsset` in Project window.
5. Export JSON using `Assets/TwinGraph/Export Selected GraphAsset To JSON`.
6. Re-import that JSON into the graph asset.
7. Press Play again and confirm behavior is unchanged.
