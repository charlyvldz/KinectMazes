using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


// This determines the overall shape of the map
[System.Serializable]
public class VirtualMap 
{
	private int width;	// The map Width
	private int height;	// The map Height
	public VirtualCell[,] cells;	// The cells in this map
	
	public List<VirtualRoom> rooms;
	public int storey_number;
	
	public enum DirectionType { North, South, East, West, None };	// Directions
	public int nDirections{get; private set;}
	public DirectionType[] directions;

	public readonly List<CellLocation> visitedCells = new List<CellLocation>();				// Visited cells
	public readonly List<CellLocation> visitedAndBlockedCells = new List<CellLocation>(); 	// Visited and blocked (in all 4 directions) cells
	public readonly List<CellLocation> floorCells = new List<CellLocation>();				// Walkable cells
	public readonly List<CellLocation> borderCells = new List<CellLocation>(); 				// Walls, limits and passage cells
	public readonly List<CellLocation> noneCells = new List<CellLocation>();				// Parts between walls (where columns may be placed)
	
	public CellLocation start	=		new CellLocation(-1,-1);
	public CellLocation end		=		new CellLocation(-1,-1);
	public CellLocation root		=		new CellLocation(-1,-1);	// Cell used for distance computations
	
	public  List<CellLocation> roomCells = new List<CellLocation>();

	// Constructor
	public VirtualMap (int width, int height, int storey_number = 0)
	{
		this.width = 2*width +1;
		this.height = 2*height +1;
		this.storey_number = storey_number;
		cells = new VirtualCell[this.width, this.height];
		SetupDirections(new DirectionType[4]{DirectionType.North,DirectionType.East,DirectionType.South,DirectionType.West});
		Init();
		
	}
	public int Width
	{
		get {return width;}
	}
	public int Height
	{
		get {return height;}
	}
	public int ActualWidth
	{
		get {return (width-1)/2;}
	}
	public int ActualHeight
	{
		get {return (height-1)/2;}
	}
	
	private void SetupDirections(DirectionType[] _directions){
		this.directions = _directions;
		this.nDirections = this.directions.Length;
	}

	public DirectionType GetDirectionClockwise(DirectionType _dir, float delta){
		return this.directions[(int)Mathf.Repeat((int)_dir+delta,nDirections-1.0f)];
	}

	public DirectionType GetDirectionOpposite(DirectionType _dir){
		return this.GetDirectionClockwise(_dir, this.nDirections/2);
	}
	
