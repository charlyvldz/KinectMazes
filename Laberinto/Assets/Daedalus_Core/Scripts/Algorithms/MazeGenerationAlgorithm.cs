
abstract public class MazeGenerationAlgorithm {
	public void Start(VirtualMap map){
		// Pick a random cell and start from there
		CellLocation starting_location = map.PickRandomUnvisitedLocation();
		Start (map,starting_location);
	}
	abstract public void Start(VirtualMap map, CellLocation starting_location);
}
