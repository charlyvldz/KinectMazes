using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomGenerator
{
	private int maxH;
	private int maxW;
	
	private int minH;
	private int minW;

	private void CheckDimensions (VirtualMap map)
	{
		maxH = GeneratorValues.maxRoomHeight;
		maxW = GeneratorValues.maxRoomWidth;
		minH = GeneratorValues.minRoomHeight;
		minW = GeneratorValues.minRoomWidth;

		int i = 0;
		while ((maxH > minH || maxW > minW)&& maxH*maxW*GeneratorValues.maxRooms > ((map.Width -1)/2)*((map.Height-1)/2)) {
			if (i % 2 == 0 && maxH > minH)	{
				maxH--;
				Debug.LogWarning("Map is too small. Decreasing max room height to " + maxH);
			}
			else {
				maxW--;
				Debug.LogWarning("Map is too small. Decreasing max room width to " + maxW);
			}
			i++;

			if (maxH == 2 && minH == 2) break;
		}
	}

	public List<VirtualRoom> CreateRooms (VirtualMap map)
	{
		int roomNumber = DungeonGenerator.Random.Instance.Next (GeneratorValues.minRooms, GeneratorValues.maxRooms);
		List<VirtualRoom> rooms = new List<VirtualRoom> ();
		List<CellLocation> usedLocations = new List<CellLocation>();

		// Ensure that the total area is compatible
		CheckDimensions (map);
		for (int n=0; n<roomNumber; n++) {
			int width 	= DungeonGenerator.Random.Instance.Next (minW, maxW);
			int height 	= DungeonGenerator.Random.Instance.Next (minH, maxH);
			
			VirtualRoom r = new VirtualRoom (width, height, new CellLocation (0, 0));
			
			PickBestRoomLocation(map,r,n);
//			PickRandomLocation(map,r);
			
			if (r.leftCorner != new CellLocation(-1,-1)){
//				Debug.Log ("Creating a room: w:"+width +" H:"+height +" location:"+r.leftCorner);
				rooms.Add(r);
				for (int i = 0; i < r.Width; i++) {
					for (int j = 0; j < r.Height; j++) {
						CellLocation l = new CellLocation (r.leftCorner.x + 2 * i, r.leftCorner.y + 2 * j);
						
						// Do not add to this room's cells if it already belongs to another room
						if (usedLocations.Contains(l)) {
//							Debug.Log (l + " already belongs to a room!");
							continue;
						}
						
						VirtualCell passageCell;

						// Set top passage to empty
						passageCell = map.GetCell(l.x, l.y+1);
						if (!passageCell.IsRoomWall()){	// May belong to another room already
							passageCell.Type = VirtualCell.CellType.EmptyPassage;
							if (l.y + 2 <= r.leftCorner.y + 2 * (r.Height - 1)) {
								map.connectCells (l, new CellLocation (l.x, l.y + 2));
							}
						}

						// Set right passage to empty
						passageCell = map.GetCell(l.x+1,l.y);
						if (!passageCell.IsRoomWall()){	// May belong to another room already
							passageCell.Type = VirtualCell.CellType.EmptyPassage;
							if (l.x + 2 <= r.leftCorner.x + 2 * (r.Width - 1)) {
								map.connectCells (l, new CellLocation (l.x + 2, l.y));
							}
						}

						// Other passages are walls
						if (i == r.Width - 1) {
							map.cells [l.x + 1, l.y].Type = VirtualCell.CellType.RoomWall;
						}
						if (i == 0) {
							map.cells [l.x - 1, l.y].Type = VirtualCell.CellType.RoomWall;
						}
						if (j == r.Height - 1) {
							map.cells [l.x, l.y + 1].Type = VirtualCell.CellType.RoomWall;
						}
						if (j == 0) {
							map.cells [l.x, l.y - 1].Type = VirtualCell.CellType.RoomWall;
						}


						map.AddRoomCell (l);
						
						r.cells.Add (l);
						usedLocations.Add(l);
					}
				}
			}
			
		}
		return rooms;
		
	}
	
	private void PickBestRoomLocation(VirtualMap map, VirtualRoom r, int roomNumber){
		// Traverse all floor cells checking for the best position for a room
		int best_score = 1000000;
		int current_score;
		List<CellLocation> best_locations = new List<CellLocation>();
		List<CellLocation> locations = new List<CellLocation> (map.floorCells);
		foreach(CellLocation map_l in locations){
			r.leftCorner = map_l;
			if (isRoomLocationValid(map,r)){
				current_score = 0;	 // Lower is better
				for (int i = 0; i < r.Width; i++) {
					for (int j = 0; j < r.Height; j++) {
						CellLocation possible_room_l = new CellLocation (r.leftCorner.x + 2 * i, r.leftCorner.y + 2 * j);
//						Debug.Log("Possible room l: " + possible_room_l);
											 
						// Check for corridor vicinity
						if (map.IsRock(possible_room_l) && map.HasAdjacentFloor(possible_room_l)) current_score += 1;
						 
						// Check for floor overlap
						if (map.IsFloor(possible_room_l)) current_score += 3;
						
						// Check for room overlap
						if (map.IsRoomFloor(possible_room_l)) current_score += 100;

						// If multi-storey, the first room should be placed above another room!
//						if (roomNumber == 0 && !belowMap.isRoomFloor(possible_room_l)) current_score += 5;
					
						// TODO: may be more efficient to exit now if the score is already low enough!
					}
				}
				
				if (current_score == 0) continue; // Zero is not a valid score, as it means the room is isolated
				
				if (current_score == best_score){
					best_locations.Add(map_l);	
				} else if (current_score < best_score){
					best_score = current_score;
					best_locations.Clear();
					best_locations.Add(map_l);
				}
			}
		}

		if (best_locations.Count == 0) r.leftCorner = new CellLocation(-1,-1);
		else r.leftCorner = best_locations[DungeonGenerator.Random.Instance.Next (0, best_locations.Count - 1)];
		
	}
	
	private void PickRandomRoomLocation(VirtualMap map, VirtualRoom r){
		// Pick a random valid Location
		List<CellLocation> locations = new List<CellLocation> (map.floorCells);
		do {
			int index = DungeonGenerator.Random.Instance.Next (0, locations.Count - 1);
			CellLocation l = locations [index];
			r.leftCorner = l;
			locations.Remove (l);
		} while(locations.Count>0 && !isRoomLocationValid(map,r));
	}
	
	public bool isRoomLocationValid (VirtualMap map, VirtualRoom r)
	{
		// A room location is valid if it is in bounds 
		for (int i = 0; i < r.Width; i++) {
			for (int j = 0; j < r.Height; j++) {
				CellLocation l = new CellLocation (r.leftCorner.x + 2 * i, r.leftCorner.y + 2 * j);
				if (!map.floorCells.Contains (l))// || map.visitedCells.Contains (l))
					return false;
			}
		}
		return true;
	}
	
	// Create doors
	public void CreateDoors (VirtualMap map, VirtualRoom r){
		List<CellLocation> borders = new List<CellLocation> ();
		
		// Examine borders, create a list of border floors
		for (int i = 0; i < r.Width; i++) {
			for (int j = 0; j < r.Height; j++) {
				if (i == 0 || j == 0 || i == r.Width - 1 || j == r.Height - 1) {
					CellLocation l = new CellLocation (r.leftCorner.x + 2 * i, r.leftCorner.y + 2 * j);
					borders.Add (l);
				}
			}
		}

		// Create doors at the borders
		CellLocation target_passage;
		foreach(CellLocation l in borders){
			// Create the door in the wall passages
			foreach(VirtualMap.DirectionType dir in map.directions){
				target_passage = map.GetNeighbourCell (l, dir);
				if (map.GetCell(target_passage).IsWall())
					CheckDoorCreation(map, r, l, dir);
			}
		}
	}
	
	/*
	 * Control if it can open a door in that direction. Open it if possible
	 */
	private bool CheckDoorCreation (VirtualMap map, VirtualRoom r, CellLocation start_floor, VirtualMap.DirectionType direction)
	{
		bool result = false;
		CellLocation end_floor = map.GetTargetLocation (start_floor, direction);
		CellLocation passage = map.GetNeighbourCell (start_floor, direction);
		if (end_floor.isValid() && !map.GetCell(end_floor).IsRock()){
			if (map.roomCells.Contains (end_floor)) {
				// We are connecting to a room (only once then)
				if (DungeonGenerator.Random.Instance.Next(0,100)>GeneratorValues.doorsDensityModifier 
						&& r.IsConnectedToARoomAt(end_floor)) return result;
				
				// Update the connected room
				foreach (VirtualRoom tr in map.rooms) {
					if (tr.containsLocation (end_floor)) {
						tr.AddDoorToAnotherRoom(passage);
						r.AddDoorToAnotherRoom(passage);
						tr.connectedRooms.Add(r);
						r.connectedRooms.Add(tr);
						break;
					}
				}	
				
			} else {
				// We are connecting to the corridor
				// We need one door for each corridor segment that has been separated out by this room
				//		For now, we also create a door if the ending floor is a dead end, effectively removing it!
				//		Also, if the ending floor is a rock, which can happen if this room blocks out single floor cells!
				if(DungeonGenerator.Random.Instance.Next(0,100) > GeneratorValues.doorsDensityModifier
					&& r.IsConnectedToCorridor() 
					&& !map.IsDeadEnd(end_floor) && !map.IsRock(end_floor) ) return result;
				
				r.corridorExit++;
				r.corridorDoors.Add (passage);
			}
			
			map.GetCell(passage).Type = VirtualCell.CellType.Door;
			map.GetCell(passage).Orientation = direction;

			map.connectCells (start_floor, end_floor);
			result = true;

		}
		return result;
	}

}
