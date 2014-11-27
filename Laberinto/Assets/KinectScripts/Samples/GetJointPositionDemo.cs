using UnityEngine;
using System.Collections;

public class GetJointPositionDemo : MonoBehaviour 
{
	// the joint we want to track
	public KinectWrapper.NuiSkeletonPositionIndex joint = KinectWrapper.NuiSkeletonPositionIndex.HandRight;

	// joint position at the moment, in Kinect coordinates
	public Vector3 outputPosition;


	void Update () 
	{
		KinectManager manager = KinectManager.AdministradorKinect;

		if(manager && manager.EstaActivoelSensor())
		{
			if(manager.IsUserDetected())
			{
				uint userId = manager.GetPlayer1ID();

				if(manager.IsJointTracked(userId, (int)joint))
				{
					outputPosition = manager.GetJointPosition(userId, (int)joint);
				}
			}
		}
	}
}
