Thank you for buying Daedalus!

Version 1.0.7


ChangeLog

v1.0.7 - Current version
- feature: added Fake3D option to the TileMapInterpreter, when used with a 2D map. It will change all Walls and Columns to either Fake_FrontWall or Fake_AboveWall.
- feature: more control on how the entrance and exit are placed
- allows a minimum and maximum distance to be specified (leaving both to 100%, it will always place it at the farthest distance)
- the entrance is never placed in the middle of a corridor
- modified VirtualMap to check floor cell distances (added different methods for the purpose)
- feature: added directional walls for StandardMap and TileMap
- feature: Added Prefabs2DPhysicalMap, which works similarly to Meshes3DPhysicalMap but uses prefabs created using Sprites. This can be useful to instantiate sprites just like prefabs (with other gameobjects as children).
- minor feature: Meshes3DPhysicalMap and Prefabs2DPhysicalMap now support setting the tile sizes manually.
- bugfix: rooms that are too small to have directional floors will now use the RoomFloorInside tile instead of the default RoomFloor tile.
- bugfix: removed useless DimensionalMapInterpreterAddon file
- bugfix: added ladders in the SelectionManager
- bugfix: exit and entrance with map orientation YZ now works correctly
- bugfix: meshes 3D physical map now uses correct distances for computing exit and entrance positions
- bugfix: updated to Unity 4.5 (there was an issue with GenericDrawer)
- misc: added assert to guarda against missing variations
- misc: renamed "MapGenerator" to "VirtualMapGenerator"
- misc: renamed some methods in VirtualMap to match camel case
- misc: changed 'SendMessage' to 'BroadcastMessage' when generation ends
- misc: added 'IsCurrentlyGenerating' public flag to GeneratorBehaviour
- misc: Meshes3DPhysicalMap now will raise an assertion error when it cannot get the mesh filter bounds.
- misc: When Generate is clicked, any existing older maps are now deleted (useful for when errors appear).
- bugfix: fixed small bug with default loading for Sprites3D
- bugfix: fixed a few bugs while generating directional tiles
- updated 2DToolkit with support for directional walls
- updated with new directional wall assets
- fake3dWalls option is now enabled only for Sprites2DPhysicalMap
- added default fake3d assets
- added default standard map directional columns
- enabled directional walls for StandardMap  
- changed directional Walls to Columns (only for StandardMap)
- bugfix: if using directional tiles, fake3D is automatically disabled
- bugfix: fixed Sprites2D_Prefabs tile priorities issues
- known issue: 2DToolkit will give a "GUILayout: Mismatched LayourGroup.Repaint" exception when using the editor. This is harmless.
- directional tiles now work with fak3D tiles
- bugfix: fixed orientation of fake3d assets
- bugfix: fixed IsCorridorDirectionalWall()
- added new fake3d assets
- rocks behave better with fake3d assets, as does the bottom side of the dungeon
- added DoorHorizontal e DoorVertical for the doors
- DoorHorizontal needs a slightly taller sprite

v1.0.0
Fixed ceiling over the stairs bug
Added ‘last used seed’ in the GeneratorBehaviour advanced panel. This can be useful for reporting bugs (even when not choosing a seed)
Added new physicalMap.GetObjectsOfZone(int zone_number) method, that allows to select a portion of the map. This is used now for differentiating storeys
Added default values to Sprites2DMap

v0.13.10
Fixed default bugs and values: now any PhysicalMap is ready-to-go just after being added to the Generator Behaviour!
default storey height is now 1
fixed orientations of StandardMap3D and TileMap3D
tk2dspritephysicalmap now has default tile texture size of 4
all default IDs for tk2dspritephysicalmap
tk2dspritephysicalmap now has default tile texture size of 32 and pixels per meter of 8
all defaults IDs and layers for tk2dtilesphysicalmap
added default values to Sprite2DMap

v0.13.9
Added new defaults for everything

v0.13.8
Added option for TileMap perimeter orientation

v0.13.7
Perimeter walls are now correctly oriented in StandardMaps

v0.13.6
BugFix: 2×1 stairs now appear correctly in TileMaps

v0.13.5
BugFix: 2×1 stairs now do not appear in front of doors
Removed some leftover debug logs

v0.13.4
Fixed bug with stairs being created in corridors
Fixed bug with >2 storeys loosing their main type
Set default options to the simplest dungeon

v0.13.3
Fixed bug with door orientation
Fixed bug with columns being created in the void part in a StandardMap
Fixed bugs with column creation in TileMap

v0.13.2
Updated all the TileMap interpreters with the new modifications
Set minimum variation weight to 1
Refactored VirtualMap and MapInterpreter (and others)
Added all defaults

v0.13.1
Added Ladder2 cell type, a ladder (or stairs) of two tiles
Modified MetricLocation to report the storeys number
modified all interpreters to work with this modification
Added subtypes to VirtualCell, for multiple typing checks (they work like tags, basically, and CreateObjects should be called for each tag!)
this allows for multiple ‘things’ to be placed in a single cell by the interpreter. For example: ladders
Reworked map interpreter to work in two phases:
phase 1: the map is actually interpreted, changing the cell types (and subtypes) as needed (much like shader code)
phase 2: the current interpreted map is ‘rendered’ through CreateObject
Completely refactored Map Interpreters:
the interpreters now function similarly (easier code manteinance)
they now work in phases: Read and Build
they do not longer need Width, Height or Initialise
they work on the whole array of virtual maps
support for multiple types in a cell, while retaining old support (main type)
easier manteinance (most code is in the abstract map interpreter now
easier addition of new cell types
Added ‘Perimeter’ option and two new cell types: ‘PerimeterWall’ and ‘PerimeterColumn’

v0.13.0 – Multistorey update!
Added option for multiple storeys
Added a new cell type, ‘Ladder’ (it is placed in the storey below and reaches the storey above)
Added option for 2D or 3D map type
added warning and disabled generation if a 2D physical map is used with a 3D map type
Added default Ladder

v0.12.10
Added various warnings when necessary prefabs are missing
Stop generation if necessary prefabs are missing
Added guards on existence of ‘physicalMap’ when destroying it to avoid null references

v0.12.9
Fixed ‘Randomize Orientations’: if disabled, all tiles will have the same orientation. If enabled, the orientation will be randomized
‘Advanced Tiles’ renamed to ‘Directional Tiles’
‘Randomize Orientations’ is now hidden to the user if ‘Directional Tiles’ is enabled
‘Door Columns’ now renamed to ‘Passage Columns’. They are now available even if ‘Draw Doors in Passages’ is Disabled
Fixed defaults of ‘Sprites2DPhysicalMapBehaviour’


1. Installation

If you're going to use the 2DToolKit integration feature, you need to import the 2DToolKit plugin and then double-click on the Daedalus_2DToolKit_Integration package. You can find it in the Daedalus_Import folder.


2. Documentation

You can find the full documentation at this link: http://daedalus.artskillz.net/daedalus-documentation


3. Help and Feedback

If you need help with Daedalus or if you want to give us your precious feedback, please refer to the Q&A page at this link: http://daedalus.artskillz.net/questions


4. Videos and tutorials

You can check out some videos and tutorials here: http://daedalus.artskillz.net/videos


If you like Daedalus, please consider writing a review on the AssetStore and maybe give us a few stars, that would help us a lot! ;)

Also, let us know how you’re using Daedalus in your game. We’d love to see Daedalus in action in other people’s games!

