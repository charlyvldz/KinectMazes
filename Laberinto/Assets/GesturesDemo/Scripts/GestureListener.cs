using UnityEngine;
using System.Collections;
using System;

public class GestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
	// GUI Text to display the gesture messages.
	public GUIText GestureInfo;
	
	private bool _swipeLeft;
	private bool _swipeRight;
	private bool _swipeUp;
	private bool _swipeDown;
	
	
	public bool IsSwipeLeft()
	{
	    if (!_swipeLeft) return false;
	    _swipeLeft = false;
	    return true;
	}
	
	public bool IsSwipeRight()
	{
	    if (!_swipeRight) return false;
	    _swipeRight = false;
	    return true;
	}
	
	public bool IsSwipeUp()
	{
	    if (!_swipeUp) return false;
	    _swipeUp = false;
	    return true;
	}
	
	public bool IsSwipeDown()
	{
	    if (!_swipeDown) return false;
	    _swipeDown = false;
	    return true;
	}
	

	public void UserDetected(uint userId, int userIndex)
	{
		// detect these user specific gestures
		KinectManager manager = KinectManager.AdministradorKinect;
		
		manager.DetectGesture(userId, KinectGestures.Gestures.SwipeLeft);
		manager.DetectGesture(userId, KinectGestures.Gestures.SwipeRight);
//		manager.DetectGesture(userId, KinectGestures.Gestures.SwipeUp);
//		manager.DetectGesture(userId, KinectGestures.Gestures.SwipeDown);

		if(GestureInfo != null)
		{
			GestureInfo.guiText.text = "Swipe left or right to change the slides.";
		}
	}
	
	public void UserLost(uint userId, int userIndex)
	{
		if(GestureInfo != null)
		{
			GestureInfo.guiText.text = string.Empty;
		}
	}

	public void GestureInProgress(uint userId, int userIndex, KinectGestures.Gestures gesture, 
		float progress, KinectWrapper.SkeletonJoint joint, Vector3 screenPos)
	{
		// don't do anything here
	}

	public bool GestureCompleted (uint userId, int userIndex, KinectGestures.Gestures gesture, 
		KinectWrapper.SkeletonJoint joint, Vector3 screenPos)
	{
		string sGestureText = gesture + " detected";
		if(GestureInfo != null)
		{
			GestureInfo.guiText.text = sGestureText;
		}
	    switch (gesture)
	    {
	        case KinectGestures.Gestures.SwipeLeft:
	            _swipeLeft = true;
	            break;
	        case KinectGestures.Gestures.SwipeRight:
	            _swipeRight = true;
	            break;
	        case KinectGestures.Gestures.SwipeUp:
	            _swipeUp = true;
	            break;
	        case KinectGestures.Gestures.SwipeDown:
	            _swipeDown = true;
	            break;
	    }

	    return true;
	}

	public bool GestureCancelled (uint userId, int userIndex, KinectGestures.Gestures gesture, 
		KinectWrapper.SkeletonJoint joint)
	{
		// don't do anything here, just reset the gesture state
		return true;
	}
	
}
