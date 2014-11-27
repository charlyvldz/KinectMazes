using System.Collections.Generic;
using System;
using UnityEngine;

public enum MazeGenerationAlgorithmChoice {HuntAndKill, RecursiveBacktracker}

public class VirtualMapGenerator 
{
	
	public uint lastUsedSeed = 0U;
	bool verbose = false;
	public void InitialiseSeed(bool useSeed, int seed){
		if (useSeed)	lastUsedSeed = DungeonGenerator.Random.Instance.Initialize(seed);
		else 			lastUsedSeed = DungeonGenerator.Random.Instance.Initialize();
	}

	public VirtualMap Generate (int width, int height, bool willCreateRooms, int storey_number, VirtualMap vmapBelow = null)
	{
		// Create a new map  
		VirtualMap map = new VirtualMap(width, height, storey_number);
		
		MazeGenerationAlgorithm algorithm;
		switch(GeneratorValues.algorithmChoice){
			case MazeGenerationAlgorithmChoice.HuntAndKill: algorithm = new HuntAndKillMazeGenerationAlgorithm(); break;
			case MazeGenerationAlgorithmChoice.RecursiveBacktracker: algorithm = new RecursiveBacktrackerMazeGenerationAlgorithm(); break;
			default: algorithm = new HuntAndKillMazeGenerationAlgorithm(); break;
		}
		if (vmapBelow == null) algorithm.Start(map);
		else algorithm.Start(map,vmapBelow.start); 

		Sparsify(map,vmapBelow);
		if (verbose) Debug.Log("Sparsified!");
		
		OpenDeadEnds(map);
		if (verbose) Debug.Log("Opened dead ends!");
				
		CreateRocks(map);
		if (verbose) Debug.Log("Added rocks!");
		
		if (willCreateRooms) {
			map.CreateRooms();
			if (verbose) Debug.Log("Added rooms!");
			
			CreateDoors(map);
			if (verbose) Debug.Log("Added doors!");
		}
		
		if (GeneratorValues.createStartAndEnd || GeneratorValues.multiStorey) CreateStartAndEnd(map,vmapBelow);

		return map;
	}
	
	// Sparsify the map by removing dead-end cells.
	public void Sparsify(VirtualMap map, VirtualMap vmapBelow)
	{
		// Compute the number of cells to remove as a percentage of the total number of cells in the map
        int noOfDeadEndCellsToRemove = (int) Math.Floor((decimal) GeneratorValues.sparsenessModifier / 100 * (map.ActualWidth*map.ActualHeight));
 		if (verbose) Debug.Log("Sparsify: removing  " + GeneratorValues.sparsenessModifier + "% i.e. " + noOfDeadEndCellsToRemove + " out of " + map.ActualWidth*map.ActualHeight + " cells");
 
		int noOfRemovedCells = 0;
		IEnumerable<CellLocation> deads;
		while(noOfRemovedCells < noOfDeadEndCellsToRemove)
		{
			// We sweep and remove all current dead ends
			deads = map.DeadEndCellLocations;

			int currentlyRemovedCells=0;
			foreach(CellLocation location in deads)
			{
				if (vmapBelow != null && location == vmapBelow.end) continue; // For multi-storey to work correctly, we do not remove the cell above the below's end
//				Debug.Log("Dead at " + location);
//				Debug.Log(map.CalculateDeadEndCorridorDirection(location));
				map.CreateWall(location, map.CalculateDeadEndCorridorDirection(location));
				currentlyRemovedCells++;
				if(++noOfRemovedCells == noOfDeadEndCellsToRemove) break;
			}
			if(currentlyRemovedCells==0) {
//				Debug.Log("We have no more dead ends!");
				break;	// No more dead endss
			} 
//			Debug.Log("We removed a total of " + noOfRemovedCells + " cells"); 
		}
	}
	
