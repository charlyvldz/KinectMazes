using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

[CustomEditor (typeof(GeneratorBehaviour))]
public class GeneratorBehaviourInterface : Editor {
	
	GUIStyle errorStyle;

	public bool showAdvanced = false;
	public override void OnInspectorGUI() {
		errorStyle = new GUIStyle();
		errorStyle.normal.textColor = Color.red;

		GeneratorBehaviour t = (GeneratorBehaviour) target;

		t.mapDimensionsType = (MapDimensionsType)EditorGUILayout.EnumPopup("Map Dimensions Type",t.mapDimensionsType);
		
		t.MapWidth = EditorGUILayout.IntSlider("Map Width",t.MapWidth,2,GeneratorBehaviour.MAX_SIZE);
		t.MapHeight = EditorGUILayout.IntSlider("Map Length",t.MapHeight,2,GeneratorBehaviour.MAX_SIZE);
		
		t.algorithmChoice = (MazeGenerationAlgorithmChoice)EditorGUILayout.EnumPopup("Generation Algorithm: ",t.algorithmChoice);
		
		EditorGUILayout.BeginHorizontal();
		t.useSeed = EditorGUILayout.Toggle("Use seed",t.useSeed);
		EditorGUI.BeginDisabledGroup(t.useSeed == false);
		t.seed = EditorGUILayout.IntSlider("Seed",t.seed,0,10000);
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.EndHorizontal();

		t.directionChangeModifier = EditorGUILayout.IntSlider("Non-Linearity %",t.directionChangeModifier,0,100);
		t.sparsenessModifier = EditorGUILayout.IntSlider("Sparseness %",t.sparsenessModifier,0,90);	// Sparseness cannot be too high!
		t.openDeadEndModifier = EditorGUILayout.IntSlider("Link Dead Ends %",t.openDeadEndModifier,0,100);
		
		t.createRooms = EditorGUILayout.Toggle("Create Rooms",t.createRooms);
		if(t.createRooms){
				
			EditorGUILayout.BeginHorizontal();
			t.minRooms = EditorGUILayout.IntSlider("Min Rooms",t.minRooms,0,10);
			t.maxRooms = EditorGUILayout.IntSlider("Max Rooms",t.maxRooms,0,10);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			t.minRoomWidth = EditorGUILayout.IntSlider("Min Room Width",t.minRoomWidth,2,10);
			t.maxRoomWidth = EditorGUILayout.IntSlider("Max Room Width",t.maxRoomWidth,2,10);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			t.minRoomHeight = EditorGUILayout.IntSlider("Min Room Length",t.minRoomHeight,2,10);
			t.maxRoomHeight = EditorGUILayout.IntSlider("Max Room Length",t.maxRoomHeight,2,10);
			EditorGUILayout.EndHorizontal();
		
			t.doorsDensityModifier = EditorGUILayout.IntSlider("Passage Density %",t.doorsDensityModifier,0,100);
		}

		if (t.mapDimensionsType == MapDimensionsType.THREE_DEE){
			t.multiStorey = EditorGUILayout.Toggle("Multi-Storey",t.multiStorey);
			if (t.multiStorey) t.numberOfStoreys = EditorGUILayout.IntSlider("Number of Storeys",t.numberOfStoreys,1,10);
			else t.numberOfStoreys = 1;
		} else {
			t.multiStorey = false;
			t.numberOfStoreys = 1;
		}

		EditorGUILayout.BeginHorizontal();
		t.createStartAndEnd = EditorGUILayout.Toggle("Create Entrance and Exit",t.createStartAndEnd);
		if (t.createRooms && t.createStartAndEnd) t.forceStartAndEndInRooms = EditorGUILayout.Toggle("Force in Room",t.forceStartAndEndInRooms);
		EditorGUILayout.EndHorizontal();
		if (t.createStartAndEnd) EditorGUILayout.MinMaxSlider(new GUIContent("Min & Max distance % between entrance and exit"),ref t.minimumDistanceBetweenStartAndEnd,ref t.maximumDistanceBetweenStartAndEnd,0,100);


		PhysicalMapBehaviour physicalMapBehaviour = t.gameObject.GetComponent<PhysicalMapBehaviour>();
		MapInterpreterBehaviour interpreterBehaviour = t.gameObject.GetComponent<MapInterpreterBehaviour>();
		
		this.showAdvanced = EditorGUILayout.Foldout(this.showAdvanced, "Advanced Options");
		if(this.showAdvanced){
			GUILayout.BeginHorizontal();
			t.printTimings = EditorGUILayout.Toggle("Print Timings",t.printTimings);
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Last used seed: " + t.lastUsedSeed);
		}

		EditorGUI.BeginDisabledGroup(Application.isPlaying == true);
		bool canGenerate = true;
		if (physicalMapBehaviour == null){
			GUILayout.Label("Attach a Physical Map Behaviour to enable generation!",errorStyle);	
			canGenerate = false;
		}
		else if (t.mapDimensionsType == MapDimensionsType.THREE_DEE && !physicalMapBehaviour.SupportsThreeDeeMap){
			GUILayout.Label("The attached Physical Map Behaviour does not support a 3D map!",errorStyle);	
			canGenerate = false;
		}
		if (interpreterBehaviour == null){
			GUILayout.Label("Attach a Map Interpreter Behaviour to enable generation!",errorStyle);	
			canGenerate = false;
		}
		
		EditorGUI.BeginDisabledGroup(!canGenerate);
		if (GUILayout.Button("Generate!")) t.Generate();
		EditorGUI.EndDisabledGroup();
		
		#region Save/Load buttons
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Save"))
		{
			SavePrefab();
		}
		EditorGUILayout.EndHorizontal();
		#endregion
		EditorGUI.EndDisabledGroup();
		
	}
	
	private void SavePrefab(){
		GameObject prefab = ((GeneratorBehaviour) target).gameObject;
		
		string path = EditorUtility.SaveFilePanel("Save file...", "Assets", "DungeonPrefab", "prefab");
		string[] path_tokens = path.Split('/');
		for (int i=0; i<path_tokens.Length; i++){
			if (path_tokens[i] == "Assets"){
				path = "Assets";
				for (int j=i+1; j<path_tokens.Length; j++){
					path += "/"+path_tokens[j];
				}
				break;
			}
		}
		
		if (path == "") return;
		
		PrefabUtility.CreatePrefab(path,prefab);
		
	}

}