	private void Init()
	{	
		// Initialise the virtual map with interleaved cells floors and walls, with Ninguno cells between walls
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				CellLocation location = new CellLocation(i, j);
				cells[i,j] = new VirtualCell(false,location);
				if(i%2==0)
				{
					if(j%2==0){
						cells[i,j].Type=VirtualCell.CellType.None;
						noneCells.Add(location);
					}
					else
					{
						cells[i,j].Type=VirtualCell.CellType.CorridorWall;
						borderCells.Add(location);
					}
				}
				else
				{
					if(j%2==0)
					{	
						cells[i,j].Type=VirtualCell.CellType.CorridorWall;
						borderCells.Add(location);
					}
					else
					{
						cells[i,j].Type=VirtualCell.CellType.CorridorFloor;
						floorCells.Add(location);
					}
				}
			}
		}
	}
	
	
	public CellLocation[] GetAllNeighbours(CellLocation location){
		CellLocation[] neighs = new CellLocation[4];
		neighs[0] = GetNeighbourCell(location,DirectionType.North);
		neighs[1] = GetNeighbourCell(location,DirectionType.East);
		neighs[2] = GetNeighbourCell(location,DirectionType.South);
		neighs[3] = GetNeighbourCell(location,DirectionType.West);
		return neighs;
	}
	
	
	public CellLocation[] GetAllSameNeighbours(CellLocation location){
		CellLocation[] neighs = new CellLocation[4];
		neighs[0] = GetNeighbourCellOfSameType(location,DirectionType.North);
		neighs[1] = GetNeighbourCellOfSameType(location,DirectionType.East);
		neighs[2] = GetNeighbourCellOfSameType(location,DirectionType.South);
		neighs[3] = GetNeighbourCellOfSameType(location,DirectionType.West);
		return neighs;
	}
	
	// Return the next virtual cell in the given direction (for floors, these are walls. for walls, these are floors)
	public CellLocation GetNeighbourCell(CellLocation location, DirectionType direction){
		return GetNeighbourCellAtStep(location,direction,1);
	}
	
	// Return the next virtual cell of the same type in the given direction (i.e. floor for floor, wall for wall)
	public CellLocation GetNeighbourCellOfSameType(CellLocation location, DirectionType direction){
		return GetNeighbourCellAtStep(location,direction,2);
	}

	public CellLocation GetNeighbourCellAtStep(CellLocation location, DirectionType direction, int step){
		switch(direction)
		{
		case DirectionType.South:
			return new CellLocation(location.x, location.y - step);
		case DirectionType.West:
			return new CellLocation(location.x -step, location.y);
		case DirectionType.North:
			return new CellLocation(location.x, location.y + step);
		case DirectionType.East:
			return new CellLocation(location.x +step, location.y);
		default:
			throw new InvalidOperationException();
		}
	}

	
    public bool LocationIsOutsideBounds(CellLocation location)
    {
        return ((location.x < 0) || (location.x >= Width) || (location.y < 0) || (location.y >= Height));
    }
	
	public bool LocationIsInPerimeter(CellLocation location){
		return ((location.x == 0) || (location.x == Width-1) || (location.y == 0) || (location.y == Height-1));
	}

	
	public void CreateRooms()
	{
		RoomGenerator roomGenerator=new RoomGenerator();
		rooms=roomGenerator.CreateRooms(this);
	}
	// Mark this cell as visited
	public void FlagCellAsVisited(CellLocation location)
    {
        if (LocationIsOutsideBounds(location)) throw new ArgumentException("Location is outside of Map bounds", "location");
        if (this.cells[location.x, location.y].visited) throw new ArgumentException("Location is already visited", "location");

        this.cells[location.x, location.y].visited = true;
        visitedCells.Add(location);
    }
	//add a room cell to the map
	public void AddRoomCell(CellLocation l)
	{
		roomCells.Add(l);
		this.cells[l.x,l.y].Type=VirtualCell.CellType.RoomFloor;
	}
	
	// Did we process all the cells in the map yet?
	public bool AllCellsVisited
    {
        get { return visitedCells.Count == ((Width -1)/2)*((Height-1)/2); }
    }
	
	// Pick a random cell in the map and mark it as Visited
	public CellLocation PickRandomUnvisitedLocation(){
		List<CellLocation> locations = new List<CellLocation>(floorCells);
		foreach (CellLocation l in visitedCells) locations.Remove(l);
		
		int index= DungeonGenerator.Random.Instance.Next(0,locations.Count-1);
		CellLocation location = locations[index];

		return location;
	}

	public void MarkAsVisited(CellLocation l){
		VirtualCell cell = GetCell(l);
		cell.visited = true;
		visitedCells.Add(l);
	}

	// Does this cell have any neighbour in a certain direction?
	public bool HasAdjacentCellInDirection (CellLocation location, DirectionType direction)
	{
		if (LocationIsOutsideBounds(location)) return false;
		CellLocation l = GetNeighbourCellOfSameType(location, direction);
		return !LocationIsOutsideBounds(l);
	}
	// Is this cell's neighbour marked as Visited?
	public bool AdjacentCellInDirectionIsVisited(CellLocation location, DirectionType direction)
    {
        if (HasAdjacentCellInDirection(location, direction))
		{
	        switch(direction)
	        {
	            case DirectionType.South:
	                return this.cells[location.x, location.y - 2].visited;
	            case DirectionType.West:
	                return this.cells[location.x-2, location.y].visited;
	            case DirectionType.North:
	                return this.cells[location.x, location.y + 2].visited;
	            case DirectionType.East:
	                return this.cells[location.x + 2, location.y].visited;
	            default:
	                throw new InvalidOperationException();
	        }
		}
		
		return false;
    }
	// Get a random visited cell
	public CellLocation GetRandomVisitedCell(CellLocation location)
    {
		List<CellLocation> tempCells = new List<CellLocation>(visitedCells);
		
		//tempCells.Remove(location);
		
		// NOTE: when does visistedAndBlockedCells get populated???
		foreach (CellLocation l in visitedAndBlockedCells)
		{
			tempCells.Remove(l);
		}
	    foreach (CellLocation l in roomCells)
		{
			tempCells.Remove(l);
		}
		
        if (tempCells.Count == 0) return new CellLocation(-1,-1);
		
        int index = DungeonGenerator.Random.Instance.Next(0, tempCells.Count -1);

        return tempCells[index];   
    }
	
	// Create a corridor between one cell and another, digging the passage in-between
	public CellLocation CreateCorridor(CellLocation location, VirtualMap.DirectionType direction)
    {
		VirtualCell cell;
        CellLocation target_location = GetTargetLocation(location, direction);
		cell = this.GetCell(target_location);
//		cell.Orientation = direction;	// This is removed, because the directional tiles will take care of this instead

		CellLocation connection_location = GetNeighbourCell(location, direction);
		cell = this.GetCell(connection_location);
		cell.Type = VirtualCell.CellType.EmptyPassage;
//		cell.Orientation = direction;		// Setting this would just change randomly the direction of some walls in tilemaps. This is removed for now!

		return target_location;
    }
	// Get target location from a valid cell/direction
	public CellLocation GetTargetLocation(CellLocation location, VirtualMap.DirectionType direction)
    {
        if (!HasAdjacentCellInDirection(location, direction)) 
			return new CellLocation(-1,-1);
		else
			return GetNeighbourCellOfSameType(location, direction);
        
    }
	
	// A dead end cell has one and only one direction free for walking (i.e. there is no wall there -> empty)
	public bool IsDeadEnd(CellLocation l)
	{
		int emptyCount = 0;
		
		CellLocation[] locs = GetAllNeighbours(l);
		foreach(CellLocation n_l in locs){
			if (this.cells[n_l.x,n_l.y].Type == VirtualCell.CellType.EmptyPassage) {
				emptyCount++;
//				Debug.Log("For loc " + l + " neigh " + n_l + " is empty!");	
			}
		}
		
		return emptyCount == 1;	
	}
	
	// A rock is an unreachable place surrounded by walls
	public bool IsRock(CellLocation l) 
	{
		int emptyCount = 0;
		
		CellLocation[] locs = GetAllNeighbours(l);
		foreach(CellLocation n_l in locs){
			if (this.cells[n_l.x,n_l.y].Type == VirtualCell.CellType.EmptyPassage) {
				emptyCount++;
//				Debug.Log("For loc " + l + " neigh " + n_l + " is empty!");	
			}
		}
        
		if(emptyCount==0 && this.cells[l.x,l.y].connectedCells.Count>0){
//			Debug.Log ("Not a rock, just an isolated floor cell!");
			return false;
		}
		return emptyCount == 0;
	}

	// TODO: remove those
	public bool IsFloor(CellLocation l){
		return this.GetCell(l).IsFloor();
	}
	
	public bool IsRoomFloor(CellLocation l){
		return this.GetCell(l).Type == VirtualCell.CellType.RoomFloor;	
	}

	public bool CellsAreInTheSameRoom(CellLocation l1, CellLocation l2){
		VirtualRoom room1 = null, room2 = null;
		foreach(VirtualRoom room in this.rooms){
			if (room.containsLocation(l1)) {
				room1 = room;
//				Debug.Log ("ROOM 1: " + room1);
				break;
			}
		}
		foreach(VirtualRoom room in this.rooms){
			if (room.containsLocation(l2)) {
				room2 = room;
//				Debug.Log ("ROOM 2: " + room2);
				break;
			}
		}

//		if (room1 == room2) Debug.Log ("SAME ROOM!");
//		else Debug.Log ("NOT SAME!");

		return room1 == room2;
	}

	
	public bool HasAdjacentFloor(CellLocation l){
		CellLocation[] locs = GetAllSameNeighbours(l);
		foreach(CellLocation n_l in locs){
//			Debug.Log(n_l);
			if (!LocationIsOutsideBounds(n_l) && GetCell(n_l).IsFloor()) {
				return true;
			} 
		} 
		return false;
	}

	public bool HasAdjacentDoor(CellLocation l){
		foreach(CellLocation n_l in GetAllNeighbours(l)){
			if (!LocationIsOutsideBounds(n_l) && GetCell(n_l).IsDoor()) {
				return true;
			} 
		} 
		return false;
	}

	
	
	public VirtualCell GetCell(CellLocation l){
		return this.cells[l.x,l.y];	
	}
	public VirtualCell GetCell(int x, int y){
		return this.cells[x,y];
	}
	
	
	public IEnumerable<CellLocation> DeadEndCellLocations
    {
        get
		{
			//NOTE: This creates an enumerator, so that if a location becomes a dead end during the following removal it will be updated automatically (if following the order of the grid!)
			foreach(CellLocation l in floorCells)
                if (IsDeadEnd(l)) {
					//Debug.Log("Location " + l + " is a dead end!");
					yield return new CellLocation(l.x, l.y);
				}
        }
    }
	
	
	public IEnumerable<CellLocation> WalkableLocations
    {
        get
		{
            foreach(CellLocation l in floorCells)	// Floor cells may also be rocks!
                if (cells[l.x,l.y].IsFloor()) yield return new CellLocation(l.x, l.y);
        }
    }
	public IEnumerable<CellLocation> RoomWalkableLocations {
		get
		{
			foreach(CellLocation l in roomCells) yield return new CellLocation(l.x, l.y);
		}
	}
	
	public DirectionType CalculateDeadEndCorridorDirection(CellLocation location)
    {
	    if (!IsDeadEnd(location)) throw new InvalidOperationException();
	
	    if (this.cells[location.x, location.y-1].Type == VirtualCell.CellType.EmptyPassage) return DirectionType.South;
	    if (this.cells[location.x, location.y+1].Type == VirtualCell.CellType.EmptyPassage) return DirectionType.North;
	    if (this.cells[location.x-1, location.y].Type == VirtualCell.CellType.EmptyPassage) return DirectionType.West;
	    if (this.cells[location.x+1, location.y].Type == VirtualCell.CellType.EmptyPassage) return DirectionType.East;
	
	    throw new InvalidOperationException();
   }
	public void CreateWall(CellLocation location, DirectionType direction)
    {
		CellLocation connection=GetNeighbourCell(location, direction);
		
		if(!(this.cells[connection.x,connection.y].Type==VirtualCell.CellType.RoomWall))
		 this.cells[connection.x,connection.y].Type=VirtualCell.CellType.CorridorWall;
				
		// Remove the cell from his father
		CellLocation target = GetTargetLocation(location, direction);		
		this.cells[target.x,target.y].connectedCells.Remove(location);
		this.cells[location.x,location.y].connectedCells.Remove(target);

    }
	//is the cell in that direction a Rock?
	public bool AdjacentCellInDirectionIsRock(CellLocation location, DirectionType direction)
    {
        if (HasAdjacentCellInDirection(location, direction))
		{
			CellLocation l = GetNeighbourCellOfSameType(location, direction);
			return IsRock(l);
		}
		return true;
    }

	// Check if two locations are the same
	public bool CompareLocations (CellLocation location1, CellLocation location2)
	{
		return (location1.x == location2.x && location1.y == location2.y);
	}

	
	// Can we put a door around this cell?
    public bool IsDoorable(CellLocation l)
	{
//			Debug.Log("Is " + l + " doorable?");
			// Cannot already have a door here
//			if ((!LocationIsOutsideBounds(new Location(l.x-1,l.y)) && this.cells[l.x-1,l.y].Type == VirtualCell.CellType.Door) || 
//			(!LocationIsOutsideBounds(new Location(l.x+1,l.y)) && this.cells[l.x+1,l.y].Type == VirtualCell.CellType.Door)  || 
//			(!LocationIsOutsideBounds(new Location(l.x,l.y-1)) &&this.cells[l.x,l.y-1].Type == VirtualCell.CellType.Door)  || 
//			(!LocationIsOutsideBounds(new Location(l.x,l.y+1)) &&this.cells[l.x,l.y+1].Type == VirtualCell.CellType.Door) ) {
//				Debug.Log("No connections to floors here!");
//				return false;
//			}
//			else
//			{
				// Not a rock
//				Debug.Log("Is dead end: " + isDeadEnd(l));
//				Debug.Log("Is there a rock? " + isRock(l));
			return !IsRock(l);
//			}
            
	}

	public bool IsInRoom(CellLocation l){
		if (rooms == null || rooms.Count == 0) return false;
		foreach(VirtualRoom room in rooms){
			if (room.isInRoom(l)){
				return true;
			}
		}
		return false;
	}
	
	public bool IsOnRoomBorder(CellLocation l){
		if (rooms == null || rooms.Count == 0) return false;
		foreach(VirtualRoom room in rooms){
			if (room.isInBorder(l)){
				return true;
			}
		}
		return false;
	}



	// Returns true if this cell can be removed from the map (i.e. not shown)
	public bool IsRemovable(CellLocation l, bool drawCorners = true)
	{
		VirtualCell cell = this.GetCell(l);
		if(cell.IsWall() || cell.IsNone() || cell.IsRock()){// || cell.IsColumn()){	// May be removed
			int validNeigh = 0;
			int wallCount = 0;
			int emptyCount = 0;
			
			CellLocation n;
			foreach(DirectionType dir in directions){
				n = GetNeighbourCell(l,dir);
				if(!LocationIsOutsideBounds(n)){
					validNeigh++;
					VirtualCell neigh_cell = GetCell(n);
//					if (l.x == 3 && l.y == 0) Debug.Log (neigh_cell.Type);
					if (neigh_cell.IsWall()) wallCount++;
					else if (neigh_cell.IsNone() || neigh_cell.IsRock()) emptyCount++; 
				}
			}    
			// Show corners. Note that only Ninguno cells can be corners (surrounded by walls)
			if (drawCorners){
//				Debug.Log ("Cell " + l + " " +wallCount + " , " + emptyCount + " , " + validNeigh);
				if (cell.IsNone() && wallCount == validNeigh) {	
					// At least one neigh need not be removable as well for this to be a corner (and not an isolated Ninguno cell)
					foreach(DirectionType dir in directions){
						n = GetNeighbourCell(l,dir);
						bool neighRemovable = true;
						if(!LocationIsOutsideBounds(n)) neighRemovable =  IsRemovable(n,drawCorners);
						if (!neighRemovable) return false;
					}
					return true;
				} 
			}
			return wallCount + emptyCount == validNeigh;
		}
		return false;
	}
	public MetricLocation GetActualLocation(CellLocation l, int storey)
	{
		return new MetricLocation(l.x/2.0f,l.y/2.0f, storey);
	}




	/****************************
	 *  Connection and distance
	 ***************************/

	private void ResetCellsAsUnvisited(){
		foreach (VirtualCell c in this.cells) {
			c.visited = false;
			c.distance_from_root = 10000; // Initial large distance
		}
	}

	public void ComputeCellDistances(CellLocation startCellLocation = default(CellLocation)){
		// Computes all cell distances from a starting location
		this.ResetCellsAsUnvisited ();
		List<CellLocation> unvisited_locations = new List<CellLocation>(this.WalkableLocations);
		CellLocation currentLocation = startCellLocation;
		if (startCellLocation == default(CellLocation)) currentLocation = unvisited_locations[0];
		int current_distance = 0;
		GetCell (currentLocation).distance_from_root = 0;
		this.root = currentLocation;	// We set this as the root
		int it = 0;
		while (true) {
//			Debug.Log ("CHECKING LOC" + currentLocation);
			VirtualCell current_cell = GetCell (currentLocation);
			CellLocation[] neighbour_locations = this.GetAllSameNeighbours (currentLocation);
			current_distance = current_cell.distance_from_root + 1;
//			Debug.Log ("NEW DIST: " + current_distance);
			foreach (CellLocation nl in neighbour_locations) {
				if (LocationIsOutsideBounds(nl)) continue;
				if (!CanConnectTo(currentLocation,nl)) continue;
				VirtualCell nc = this.GetCell (nl);
//				Debug.Log ("NEIGH LOC" + nl);
				if (!nc.visited) {
					int last_distance = nc.distance_from_root;
					nc.distance_from_root = (current_distance <= last_distance) ? current_distance : last_distance;
//					Debug.Log ("SET DIST " + nc.distance_from_root);
				}
			}
			current_cell.visited = true;
			unvisited_locations.Remove (currentLocation);
			if (unvisited_locations.Count == 0) break;	// We finished!

			// Choose the next cell to use
			int min_distance = 10000;
			foreach (CellLocation cl in unvisited_locations) {
				if (GetCell (cl).distance_from_root < min_distance) {
					currentLocation = cl;
					min_distance = GetCell (cl).distance_from_root;
				}
			}
			it++;
			if (it>100) break;
		}
//		PrintDistances ();
	}

	public int GetMaximumDistance(){
		int max_distance = 0;
		foreach (CellLocation cl in WalkableLocations) {
			int walk_distance = GetWalkDistance(root,cl);
			if (walk_distance > max_distance) max_distance = walk_distance;
		}
		return max_distance;
	}

	public int GetWalkDistance(CellLocation s, CellLocation e){
		// TODO: this works by walking to the root and back!! Doens't consider other paths! Not good!
		return Mathf.Abs( GetCell (s).distance_from_root + GetCell (e).distance_from_root);
	}


	public bool CanConnectTo(CellLocation a, CellLocation b){
		// Only if there is not a wall in-between.
		// Works only for neighbours!
		DirectionType direction = GetDirectionBetween (a, b);
//		Debug.Log ("Direction between " + a + " and " + b + " is " + direction);
		CellLocation passage_cell_location = GetNeighbourCell (a, direction);
//		Debug.Log ("PASSAGE: " + passage_cell_location);
		bool canConnect = !GetCell(passage_cell_location).IsWall();
//		Debug.Log ("CAN CONNECT? " + canConnect);
		return canConnect;
	}

	public DirectionType GetDirectionBetween(CellLocation a, CellLocation b){
		// Works only for neighbours
		DirectionType type = DirectionType.None;
		if (a.x < b.x) type = DirectionType.East;
		else if (a.x > b.x) type = DirectionType.West;
		else if (a.y > b.y) type = DirectionType.South;
		else if (a.y < b.y) type = DirectionType.North;
		return type;
	}


	public CellLocation GetCellLocationFartherThan(IEnumerable<CellLocation> iterable_locations, float min_wanted_distance, float max_wanted_distance){
		float max_distance = 0;
		CellLocation start = this.root;
		CellLocation current_chosen_location = default(CellLocation);
		foreach (CellLocation cl in iterable_locations) {
			int current_distance = GetWalkDistance(start,cl);
			current_chosen_location = cl;
//			Debug.Log ("MIN: " + min_wanted_distance + "   MAX: " + max_wanted_distance);
//			Debug.Log ("current new max: " + current_distance);
			if (current_distance >= min_wanted_distance && current_distance <= max_wanted_distance) {
//				Debug.Log ("FOUND!");
				return current_chosen_location;	// Found it!
			}
		}
		// Couldn't find that distance!
		DebugUtils.Assert (true, "Couldn't find a cell distant " + min_wanted_distance + " from the starting cell! Returning a cell with distance " + max_distance);
		return current_chosen_location;
	}

	// DEPRECATED
	public void connectCells(CellLocation s, CellLocation e)
	{
//		Debug.Log("Connecting cell " + s + " and " + e);
		this.cells[s.x,s.y].connectedCells.Add(e);
		this.cells[e.x,e.y].connectedCells.Add(s);
	}
	public void disconnectCells(CellLocation s, CellLocation e)
	{
		this.cells[s.x,s.y].connectedCells.Remove(e);
		this.cells[e.x,e.y].connectedCells.Remove(s);
	}


	private int Min(int a, int b)
	{
		return (a<=b)?a:b;
	}



	public void Print(){
		Debug.Log(width + "X" + height);
		string s ="";
		for (int i = 0; i < this.width; i++){
			for (int j = 0; j < this.height; j++){	
				if (cells[i,j].IsFloor()){
					s+="o";
				} else if (cells[i,j].IsWall()){
					s+="|";	
				} else {
					s+="x";
				}
			}
			s +="\n";
		}
		Debug.Log(s);
	}

	
	public void PrintDistances(){
		// Prints the grid with distances on the walkable tiles
		Debug.Log(width + "X" + height);
		string s ="";
		for (int i = 0; i < this.width; i++){
			for (int j = 0; j < this.height; j++){	
				if (cells[i,j].IsFloor()){
					s += cells[i,j].distance_from_root;
				} else if (cells[i,j].IsRock()){
					s += "X";
				}
//				} else if (cells[i,j].IsWall()){
//					s+="|";	
//				} else {
//					s+="x";
//				}
			}
			s +="\n";
		}
		Debug.Log(s);
	}
}

