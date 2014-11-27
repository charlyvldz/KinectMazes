using UnityEngine;
using System.Collections;

public class GeneratorValues 
{
	public static int seed=0;
	public static MazeGenerationAlgorithmChoice algorithmChoice;
	public static int directionChangeModifier=0;
	public static int sparsenessModifier=0;
	public static int openDeadEndModifier=0;
	
	public static int minRooms=2;
	public static int maxRooms=8;
	public static int minRoomWidth=3;
	public static int maxRoomWidth=7;
	public static int minRoomHeight=3;
	public static int maxRoomHeight=7;
	public static int doorsDensityModifier=0;
	
	public static bool createStartAndEnd = true;
	public static bool forceStartAndEndInRooms = true;
	
	public static bool addCeilingToCorridors = false;
	public static bool addCeilingToRooms = false;
	
	public static bool drawRocks   = false;
	public static bool drawWallCorners = false;
	public static bool multiStorey = false;
	public static int numberOfStoreys = 1;
	public static float minimumDistanceBetweenStartAndEnd = 0;
	public static float maximumDistanceBetweenStartAndEnd = 100;

}
