using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[RequireComponent (typeof (GeneratorBehaviour))]
public abstract class MapInterpreterBehaviour : MonoBehaviour {
	
	abstract public MapInterpreter Generate();

	// Common parameters
	public bool drawRocks = false;
	public bool drawWallCorners = false;
	public bool drawDoors = false;
	public bool createColumnsInRooms = false;
	public bool randomOrientations = false;
	
	public bool useDirectionalFloors = false;

	public bool usePerimeter = false;
	public bool internalPerimeter = false;

	// 3D parameters
	public bool addCeilingToCorridors = false;
	public bool addCeilingToRooms = false;

}
