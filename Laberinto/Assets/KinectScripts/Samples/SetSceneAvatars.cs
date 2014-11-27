using UnityEngine;
using System.Collections;

public class SetSceneAvatars : MonoBehaviour 
{

	void Start () 
	{
		KinectManager manager = KinectManager.AdministradorKinect;
		
		if(manager)
		{
			manager.ClearKinectUsers();
			
			AvatarController[] avatars = FindObjectsOfType(typeof(AvatarController)) as AvatarController[];
			
			foreach(AvatarController avatar in avatars)
			{
				manager.AvataresdeJugador1.Add(avatar.gameObject);
			}
			
			manager.ResetAvatarControllers();
		}
	}
	
}
