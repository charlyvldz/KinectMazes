using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum MapDimensionsType{TWO_DEE, THREE_DEE}

public class GeneratorBehaviour : MonoBehaviour
{
	public static int MAX_SIZE = 16;
	
	public int seed = 0;
	public bool useSeed = false;
	
	public int MapWidth = 2;
	public int MapHeight = 2;
	
	public MazeGenerationAlgorithmChoice algorithmChoice;
	
	// These should be percents
	public int directionChangeModifier = 0;		
	public int sparsenessModifier = 0;
	public int openDeadEndModifier = 0;
	
	public bool createRooms = false;
	public int minRooms;
	public int maxRooms;
	public int minRoomWidth;
	public int maxRoomWidth;
	public int minRoomHeight;
	public int maxRoomHeight;
	
	public int doorsDensityModifier = 0;
	
	public bool createStartAndEnd = false;
	public bool forceStartAndEndInRooms = false;
	public float minimumDistanceBetweenStartAndEnd;
	public float maximumDistanceBetweenStartAndEnd;

	DateTime preDate;
	DateTime postDate;
	
	[SerializeField]
	GameObject rootMapGo = null;

	[SerializeField]
	public PhysicalMap physicalMap;
	
	[SerializeField]
	public PhysicalMapBehaviour physicalMapBehaviour;
	
	[SerializeField]
	public MapInterpreterBehaviour interpreterBehaviour;

	[SerializeField]
	public bool isCurrentlyGenerating = false;
	
	public bool printTimings = false;

	// Multiple storeys
	public int numberOfStoreys = 1;
	public bool multiStorey = false;

	public MapDimensionsType mapDimensionsType; 

	private VirtualMap[] virtualMaps;	// One for each storey

	public uint lastUsedSeed;

	public void Generate ()
	{
		isCurrentlyGenerating = true;
		// Make sure we have a PhysicalMapBehaviour component
		physicalMapBehaviour = gameObject.GetComponent<PhysicalMapBehaviour>();
		if (physicalMapBehaviour == null) {
			Debug.LogError("You need to attach a PhysicalMapBehaviour to this gameObject to enable generation!",this);
			return;
		}
		
		// Make sure we have a MapInterpreterBehaviour component
		interpreterBehaviour = gameObject.GetComponent<MapInterpreterBehaviour>();
		if (interpreterBehaviour == null) {
			Debug.LogError("You need to attach a MapInterpreterBehaviour to this gameObject to enable generation!",this);
			return;
		}
		


		
		// Remove the existing map 
		if (rootMapGo != null){
			if (physicalMap != null) {
				physicalMap.CleanUp();
				DestroyImmediate (physicalMap);
			}

			if (Application.isPlaying) Destroy(rootMapGo);
			else DestroyImmediate(rootMapGo);	
			virtualMaps = null;
			physicalMap = null;
		}

		// Remove any other existing children as well
		foreach(Transform childTr in this.transform){
			DestroyImmediate (childTr.gameObject);
		}

		MapInterpreter interpreter;
		VirtualMapGenerator mapGenerator;
		
		if (printTimings) preDate = System.DateTime.Now;
		
		if (!ForceCommonSenseOptions()) return;

		physicalMapBehaviour.MeasureSizes();
		SetGeneratorValues();
		
		mapGenerator = new VirtualMapGenerator ();

		virtualMaps = new VirtualMap[numberOfStoreys];
		mapGenerator.InitialiseSeed(useSeed,seed);
		lastUsedSeed = mapGenerator.lastUsedSeed;
		for(int i=0; i<numberOfStoreys; i++){
			if (i==0) virtualMaps[i] = mapGenerator.Generate(MapWidth,MapHeight,createRooms,i);
			else virtualMaps[i] = mapGenerator.Generate(MapWidth,MapHeight,createRooms,i,virtualMaps[i-1]);
		}
		interpreter = interpreterBehaviour.Generate();
		physicalMap = physicalMapBehaviour.Generate(virtualMaps,this,interpreter);

		this.rootMapGo = physicalMap.rootMapGo;
			
		if (printTimings){
			postDate = System.DateTime.Now;
			TimeSpan timeDifference = postDate.Subtract (preDate);
			Debug.Log ("Generated in " + timeDifference.TotalMilliseconds.ToString () + " ms");
		}

		BroadcastMessage("DungeonGenerated",SendMessageOptions.DontRequireReceiver);
		isCurrentlyGenerating = false;
	}
	
	
	private bool ForceCommonSenseOptions(){
		if (!createRooms || !createStartAndEnd) forceStartAndEndInRooms = false;
//		if (!createStartAndEnd) createPlayer = false;
		
		if (maxRooms < minRooms) maxRooms = minRooms;
		if (maxRoomHeight < minRoomHeight) maxRoomHeight = minRoomHeight;
		if (maxRoomWidth < minRoomWidth) maxRoomWidth = minRoomWidth;		
		
		return this.physicalMapBehaviour.CheckDefaults();
	}
	
	private void SetGeneratorValues()
	{
		GeneratorValues.seed = seed;
		GeneratorValues.algorithmChoice = algorithmChoice;
		GeneratorValues.directionChangeModifier = directionChangeModifier;
		GeneratorValues.sparsenessModifier = sparsenessModifier;
		GeneratorValues.openDeadEndModifier = openDeadEndModifier;
		
		GeneratorValues.minRooms = minRooms;
		GeneratorValues.maxRooms = maxRooms;
		GeneratorValues.minRoomWidth = minRoomWidth;
		GeneratorValues.maxRoomWidth = maxRoomWidth;
		GeneratorValues.minRoomHeight = minRoomHeight;
		GeneratorValues.maxRoomHeight = maxRoomHeight;
		GeneratorValues.doorsDensityModifier = doorsDensityModifier;
		
		GeneratorValues.createStartAndEnd = createStartAndEnd;
		GeneratorValues.forceStartAndEndInRooms = forceStartAndEndInRooms;
		GeneratorValues.multiStorey = multiStorey;
		GeneratorValues.numberOfStoreys = numberOfStoreys;
		
		GeneratorValues.minimumDistanceBetweenStartAndEnd = minimumDistanceBetweenStartAndEnd;
		GeneratorValues.maximumDistanceBetweenStartAndEnd = maximumDistanceBetweenStartAndEnd;
	}
	
	public PhysicalMap getPhysicalMap(){
		return this.physicalMap;	
	}
	public VirtualMap[] getVirtualMaps(){
		return this.virtualMaps;	
	}
}
	