	// Open dead ends by linking them to rooms
	public void OpenDeadEnds(VirtualMap map)
    {
//		Debug.Log("DEAD END MOD: " + GeneratorValues.openDeadEndModifier);
		if (GeneratorValues.openDeadEndModifier == 0) return;
		
		IEnumerable<CellLocation> deads = map.DeadEndCellLocations;
        foreach (CellLocation deadEnd in deads)
        {
			if (DungeonGenerator.Random.Instance.Next(1,99) < GeneratorValues.openDeadEndModifier){
                CellLocation currentLocation = deadEnd;
//				int count=0;
                do
                {
                    // Initialize the direction picker not to select the dead-end corridor direction
                    DirectionPicker directionPicker = new DirectionPicker(map,currentLocation,map.CalculateDeadEndCorridorDirection(currentLocation));
//                    Debug.Log("We have a dead and " + directionPicker);
					VirtualMap.DirectionType direction = directionPicker.GetNextDirection(map,currentLocation);
//					Debug.Log("We choose dir " + direction);
					if (direction == VirtualMap.DirectionType.None)                          
						throw new InvalidOperationException("Could not remove the dead end!");
//						Debug.Log("Cannot go that way!");
                    else
	                    // Create a corridor in the selected direction
	                    currentLocation = map.CreateCorridor(currentLocation, direction);
//					count++;
                } while (map.IsDeadEnd(currentLocation) && currentLocation != deadEnd); // Stop when you intersect an existing corridor, or when you end back to the starting cell (that means we could not remove the dead end, happens with really small maps
//				Debug.Log("Dead end removed"); 
            }
        }
    } 
	
	public void CreateDoors(VirtualMap map){
		RoomGenerator roomG = new RoomGenerator();
		foreach(VirtualRoom r in map.rooms) roomG.CreateDoors(map,r);
	}
	
	public void CreateRocks(VirtualMap map){
		foreach (CellLocation c in map.visitedCells) {
			if(map.IsRock(c)) map.cells[(int)c.x,(int)c.y].Type=VirtualCell.CellType.Rock;
		}
	}
	
	public void CreateStartAndEnd(VirtualMap map, VirtualMap vmapBelow) {

		List<CellLocation> iterable_locations;
		bool foundStart = false;

		if (GeneratorValues.forceStartAndEndInRooms) iterable_locations = new List<CellLocation>(map.RoomWalkableLocations);	//  && map.rooms.Count > 0
		else iterable_locations = new List<CellLocation>(map.WalkableLocations);
		
		// Makes sure it is in some dead end, or in a room, if possible
		List<CellLocation> possible_start_locations = new List<CellLocation>();
		foreach(CellLocation l in iterable_locations){
			if (map.IsDeadEnd(l) || map.IsInRoom(l)) possible_start_locations.Add(l);
		}

		// If not possible, consider all locations equally
		if (possible_start_locations.Count == 0) possible_start_locations = iterable_locations;
		//		Debug.Log ("Possible start locations: " + possible_start_locations.Count);

		// TODO: Makes sure the start is on the perimeter 
//		foreach (CellLocation l in possible_start_locations) {
//			if (map.IsOnPerimeter(l) possible_start_locations
//		}

		if (vmapBelow == null){
			// Choose a random walkable cell as the starting point
			int index;
			index = DungeonGenerator.Random.Instance.Next(0,possible_start_locations.Count-1);
			if(index !=-1 && possible_start_locations.Count != 0){
				map.start = new CellLocation(possible_start_locations[index].x,possible_start_locations[index].y);
				foundStart = true;
			}
		} else {
			// Choose the cell above the below map's end as the starting point
			map.start = vmapBelow.end;
			foundStart = true;
		}

		if (foundStart){
//			Debug.Log ("CHOSEN START: " + map.start);
			// For this to work, we must compute the distance of all cells from the starting cell
			map.ComputeCellDistances (startCellLocation:map.start);

			// Choose a cell at a certain distance from the starting point as the ending point
			map.end = map.GetCellLocationFartherThan(iterable_locations, GeneratorValues.minimumDistanceBetweenStartAndEnd/100.0f * map.GetMaximumDistance(), GeneratorValues.maximumDistanceBetweenStartAndEnd/100.0f * map.GetMaximumDistance());
		}

		DebugUtils.Assert(foundStart, "Cannot find a suitable entrance!");
	}	

}
