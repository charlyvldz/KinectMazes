using UnityEngine;
using System.Collections;

public class LoadMainLevel : MonoBehaviour 
{
	private bool levelLoaded = false;
	
	
	void Update() 
	{
		KinectManager manager = KinectManager.AdministradorKinect;
		
		if(!levelLoaded && manager && KinectManager.SensorKinectInicializado())
		{
			levelLoaded = true;
			Application.LoadLevel(1);
		}
	}
	
}
