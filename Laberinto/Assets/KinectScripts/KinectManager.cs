using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;






/// <summary>
/// Clase que Hereda de las clases de MonoBehaviour (Proporcionada por las librerías de Unity
/// </summary>
public class KinectManager : MonoBehaviour
{

    #region VARIABLES GLOBALES
    //variable que muestra el suvizado
    public enum Smoothing : int
    {
        Ninguno, 
        Default, 
        Medio, 
        Agresivo
    }




    //Se determina si hay dos usuarios en el campo de visión del sensor Kinect
	public bool DosUsuarios = false;
	
    //Variable para determinar y procesar un nuevo mapa de usuario
	public bool ProcesarMapadeUsuario = false;
	
    //Variable para determinar y procesar el color del mapa
	public bool ProcesarColordeMapa = false;
	
	//Variable que procesa el mapa de usuario
	public bool MostrarMapadeUsuario = false;
	
	//Variable que determina si el color se va a mostrar
	public bool MostrarColordeMapa = false;
	
	//Variable que determina si se muestran las líneas del esqueleto procesadas por Kinect
	public bool MostrarLineasdeEsqueleto = false;

    //Variable que especifica el ancho de la imagen usada por la profundidad y mapas de color (expresada en porcentaje del ancho del espectro de la cámara). La altura se calcula en base al ancho detectado por el sensor
	public float MostrarAnchoPorcentualdeMapas = 20f;

	//Altura desde el suelo hasta que se detecte el final del cuerpo (en metros)
	public float AlturadeSensorKinect = 1.0f;

	//Angulo de elevacion del Kinect
	public int AngulodeElevaciondeKinect = 0;
	
	//Distancia mínima entre el usuario y el Sensor
	public float DistanciaMinimaalSensor = 1.0f;
	
	//Distancia Máxima entre el usuario y el sensor
	public float DistanciaMaximaalSensor = 0f;
	
	//variable que detecta si el usuario está cerca del sensor
	public bool DetectaCercaniadeUsuario = true;
	
	//Variable que muestra si el uso de si una articulación está en un conjunto completo
	public bool IgnorarConjuntosdeArticulaciones = true;
	
	//variable que muestra el suavizado con un enumerador(el de mas arriba)
	public Smoothing Suavizado = Smoothing.Default;
	
	//Conjunto de variables que muestran filtros adicionales
	public bool UsarFiltrodeOrientacionesdeHueso = false;
	public bool UsarFiltrodePiernasCortadas = false;
	public bool UsarRestricciondeOrientacionesdeHueso = true;
	public bool UsarRestricciondeAutointerseccion = false;
	
    //Lista de Objetos de juego (Clase contenida en la referencia Unity) que serán controlados por cada jugador
	public List<GameObject> AvataresdeJugador1;
	public List<GameObject> AvataresdeJugador2;
	
    //Posturas de calibracion para cada jugador, si las necesitan
	public KinectGestures.Gestures PosturadeCalibraciondeJugador1;
	public KinectGestures.Gestures PosturadeCalibraciondeJugador2;
	
	//Listas de gestos para detectar a cada jugador
	public List<KinectGestures.Gestures> lstGestosJugador1;
	public List<KinectGestures.Gestures> lstGestosJugador2;
	
	//Tiempo mínimo para detectar gestos
	public float TiempoMinimoEntreGestos = 0.7f;
	
    //Lista de Listeners de Gestos(son los que se encargan de recibir y procesar los gestos que se le hacen al sensor)
	public List<MonoBehaviour> lstListenersdeGestos;
	
    //Interfaz gráfica para mostrar mensajes
	public GameObject TextodeCalibracion;

    //Textura para mostrar el cursor del jugador1
    public GameObject CursordeMano1;   
    
    //Textura para mostrar el cursor del jugador2
	public GameObject CursordeMano2;

    //Variable que especifica si una el cursor es izquierdo o derecho y hace el gesto de click(interface) para controlar el mouse y hacer click
	public bool ControldeCursordeMouse = false;
	
	//Variable que verifica si el sensor esta inicializado
	private bool SensorKinectInicializado = false; 
	
	//variable que verifica para cuales jugadores está calibrado el kinect
	private bool Jugador1Calibrado = false;
	private bool Jugador2Calibrado = false;
	private bool TodoslosJugadoresEstanCalibrados = false;

	//se rastrea por medio de kinect quien es el jugador 1 y el 2
	private uint IDJugador1;
	private uint IDJugador2;
	private int IndicedeJugador1;
	private int IndicedeJugador2;

    //Listas de controladores de avatars que permitirán a los modelos actualizarse
	private List<AvatarController> lstControladoresdeJugador1;
	private List<AvatarController> lstControladoresdeJugador2;

	//variables de mapas de usuario
	private Texture2D lblTexturaUsuarios;
	private Color32[] arrColoresdeMapadeUsuario;
	private ushort[] EstatusPreviodeUsuario;
	private Rect RectangulodeMapadeUsuario;
	private int TamanoMapadeUsuarios;
	private Texture2D TexturadeBorradodeUsuarios;
    private Rect PoligonodeColoresdeUsuario;
	private ushort[] arrProfundidaddeMapadeUsuarios;
	private float[] arrMapadeHistogramadeUsuarios;
	
	//lista que contendrá a todos los usuarios
	private List<uint> lstTodoslosUsuarios;

    //Manejo de imagenes para kinect
	private IntPtr ptrManejodeColor;
	private IntPtr ptrManejodeProfundidad;
	
	//datos de color de imagen
	private Color32[] arrColoresdeImagen;
	private byte[] arrMapadeColordeUsuarios;
	
	//estructuras de esqueleto relacionadas
	private KinectWrapper.NuiSkeletonFrame MarcodelEsqueleto;
	private KinectWrapper.NuiTransformSmoothParameters ParametrosdeProfundidad;
    private int IniciodeJugador1;
    private int IniciodeJugador2;

    //variables que rastrean el esqueleto de los jugadores, sus posiciones, orientaciones y articulaciones
	private Vector3 PosicionJugador1;
    private Vector3 PosicionJugador2;
	private Matrix4x4 OrientaciondeJugador1;
    private Matrix4x4 OrientaciondeJugador2;
	private bool[] arrArticulacionesdeJugador1Detectadas;
    private bool[] arrArticulacionesdeJugador2Detectadas;
    private bool[] player1PrevTracked;
    private bool[] player2PrevTracked;
    private Vector3[] arrPOsiciondeArticulacionesJugador1;
    private Vector3[] arrPosiciondeArticulacionesJugador2;
    private Matrix4x4[] OrientaciondeArticulacionesJugador1;
    private Matrix4x4[] OrientaciondeArticulacionesJugador2;
	private KinectWrapper.NuiSkeletonBoneOrientation[] arrOrientaciondeArticulaciones;
	
	//calibracion de gestos para cada jugador
	private KinectGestures.GestureData datosdeCalibracionparaJugador1;
	private KinectGestures.GestureData darosdeCalibracionparaJugador2;
    
    //listas de gestos de cada jugador
	private List<KinectGestures.GestureData> lstJugador1Gestos = new List<KinectGestures.GestureData>();
	private List<KinectGestures.GestureData> lstJugador2Gestos = new List<KinectGestures.GestureData>();
	
    //tiempo de inicio de rastreo de gestos
	private float[] arrRastreodeGestosenTiempo;

    //lista de listeners de gestos
	public List<KinectGestures.GestureListenerInterface> lstHeadersdeGestos;
    private Matrix4x4 kinectToWorld;
    private Matrix4x4 MatrizdeMovimientos;
	private static KinectManager administradorKinect;

    //temporizador para controlar las mezclas de los filtros
    private float nuiUltimaVez;

	// Filters
	private TrackingStateFilter[] arrFiltrodeEstadodeRastreo;
	private BoneOrientationsFilter[] arrFiltrodeOrientaciondeHuesos;
	private ClippedLegsFilter[] arrFiltrodePiernasCortadas;
	private BoneOrientationsConstraint FiltrodeRestriccionesdeHuesos;
	private SelfIntersectionConstraint RestricciondeInterseccion;


    #endregion


    public static KinectManager AdministradorKinect
    {
        get
        {
            return administradorKinect;
        }
    }
	
	//metodo que verifica si el sensor kinect está inicializado
	public static bool IsKinectInitialized()
	{
		return administradorKinect != null ? administradorKinect.SensorKinectInicializado : false;
	}
	
	//metodo que verifica si el sensor está activo
	public bool EstaActivoelSensor()
	{
		return SensorKinectInicializado;
	}
	
	
	public static bool IsCalibrationNeeded()
	{
		return false;
	}
	
	
	public ushort[] GetRawDepthMap()
	{
		return arrProfundidaddeMapadeUsuarios;
	}
	

	public ushort ObtenerProfundidadporPixel(int x, int y)
	{
		int index = y * KinectWrapper.Constants.DepthImageWidth + x;
		
		if(index >= 0 && index < arrProfundidaddeMapadeUsuarios.Length)
			return arrProfundidaddeMapadeUsuarios[index];
		else
			return 0;
	}
	
	
	public Vector2 GetDepthMapPosForJointPos(Vector3 posJoint)
	{
		Vector3 vDepthPos = KinectWrapper.MapSkeletonPointToDepthPoint(posJoint);
		Vector2 vMapPos = new Vector2(vDepthPos.x, vDepthPos.y);
		
		return vMapPos;
	}
	
	// returns the color map position for a depth 2d position
	public Vector2 GetColorMapPosForDepthPos(Vector2 posDepth)
	{
		int cx, cy;

		KinectWrapper.NuiImageViewArea pcViewArea = new KinectWrapper.NuiImageViewArea 
		{
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };
		
		KinectWrapper.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(
			KinectWrapper.Constants.ColorImageResolution,
			KinectWrapper.Constants.DepthImageResolution,
			ref pcViewArea,
			(int)posDepth.x, (int)posDepth.y, ObtenerProfundidadporPixel((int)posDepth.x, (int)posDepth.y),
			out cx, out cy);
		
		return new Vector2(cx, cy);
	}
	
	// returns the depth image/users histogram texture,if ProcesarMapadeUsuario is true
    public Texture2D GetUsersLblTex()
    { 
		return lblTexturaUsuarios;
	}
	
	// returns the color image texture,if ProcesarColordeMapa is true
    public Texture2D GetUsersClrTex()
    { 
		return TexturadeBorradodeUsuarios;
	}
	
	// returns true if at least one user is currently detected by the sensor
	public bool IsUserDetected()
	{
		return SensorKinectInicializado && (lstTodoslosUsuarios.Count > 0);
	}
	
	// returns the UserID of Player1, or 0 if no Player1 is detected
	public uint GetPlayer1ID()
	{
		return IDJugador1;
	}
	
	// returns the UserID of Player2, or 0 if no Player2 is detected
	public uint GetPlayer2ID()
	{
		return IDJugador2;
	}
	
	// returns the index of Player1, or 0 if no Player2 is detected
	public int GetPlayer1Index()
	{
		return IndicedeJugador1;
	}
	
	// returns the index of Player2, or 0 if no Player2 is detected
	public int GetPlayer2Index()
	{
		return IndicedeJugador2;
	}
	
	// returns true if the User is calibrated and ready to use
	public bool IsPlayerCalibrated(uint UserId)
	{
		if(UserId == IDJugador1)
			return Jugador1Calibrado;
		else if(UserId == IDJugador2)
			return Jugador2Calibrado;
		
		return false;
	}
	
	// returns the raw unmodified joint position, as returned by the Kinect sensor
	public Vector3 GetRawSkeletonJointPos(uint UserId, int joint)
	{
		if(UserId == IDJugador1)
			return joint >= 0 && joint < arrPOsiciondeArticulacionesJugador1.Length ? (Vector3)MarcodelEsqueleto.SkeletonData[IniciodeJugador1].SkeletonPositions[joint] : Vector3.zero;
		else if(UserId == IDJugador2)
			return joint >= 0 && joint < arrPosiciondeArticulacionesJugador2.Length ? (Vector3)MarcodelEsqueleto.SkeletonData[IniciodeJugador2].SkeletonPositions[joint] : Vector3.zero;
		
		return Vector3.zero;
	}
	
	// returns the User position, relative to the Kinect-sensor, in meters
	public Vector3 GetUserPosition(uint UserId)
	{
		if(UserId == IDJugador1)
			return PosicionJugador1;
		else if(UserId == IDJugador2)
			return PosicionJugador2;
		
		return Vector3.zero;
	}
	
	// returns the User rotation, relative to the Kinect-sensor
	public Quaternion GetUserOrientation(uint UserId, bool flip)
	{
		if(UserId == IDJugador1 && arrArticulacionesdeJugador1Detectadas[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter])
			return ConvertMatrixToQuat(OrientaciondeJugador1, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter, flip);
		else if(UserId == IDJugador2 && arrArticulacionesdeJugador2Detectadas[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter])
			return ConvertMatrixToQuat(OrientaciondeJugador2, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter, flip);
		
		return Quaternion.identity;
	}
	
	// returns true if the given joint of the specified user is being tracked
	public bool IsJointTracked(uint UserId, int joint)
	{
		if(UserId == IDJugador1)
			return joint >= 0 && joint < arrArticulacionesdeJugador1Detectadas.Length ? arrArticulacionesdeJugador1Detectadas[joint] : false;
		else if(UserId == IDJugador2)
			return joint >= 0 && joint < arrArticulacionesdeJugador2Detectadas.Length ? arrArticulacionesdeJugador2Detectadas[joint] : false;
		
		return false;
	}
	
	// returns the joint position of the specified user, relative to the Kinect-sensor, in meters
	public Vector3 GetJointPosition(uint UserId, int joint)
	{
		if(UserId == IDJugador1)
			return joint >= 0 && joint < arrPOsiciondeArticulacionesJugador1.Length ? arrPOsiciondeArticulacionesJugador1[joint] : Vector3.zero;
		else if(UserId == IDJugador2)
			return joint >= 0 && joint < arrPosiciondeArticulacionesJugador2.Length ? arrPosiciondeArticulacionesJugador2[joint] : Vector3.zero;
		
		return Vector3.zero;
	}
	
	// returns the local joint position of the specified user, relative to the parent joint, in meters
	public Vector3 GetJointLocalPosition(uint UserId, int joint)
	{
        int parent = KinectWrapper.GetSkeletonJointParent(joint);

		if(UserId == IDJugador1)
			return joint >= 0 && joint < arrPOsiciondeArticulacionesJugador1.Length ? 
				(arrPOsiciondeArticulacionesJugador1[joint] - arrPOsiciondeArticulacionesJugador1[parent]) : Vector3.zero;
		else if(UserId == IDJugador2)
			return joint >= 0 && joint < arrPosiciondeArticulacionesJugador2.Length ? 
				(arrPosiciondeArticulacionesJugador2[joint] - arrPosiciondeArticulacionesJugador2[parent]) : Vector3.zero;
		
		return Vector3.zero;
	}
	
	// returns the joint rotation of the specified user, relative to the Kinect-sensor
	public Quaternion GetJointOrientation(uint UserId, int joint, bool flip)
	{
		if(UserId == IDJugador1)
		{
			if(joint >= 0 && joint < OrientaciondeArticulacionesJugador1.Length && arrArticulacionesdeJugador1Detectadas[joint])
				return ConvertMatrixToQuat(OrientaciondeArticulacionesJugador1[joint], joint, flip);
		}
		else if(UserId == IDJugador2)
		{
			if(joint >= 0 && joint < OrientaciondeArticulacionesJugador2.Length && arrArticulacionesdeJugador2Detectadas[joint])
				return ConvertMatrixToQuat(OrientaciondeArticulacionesJugador2[joint], joint, flip);
		}
		
		return Quaternion.identity;
	}
	
	// returns the joint rotation of the specified user, relative to the parent joint
	public Quaternion GetJointLocalOrientation(uint UserId, int joint, bool flip)
	{
        int parent = KinectWrapper.GetSkeletonJointParent(joint);

		if(UserId == IDJugador1)
		{
			if(joint >= 0 && joint < OrientaciondeArticulacionesJugador1.Length && arrArticulacionesdeJugador1Detectadas[joint])
			{
				Matrix4x4 localMat = (OrientaciondeArticulacionesJugador1[parent].inverse * OrientaciondeArticulacionesJugador1[joint]);
				return Quaternion.LookRotation(localMat.GetColumn(2), localMat.GetColumn(1));
			}
		}
		else if(UserId == IDJugador2)
		{
			if(joint >= 0 && joint < OrientaciondeArticulacionesJugador2.Length && arrArticulacionesdeJugador2Detectadas[joint])
			{
				Matrix4x4 localMat = (OrientaciondeArticulacionesJugador2[parent].inverse * OrientaciondeArticulacionesJugador2[joint]);
				return Quaternion.LookRotation(localMat.GetColumn(2), localMat.GetColumn(1));
			}
		}
		
		return Quaternion.identity;
	}
	
	// returns the direction between baseJoint and nextJoint, for the specified user
	public Vector3 GetDirectionBetweenJoints(uint UserId, int baseJoint, int nextJoint, bool flipX, bool flipZ)
	{
		Vector3 jointDir = Vector3.zero;
		
		if(UserId == IDJugador1)
		{
			if(baseJoint >= 0 && baseJoint < arrPOsiciondeArticulacionesJugador1.Length && arrArticulacionesdeJugador1Detectadas[baseJoint] &&
				nextJoint >= 0 && nextJoint < arrPOsiciondeArticulacionesJugador1.Length && arrArticulacionesdeJugador1Detectadas[nextJoint])
			{
				jointDir = arrPOsiciondeArticulacionesJugador1[nextJoint] - arrPOsiciondeArticulacionesJugador1[baseJoint];
			}
		}
		else if(UserId == IDJugador2)
		{
			if(baseJoint >= 0 && baseJoint < arrPosiciondeArticulacionesJugador2.Length && arrArticulacionesdeJugador2Detectadas[baseJoint] &&
				nextJoint >= 0 && nextJoint < arrPosiciondeArticulacionesJugador2.Length && arrArticulacionesdeJugador2Detectadas[nextJoint])
			{
				jointDir = arrPosiciondeArticulacionesJugador2[nextJoint] - arrPosiciondeArticulacionesJugador2[baseJoint];
			}
		}
		
		if(jointDir != Vector3.zero)
		{
			if(flipX)
				jointDir.x = -jointDir.x;
			
			if(flipZ)
				jointDir.z = -jointDir.z;
		}
		
		return jointDir;
	}
	
	// adds a gesture to the list of detected gestures for the specified user
	public void DetectGesture(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);
		if(index >= 0)
			DeleteGesture(UserId, gesture);
		
		KinectGestures.GestureData gestureData = new KinectGestures.GestureData
		{
		    userId = UserId,
		    gesture = gesture,
		    state = 0,
		    joint = 0,
		    progress = 0f,
		    complete = false,
		    cancelled = false,
		    checkForGestures = new List<KinectGestures.Gestures>()
		};

	    switch(gesture)
		{
			case KinectGestures.Gestures.ZoomIn:
				gestureData.checkForGestures.Add(KinectGestures.Gestures.ZoomOut);
				gestureData.checkForGestures.Add(KinectGestures.Gestures.Wheel);			
				break;

			case KinectGestures.Gestures.ZoomOut:
				gestureData.checkForGestures.Add(KinectGestures.Gestures.ZoomIn);
				gestureData.checkForGestures.Add(KinectGestures.Gestures.Wheel);			
				break;

			case KinectGestures.Gestures.Wheel:
				gestureData.checkForGestures.Add(KinectGestures.Gestures.ZoomIn);
				gestureData.checkForGestures.Add(KinectGestures.Gestures.ZoomOut);			
				break;
			

		}
		
		if(UserId == IDJugador1)
			lstJugador1Gestos.Add(gestureData);
		else if(UserId == IDJugador2)
			lstJugador2Gestos.Add(gestureData);
	}
	
	
	public bool ResetGesture(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);
		if(index < 0)
			return false;
		
		KinectGestures.GestureData gestureData = (UserId == IDJugador1) ? lstJugador1Gestos[index] : lstJugador2Gestos[index];
		
		gestureData.state = 0;
		gestureData.joint = 0;
		gestureData.progress = 0f;
		gestureData.complete = false;
		gestureData.cancelled = false;
		gestureData.startTrackingAtTime = Time.realtimeSinceStartup + KinectWrapper.Constants.MinTimeBetweenSameGestures;

		if(UserId == IDJugador1)
			lstJugador1Gestos[index] = gestureData;
		else if(UserId == IDJugador2)
			lstJugador2Gestos[index] = gestureData;
		
		return true;
	}
	
	// resets the gesture-data states for all detected gestures of the specified user
	public void ResetPlayerGestures(uint UserId)
	{
		if(UserId == IDJugador1)
		{
			int listSize = lstJugador1Gestos.Count;
			
			for(int i = 0; i < listSize; i++)
			{
				ResetGesture(UserId, lstJugador1Gestos[i].gesture);
			}
		}
		else if(UserId == IDJugador2)
		{
			int listSize = lstJugador2Gestos.Count;
			
			for(int i = 0; i < listSize; i++)
			{
				ResetGesture(UserId, lstJugador2Gestos[i].gesture);
			}
		}
	}
	
	// deletes the given gesture from the list of detected gestures for the specified user
	public bool DeleteGesture(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);
		if(index < 0)
			return false;
		
		if(UserId == IDJugador1)
			lstJugador1Gestos.RemoveAt(index);
		else if(UserId == IDJugador2)
			lstJugador2Gestos.RemoveAt(index);
		
		return true;
	}
	
	// clears detected gestures list for the specified user
	public void ClearGestures(uint UserId)
	{
		if(UserId == IDJugador1)
		{
			lstJugador1Gestos.Clear();
		}
		else if(UserId == IDJugador2)
		{
			lstJugador2Gestos.Clear();
		}
	}
	
	// returns the count of detected gestures in the list of detected gestures for the specified user
	public int GetGesturesCount(uint UserId)
	{
		if(UserId == IDJugador1)
			return lstJugador1Gestos.Count;
		else if(UserId == IDJugador2)
			return lstJugador2Gestos.Count;
		
		return 0;
	}
	
	// returns the list of detected gestures for the specified user
	public List<KinectGestures.Gestures> GetGesturesList(uint UserId)
	{
		List<KinectGestures.Gestures> list = new List<KinectGestures.Gestures>();

		if(UserId == IDJugador1)
		{
			foreach(KinectGestures.GestureData data in lstJugador1Gestos)
				list.Add(data.gesture);
		}
		else if(UserId == IDJugador2)
		{
			foreach(KinectGestures.GestureData data in lstJugador1Gestos)
				list.Add(data.gesture);
		}
		
		return list;
	}
	
	// returns true, if the given gesture is in the list of detected gestures for the specified user
	public bool IsGestureDetected(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);
		return index >= 0;
	}
	
	// returns true, if the given gesture for the specified user is complete
	public bool IsGestureComplete(uint UserId, KinectGestures.Gestures gesture, bool bResetOnComplete)
	{
		int index = GetGestureIndex(UserId, gesture);

		if(index >= 0)
		{
			if(UserId == IDJugador1)
			{
				KinectGestures.GestureData gestureData = lstJugador1Gestos[index];
				
				if(bResetOnComplete && gestureData.complete)
				{
					ResetPlayerGestures(UserId);
					return true;
				}
				
				return gestureData.complete;
			}
			else if(UserId == IDJugador2)
			{
				KinectGestures.GestureData gestureData = lstJugador2Gestos[index];

				if(bResetOnComplete && gestureData.complete)
				{
					ResetPlayerGestures(UserId);
					return true;
				}
				
				return gestureData.complete;
			}
		}
		
		return false;
	}
	
	// returns true, if the given gesture for the specified user is cancelled
	public bool IsGestureCancelled(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);

		if(index >= 0)
		{
			if(UserId == IDJugador1)
			{
				KinectGestures.GestureData gestureData = lstJugador1Gestos[index];
				return gestureData.cancelled;
			}
			else if(UserId == IDJugador2)
			{
				KinectGestures.GestureData gestureData = lstJugador2Gestos[index];
				return gestureData.cancelled;
			}
		}
		
		return false;
	}
	
	// returns the progress in range [0, 1] of the given gesture for the specified user
	public float GetGestureProgress(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);

		if(index >= 0)
		{
			if(UserId == IDJugador1)
			{
				KinectGestures.GestureData gestureData = lstJugador1Gestos[index];
				return gestureData.progress;
			}
			else if(UserId == IDJugador2)
			{
				KinectGestures.GestureData gestureData = lstJugador2Gestos[index];
				return gestureData.progress;
			}
		}
		
		return 0f;
	}
	
	// returns the current "screen position" of the given gesture for the specified user
	public Vector3 GetGestureScreenPos(uint UserId, KinectGestures.Gestures gesture)
	{
		int index = GetGestureIndex(UserId, gesture);

		if(index >= 0)
		{
			if(UserId == IDJugador1)
			{
				KinectGestures.GestureData gestureData = lstJugador1Gestos[index];
				return gestureData.screenPos;
			}
			else if(UserId == IDJugador2)
			{
				KinectGestures.GestureData gestureData = lstJugador2Gestos[index];
				return gestureData.screenPos;
			}
		}
		
		return Vector3.zero;
	}
	
	// recreates and reinitializes the internal list of gesture listeners
	public void ResetGestureListeners()
	{
		// create the list of gesture listeners
		lstHeadersdeGestos.Clear();
		
		foreach(MonoBehaviour script in lstListenersdeGestos)
		{
			if(script && (script is KinectGestures.GestureListenerInterface))
			{
				KinectGestures.GestureListenerInterface listener = (KinectGestures.GestureListenerInterface)script;
				lstHeadersdeGestos.Add(listener);
			}
		}
		
	}
	
	// recreates and reinitializes the lists of avatar controllers, after the list of avatars for player 1/2 was changed
	public void ResetAvatarControllers()
	{
		if(AvataresdeJugador1.Count == 0 && AvataresdeJugador2.Count == 0)
		{
			AvatarController[] avatars = FindObjectsOfType(typeof(AvatarController)) as AvatarController[];
			
			foreach(AvatarController avatar in avatars)
			{
				AvataresdeJugador1.Add(avatar.gameObject);
			}
		}
		
		if(lstControladoresdeJugador1 != null)
		{
			lstControladoresdeJugador1.Clear();
	
			foreach(GameObject avatar in AvataresdeJugador1)
			{
				if(avatar != null && avatar.activeInHierarchy)
				{
					AvatarController controller = avatar.GetComponent<AvatarController>();
					controller.RotateToInitialPosition();
					controller.Start();
					
					lstControladoresdeJugador1.Add(controller);
				}
			}
		}
		
		if(lstControladoresdeJugador2 != null)
		{
			lstControladoresdeJugador2.Clear();
			
			foreach(GameObject avatar in AvataresdeJugador2)
			{
				if(avatar != null && avatar.activeInHierarchy)
				{
					AvatarController controller = avatar.GetComponent<AvatarController>();
					controller.RotateToInitialPosition();
					controller.Start();
					
					lstControladoresdeJugador2.Add(controller);
				}
			}
		}
	}
	
	// removes the currently detected kinect users, allowing a new detection/calibration process to start
	public void ClearKinectUsers()
	{
		if(!SensorKinectInicializado)
			return;

		// remove current users
		for(int i = lstTodoslosUsuarios.Count - 1; i >= 0; i--)
		{
			uint userId = lstTodoslosUsuarios[i];
			RemoveUser(userId);
		}
		
		ResetFilters();
	}
	
	// clears Kinect buffers and resets the filters
	public void ResetFilters()
	{
		if(!SensorKinectInicializado)
			return;
		
		// clear kinect vars
		PosicionJugador1 = Vector3.zero; PosicionJugador2 = Vector3.zero;
		OrientaciondeJugador1 = Matrix4x4.identity; OrientaciondeJugador2 = Matrix4x4.identity;
		
		int skeletonJointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
		for(int i = 0; i < skeletonJointsCount; i++)
		{
			arrArticulacionesdeJugador1Detectadas[i] = false; arrArticulacionesdeJugador2Detectadas[i] = false;
			player1PrevTracked[i] = false; player2PrevTracked[i] = false;
			arrPOsiciondeArticulacionesJugador1[i] = Vector3.zero; arrPosiciondeArticulacionesJugador2[i] = Vector3.zero;
			OrientaciondeArticulacionesJugador1[i] = Matrix4x4.identity; OrientaciondeArticulacionesJugador2[i] = Matrix4x4.identity;
		}
		
		if(arrFiltrodeEstadodeRastreo != null)
		{
			for(int i = 0; i < arrFiltrodeEstadodeRastreo.Length; i++)
				if(arrFiltrodeEstadodeRastreo[i] != null)
					arrFiltrodeEstadodeRastreo[i].Reset();
		}
		
		if(arrFiltrodeOrientaciondeHuesos != null)
		{
			for(int i = 0; i < arrFiltrodeOrientaciondeHuesos.Length; i++)
				if(arrFiltrodeOrientaciondeHuesos[i] != null)
					arrFiltrodeOrientaciondeHuesos[i].Reset();
		}
		
		if(arrFiltrodePiernasCortadas != null)
		{
			for(int i = 0; i < arrFiltrodePiernasCortadas.Length; i++)
				if(arrFiltrodePiernasCortadas[i] != null)
					arrFiltrodePiernasCortadas[i].Reset();
		}
	}
	
	
	//----------------------------------- end of public functions --------------------------------------//

	void Start()
	{
		//TextodeCalibracion = GameObject.Find("TextodeCalibracion");
		int hr = 0;
		
		try
		{
			hr = KinectWrapper.NuiInitialize(KinectWrapper.NuiInitializeFlags.UsesSkeleton |
				KinectWrapper.NuiInitializeFlags.UsesDepthAndPlayerIndex |
				(ProcesarColordeMapa ? KinectWrapper.NuiInitializeFlags.UsesColor : 0));
            if (hr != 0)
			{
            	throw new Exception("NuiInitialize Failed");
			}
			
			hr = KinectWrapper.NuiSkeletonTrackingEnable(IntPtr.Zero, 8);  // 0, 12,8
			if (hr != 0)
			{
				throw new Exception("Cannot initialize Skeleton Data");
			}
			
			ptrManejodeProfundidad = IntPtr.Zero;
			if(ProcesarMapadeUsuario)
			{
				hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.DepthAndPlayerIndex, 
					KinectWrapper.Constants.DepthImageResolution, 0, 2, IntPtr.Zero, ref ptrManejodeProfundidad);
				if (hr != 0)
				{
					throw new Exception("Cannot open depth stream");
				}
			}
			
			ptrManejodeColor = IntPtr.Zero;
			if(ProcesarColordeMapa)
			{
				hr = KinectWrapper.NuiImageStreamOpen(KinectWrapper.NuiImageType.Color, 
					KinectWrapper.Constants.ColorImageResolution, 0, 2, IntPtr.Zero, ref ptrManejodeColor);
				if (hr != 0)
				{
					throw new Exception("Cannot open color stream");
				}
			}

			// set kinect elevation angle
			KinectWrapper.NuiCameraElevationSetAngle(AngulodeElevaciondeKinect);
			
			// init skeleton structures
			MarcodelEsqueleto = new KinectWrapper.NuiSkeletonFrame() 
							{ 
								SkeletonData = new KinectWrapper.NuiSkeletonData[KinectWrapper.Constants.NuiSkeletonCount] 
							};
			
			// values used to pass to suavizado function
			ParametrosdeProfundidad = new KinectWrapper.NuiTransformSmoothParameters();
			
			switch(Suavizado)
			{
				case Smoothing.Default:
					ParametrosdeProfundidad.fSmoothing = 0.5f;
					ParametrosdeProfundidad.fCorrection = 0.5f;
					ParametrosdeProfundidad.fPrediction = 0.5f;
					ParametrosdeProfundidad.fJitterRadius = 0.05f;
					ParametrosdeProfundidad.fMaxDeviationRadius = 0.04f;
					break;
				case Smoothing.Medio:
					ParametrosdeProfundidad.fSmoothing = 0.5f;
					ParametrosdeProfundidad.fCorrection = 0.1f;
					ParametrosdeProfundidad.fPrediction = 0.5f;
					ParametrosdeProfundidad.fJitterRadius = 0.1f;
					ParametrosdeProfundidad.fMaxDeviationRadius = 0.1f;
					break;
				case Smoothing.Agresivo:
					ParametrosdeProfundidad.fSmoothing = 0.7f;
					ParametrosdeProfundidad.fCorrection = 0.3f;
					ParametrosdeProfundidad.fPrediction = 1.0f;
					ParametrosdeProfundidad.fJitterRadius = 1.0f;
					ParametrosdeProfundidad.fMaxDeviationRadius = 1.0f;
					break;
			}
			
			// init the tracking state filter
			arrFiltrodeEstadodeRastreo = new TrackingStateFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
			for(int i = 0; i < arrFiltrodeEstadodeRastreo.Length; i++)
			{
				arrFiltrodeEstadodeRastreo[i] = new TrackingStateFilter();
				arrFiltrodeEstadodeRastreo[i].Init();
			}
			
			// init the bone orientation filter
			arrFiltrodeOrientaciondeHuesos = new BoneOrientationsFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
			for(int i = 0; i < arrFiltrodeOrientaciondeHuesos.Length; i++)
			{
				arrFiltrodeOrientaciondeHuesos[i] = new BoneOrientationsFilter();
				arrFiltrodeOrientaciondeHuesos[i].Init();
			}
			
			// init the clipped legs filter
			arrFiltrodePiernasCortadas = new ClippedLegsFilter[KinectWrapper.Constants.NuiSkeletonMaxTracked];
			for(int i = 0; i < arrFiltrodePiernasCortadas.Length; i++)
			{
				arrFiltrodePiernasCortadas[i] = new ClippedLegsFilter();
			}

			// init the bone orientation constraints
			FiltrodeRestriccionesdeHuesos = new BoneOrientationsConstraint();
			FiltrodeRestriccionesdeHuesos.AddDefaultConstraints();
			// init the self intersection constraints
			RestricciondeInterseccion = new SelfIntersectionConstraint();
			
			// create arrays for joint positions and joint orientations
			int skeletonJointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
			
			arrArticulacionesdeJugador1Detectadas = new bool[skeletonJointsCount];
			arrArticulacionesdeJugador2Detectadas = new bool[skeletonJointsCount];
			player1PrevTracked = new bool[skeletonJointsCount];
			player2PrevTracked = new bool[skeletonJointsCount];
			
			arrPOsiciondeArticulacionesJugador1 = new Vector3[skeletonJointsCount];
			arrPosiciondeArticulacionesJugador2 = new Vector3[skeletonJointsCount];
			
			OrientaciondeArticulacionesJugador1 = new Matrix4x4[skeletonJointsCount];
			OrientaciondeArticulacionesJugador2 = new Matrix4x4[skeletonJointsCount];
			
			arrRastreodeGestosenTiempo = new float[KinectWrapper.Constants.NuiSkeletonMaxTracked];
			
			//create the transform matrix that converts from kinect-space to world-space
			Quaternion quatTiltAngle = new Quaternion();
			quatTiltAngle.eulerAngles = new Vector3(-AngulodeElevaciondeKinect, 0.0f, 0.0f);
			
			//float heightAboveHips = AlturadeSensorKinect - 1.0f;
			
			// transform matrix - kinect to world
			//kinectToWorld.SetTRS(new Vector3(0.0f, heightAboveHips, 0.0f), quatTiltAngle, Vector3.one);
			kinectToWorld.SetTRS(new Vector3(0.0f, AlturadeSensorKinect, 0.0f), quatTiltAngle, Vector3.one);
			MatrizdeMovimientos = Matrix4x4.identity;
			MatrizdeMovimientos[2, 2] = -1;
			
			administradorKinect = this;
			DontDestroyOnLoad(gameObject);
		}
		catch(DllNotFoundException e)
		{
			string message = "Please check the Kinect SDK installation.";
			Debug.LogError(message);
			Debug.LogError(e.ToString());
			if(TextodeCalibracion != null)
				TextodeCalibracion.guiText.text = message;
				
			return;
		}
		catch (Exception e)
		{
			string message = e.Message + " - " + KinectWrapper.GetNuiErrorString(hr);
			Debug.LogError(message);
			Debug.LogError(e.ToString());
			if(TextodeCalibracion != null)
				TextodeCalibracion.guiText.text = message;
				
			return;
		}
		
		// get the main camera rectangle
		Rect cameraRect = Camera.main.pixelRect;
		
		// calculate map width and height in percent, if needed
		if(MostrarAnchoPorcentualdeMapas == 0f)
		{
			MostrarAnchoPorcentualdeMapas = (KinectWrapper.GetDepthWidth() / 2) * 100 / cameraRect.width;
		}

		if(ProcesarMapadeUsuario)
		{
			float displayMapsWidthPercent = MostrarAnchoPorcentualdeMapas / 100f;
			float displayMapsHeightPercent = displayMapsWidthPercent * KinectWrapper.GetDepthHeight() / KinectWrapper.GetDepthWidth();

			float displayWidth = cameraRect.width * displayMapsWidthPercent;
			float displayHeight = cameraRect.width * displayMapsHeightPercent;

	        // Initialize depth & label map related stuff
	        TamanoMapadeUsuarios = KinectWrapper.GetDepthWidth() * KinectWrapper.GetDepthHeight();
	        lblTexturaUsuarios = new Texture2D(KinectWrapper.GetDepthWidth(), KinectWrapper.GetDepthHeight());
	        arrColoresdeMapadeUsuario = new Color32[TamanoMapadeUsuarios];
			EstatusPreviodeUsuario = new ushort[TamanoMapadeUsuarios];
			//RectangulodeMapadeUsuario = new Rect(Screen.width, Screen.height - lblTexturaUsuarios.height / 2, -lblTexturaUsuarios.width / 2, lblTexturaUsuarios.height / 2);
	        //RectangulodeMapadeUsuario = new Rect(cameraRect.width, cameraRect.height - cameraRect.height * MapsPercentHeight, -cameraRect.width * MapsPercentWidth, cameraRect.height * MapsPercentHeight);
			RectangulodeMapadeUsuario = new Rect(cameraRect.width - displayWidth, cameraRect.height, displayWidth, -displayHeight);
			
	        arrProfundidaddeMapadeUsuarios = new ushort[TamanoMapadeUsuarios];
	        arrMapadeHistogramadeUsuarios = new float[8192];
		}
		
		if(ProcesarColordeMapa)
		{
			float displayMapsWidthPercent = MostrarAnchoPorcentualdeMapas / 100f;
			float displayMapsHeightPercent = displayMapsWidthPercent * KinectWrapper.GetColorHeight() / KinectWrapper.GetColorWidth();
			
			float displayWidth = cameraRect.width * displayMapsWidthPercent;
			float displayHeight = cameraRect.width * displayMapsHeightPercent;
			
			// Initialize color map related stuff
	        TexturadeBorradodeUsuarios = new Texture2D(KinectWrapper.GetColorWidth(), KinectWrapper.GetColorHeight());
	        //PoligonodeColoresdeUsuario = new Rect(cameraRect.width, cameraRect.height - cameraRect.height * MapsPercentHeight, -cameraRect.width * MapsPercentWidth, cameraRect.height * MapsPercentHeight);
			PoligonodeColoresdeUsuario = new Rect(cameraRect.width - displayWidth, cameraRect.height, displayWidth, -displayHeight);
			
//			if(ProcesarMapadeUsuario)
//				RectangulodeMapadeUsuario.x -= cameraRect.width * MostrarAnchoPorcentualdeMapas; //TexturadeBorradodeUsuarios.width / 2;
			
			arrColoresdeImagen = new Color32[KinectWrapper.GetColorWidth() * KinectWrapper.GetColorHeight()];
			arrMapadeColordeUsuarios = new byte[arrColoresdeImagen.Length << 2];
		}
		
		// try to automatically find the available avatar controllers in the scene
		if(AvataresdeJugador1.Count == 0 && AvataresdeJugador2.Count == 0)
		{
			AvatarController[] avatars = FindObjectsOfType(typeof(AvatarController)) as AvatarController[];
			
			foreach(AvatarController avatar in avatars)
			{
				AvataresdeJugador1.Add(avatar.gameObject);
			}
		}
		
        // Initialize user list to contain ALL users.
        lstTodoslosUsuarios = new List<uint>();
        
		// Pull the AvatarController from each of the players Avatars.
		lstControladoresdeJugador1 = new List<AvatarController>();
		lstControladoresdeJugador2 = new List<AvatarController>();
		
		// Add each of the avatars' controllers into a list for each player.
		foreach(GameObject avatar in AvataresdeJugador1)
		{
			if(avatar != null && avatar.activeInHierarchy)
			{
				lstControladoresdeJugador1.Add(avatar.GetComponent<AvatarController>());
			}
		}
		
		foreach(GameObject avatar in AvataresdeJugador2)
		{
			if(avatar != null && avatar.activeInHierarchy)
			{
				lstControladoresdeJugador2.Add(avatar.GetComponent<AvatarController>());
			}
		}
		
		// create the list of gesture listeners
		lstHeadersdeGestos = new List<KinectGestures.GestureListenerInterface>();
		
		foreach(MonoBehaviour script in lstListenersdeGestos)
		{
			if(script && (script is KinectGestures.GestureListenerInterface))
			{
				KinectGestures.GestureListenerInterface listener = (KinectGestures.GestureListenerInterface)script;
				lstHeadersdeGestos.Add(listener);
			}
		}
		
		// GUI Text.
		if(TextodeCalibracion != null)
		{
			TextodeCalibracion.guiText.text = "WAITING FOR USERS";
		}
		
		Debug.Log("Waiting for users.");
			
		SensorKinectInicializado = true;
	}
	
	void Update()
	{
		if(SensorKinectInicializado)
		{
			// needed by the KinectExtras' native wrapper to check for next frames
			// uncomment the line below, if you use the Extras' wrapper, but none of the Extras' managers
			//KinectWrapper.UpdateKinectSensor();
			
	        // If the players aren't all calibrated yet, draw the user map.
			if(ProcesarMapadeUsuario)
			{
				if(ptrManejodeProfundidad != IntPtr.Zero &&
					KinectWrapper.PollDepth(ptrManejodeProfundidad, KinectWrapper.Constants.IsNearMode, ref arrProfundidaddeMapadeUsuarios))
				{
		        	UpdateUserMap();
				}
			}
			
			if(ProcesarColordeMapa)
			{
				if(ptrManejodeColor != IntPtr.Zero &&
					KinectWrapper.PollColor(ptrManejodeColor, ref arrMapadeColordeUsuarios, ref arrColoresdeImagen))
				{
					UpdateColorMap();
				}
			}
			
			if(KinectWrapper.PollSkeleton(ref ParametrosdeProfundidad, ref MarcodelEsqueleto))
			{
				ProcessSkeleton();
			}
			
			// Update player 1's models if he/she is calibrated and the model is active.
			if(Jugador1Calibrado)
			{
				foreach (AvatarController controller in lstControladoresdeJugador1)
				{
					//if(controller.Active)
					{
						controller.UpdateAvatar(IDJugador1);
					}
				}
					
				// Check for complete gestures
				foreach(KinectGestures.GestureData gestureData in lstJugador1Gestos)
				{
					if(gestureData.complete)
					{
						if(gestureData.gesture == KinectGestures.Gestures.Click)
						{
							if(ControldeCursordeMouse)
							{
								MouseControl.MouseClick();
							}
						}
						
						foreach(KinectGestures.GestureListenerInterface listener in lstHeadersdeGestos)
						{
							if(listener.GestureCompleted(IDJugador1, 0, gestureData.gesture, 
								(KinectWrapper.SkeletonJoint)gestureData.joint, gestureData.screenPos))
							{
								ResetPlayerGestures(IDJugador1);
							}
						}
					}
					else if(gestureData.cancelled)
					{
						foreach(KinectGestures.GestureListenerInterface listener in lstHeadersdeGestos)
						{
							if(listener.GestureCancelled(IDJugador1, 0, gestureData.gesture, 
								(KinectWrapper.SkeletonJoint)gestureData.joint))
							{
								ResetGesture(IDJugador1, gestureData.gesture);
							}
						}
					}
					else if(gestureData.progress >= 0.1f)
					{
						if((gestureData.gesture == KinectGestures.Gestures.RightHandCursor || 
							gestureData.gesture == KinectGestures.Gestures.LeftHandCursor) && 
							gestureData.progress >= 0.5f)
						{
							if(GetGestureProgress(gestureData.userId, KinectGestures.Gestures.Click) < 0.3f)
							{
								if(CursordeMano1 != null)
								{
									CursordeMano1.transform.position = Vector3.Lerp(CursordeMano1.transform.position, gestureData.screenPos, 3 * Time.deltaTime);
								}
								
								if(ControldeCursordeMouse)
								{
									MouseControl.MouseMove(gestureData.screenPos);
								}
							}
						}
			
						foreach(KinectGestures.GestureListenerInterface listener in lstHeadersdeGestos)
						{
							listener.GestureInProgress(IDJugador1, 0, gestureData.gesture, gestureData.progress, 
								(KinectWrapper.SkeletonJoint)gestureData.joint, gestureData.screenPos);
						}
					}
				}
			}
			
			// Update player 2's models if he/she is calibrated and the model is active.
			if(Jugador2Calibrado)
			{
				foreach (AvatarController controller in lstControladoresdeJugador2)
				{
					//if(controller.Active)
					{
						controller.UpdateAvatar(IDJugador2);
					}
				}

				// Check for complete gestures
				foreach(KinectGestures.GestureData gestureData in lstJugador2Gestos)
				{
					if(gestureData.complete)
					{
						if(gestureData.gesture == KinectGestures.Gestures.Click)
						{
							if(ControldeCursordeMouse)
							{
								MouseControl.MouseClick();
							}
						}
						
						foreach(KinectGestures.GestureListenerInterface listener in lstHeadersdeGestos)
						{
							if(listener.GestureCompleted(IDJugador2, 1, gestureData.gesture, 
								(KinectWrapper.SkeletonJoint)gestureData.joint, gestureData.screenPos))
							{
								ResetPlayerGestures(IDJugador2);
							}
						}
					}
					else if(gestureData.cancelled)
					{
						foreach(KinectGestures.GestureListenerInterface listener in lstHeadersdeGestos)
						{
							if(listener.GestureCancelled(IDJugador2, 1, gestureData.gesture, 
								(KinectWrapper.SkeletonJoint)gestureData.joint))
							{
								ResetGesture(IDJugador2, gestureData.gesture);
							}
						}
					}
					else if(gestureData.progress >= 0.1f)
					{
						if((gestureData.gesture == KinectGestures.Gestures.RightHandCursor || 
							gestureData.gesture == KinectGestures.Gestures.LeftHandCursor) && 
							gestureData.progress >= 0.5f)
						{
							if(GetGestureProgress(gestureData.userId, KinectGestures.Gestures.Click) < 0.3f)
							{
								if(CursordeMano2 != null)
								{
									CursordeMano2.transform.position = Vector3.Lerp(CursordeMano2.transform.position, gestureData.screenPos, 3 * Time.deltaTime);
								}
								
								if(ControldeCursordeMouse)
								{
									MouseControl.MouseMove(gestureData.screenPos);
								}
							}
						}
						
						foreach(KinectGestures.GestureListenerInterface listener in lstHeadersdeGestos)
						{
							listener.GestureInProgress(IDJugador2, 1, gestureData.gesture, gestureData.progress, 
								(KinectWrapper.SkeletonJoint)gestureData.joint, gestureData.screenPos);
						}
					}
				}
			}
		}
		
		// Kill the program with ESC.
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
	
	// Make sure to kill the Kinect on quitting.
	void OnApplicationQuit()
	{
		if(SensorKinectInicializado)
		{
			// Shutdown OpenNI
			KinectWrapper.NuiShutdown();
			administradorKinect = null;
		}
	}
	
	// Draw the Histogram Map on the GUI.
    void OnGUI()
    {
		if(SensorKinectInicializado)
		{
	        if(ProcesarMapadeUsuario && (/**(lstTodoslosUsuarios.Count == 0) ||*/ MostrarMapadeUsuario))
	        {
	            GUI.DrawTexture(RectangulodeMapadeUsuario, lblTexturaUsuarios);
	        }

			else if(ProcesarColordeMapa && (/**(lstTodoslosUsuarios.Count == 0) ||*/ MostrarColordeMapa))
			{
				GUI.DrawTexture(PoligonodeColoresdeUsuario, TexturadeBorradodeUsuarios);
			}
		}
    }
	
	// Update the User Map
    void UpdateUserMap()
    {
        int numOfPoints = 0;
		Array.Clear(arrMapadeHistogramadeUsuarios, 0, arrMapadeHistogramadeUsuarios.Length);

        // Calculate cumulative histogram for depth
        for (int i = 0; i < TamanoMapadeUsuarios; i++)
        {
            // Only calculate for depth that contains users
            if ((arrProfundidaddeMapadeUsuarios[i] & 7) != 0)
            {
				ushort userDepth = (ushort)(arrProfundidaddeMapadeUsuarios[i] >> 3);
                arrMapadeHistogramadeUsuarios[userDepth]++;
                numOfPoints++;
            }
        }
		
        if (numOfPoints > 0)
        {
            for (int i = 1; i < arrMapadeHistogramadeUsuarios.Length; i++)
	        {   
		        arrMapadeHistogramadeUsuarios[i] += arrMapadeHistogramadeUsuarios[i - 1];
	        }
			
            for (int i = 0; i < arrMapadeHistogramadeUsuarios.Length; i++)
	        {
                arrMapadeHistogramadeUsuarios[i] = 1.0f - (arrMapadeHistogramadeUsuarios[i] / numOfPoints);
	        }
        }
		
		// dummy structure needed by the coordinate mapper
        KinectWrapper.NuiImageViewArea pcViewArea = new KinectWrapper.NuiImageViewArea 
		{
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };
		
        // Create the actual users texture based on label map and depth histogram
		Color32 clrClear = Color.clear;
        for (int i = 0; i < TamanoMapadeUsuarios; i++)
        {
	        // Flip the texture as we convert label map to color array
            int flipIndex = i; // TamanoMapadeUsuarios - i - 1;
			
			ushort userMap = (ushort)(arrProfundidaddeMapadeUsuarios[i] & 7);
			ushort userDepth = (ushort)(arrProfundidaddeMapadeUsuarios[i] >> 3);
			
			ushort nowUserPixel = userMap != 0 ? (ushort)((userMap << 13) | userDepth) : userDepth;
			ushort wasUserPixel = EstatusPreviodeUsuario[flipIndex];
			
			// draw only the changed pixels
			if(nowUserPixel != wasUserPixel)
			{
				EstatusPreviodeUsuario[flipIndex] = nowUserPixel;
				
	            if (userMap == 0)
	            {
	                arrColoresdeMapadeUsuario[flipIndex] = clrClear;
	            }
	            else
	            {
					if(arrColoresdeImagen != null)
					{
						int x = i % KinectWrapper.Constants.DepthImageWidth;
						int y = i / KinectWrapper.Constants.DepthImageWidth;
	
						int cx, cy;
						int hr = KinectWrapper.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(
							KinectWrapper.Constants.ColorImageResolution,
							KinectWrapper.Constants.DepthImageResolution,
							ref pcViewArea,
							x, y, arrProfundidaddeMapadeUsuarios[i],
							out cx, out cy);
						
						if(hr == 0)
						{
							int colorIndex = cx + cy * KinectWrapper.Constants.ColorImageWidth;
							//colorIndex = TamanoMapadeUsuarios - colorIndex - 1;
							if(colorIndex >= 0 && colorIndex < TamanoMapadeUsuarios)
							{
								Color32 colorPixel = arrColoresdeImagen[colorIndex];
								arrColoresdeMapadeUsuario[flipIndex] = colorPixel;  // new Color(colorPixel.r / 256f, colorPixel.g / 256f, colorPixel.b / 256f, 0.9f);
								arrColoresdeMapadeUsuario[flipIndex].a = 230; // 0.9f
							}
						}
					}
					else
					{
		                // Create a blending color based on the depth histogram
						float histDepth = arrMapadeHistogramadeUsuarios[userDepth];
		                Color c = new Color(histDepth, histDepth, histDepth, 0.9f);
		                
						switch(userMap % 4)
		                {
		                    case 0:
		                        arrColoresdeMapadeUsuario[flipIndex] = Color.red * c;
		                        break;
		                    case 1:
		                        arrColoresdeMapadeUsuario[flipIndex] = Color.green * c;
		                        break;
		                    case 2:
		                        arrColoresdeMapadeUsuario[flipIndex] = Color.blue * c;
		                        break;
		                    case 3:
		                        arrColoresdeMapadeUsuario[flipIndex] = Color.magenta * c;
		                        break;
		                }
					}
	            }
				
			}
        }
		
		// Draw it!
        lblTexturaUsuarios.SetPixels32(arrColoresdeMapadeUsuario);

		if(!MostrarLineasdeEsqueleto)
		{
			lblTexturaUsuarios.Apply();
		}
	}
	
	// Update the Color Map
	void UpdateColorMap()
	{
        TexturadeBorradodeUsuarios.SetPixels32(arrColoresdeImagen);
        TexturadeBorradodeUsuarios.Apply();
	}
	
	// Assign UserId to player 1 or 2.
    void CalibrateUser(uint UserId, int UserIndex, ref KinectWrapper.NuiSkeletonData skeletonData)
    {
		// If player 1 hasn't been calibrated, assign that UserID to it.
		if(!Jugador1Calibrado)
		{
			// Check to make sure we don't accidentally assign player 2 to player 1.
			if (!lstTodoslosUsuarios.Contains(UserId))
			{
				if(CheckForCalibrationPose(UserId, ref PosturadeCalibraciondeJugador1, ref datosdeCalibracionparaJugador1, ref skeletonData))
				{
					Jugador1Calibrado = true;
					IDJugador1 = UserId;
					IndicedeJugador1 = UserIndex;
					
					lstTodoslosUsuarios.Add(UserId);
					
					foreach(AvatarController controller in lstControladoresdeJugador1)
					{
						controller.SuccessfulCalibration(UserId);
					}
	
					// add the gestures to detect, if any
					foreach(KinectGestures.Gestures gesture in lstGestosJugador1)
					{
						DetectGesture(UserId, gesture);
					}
					
					// notify the gesture listeners about the new user
					foreach(KinectGestures.GestureListenerInterface listener in lstHeadersdeGestos)
					{
						listener.UserDetected(UserId, 0);
					}
					
					// reset skeleton filters
					ResetFilters();
					
					// If we're not using 2 users, we're all calibrated.
					//if(!DosUsuarios)
					{
						TodoslosJugadoresEstanCalibrados = !DosUsuarios ? lstTodoslosUsuarios.Count >= 1 : lstTodoslosUsuarios.Count >= 2; // true;
					}
				}
			}
		}
		// Otherwise, assign to player 2.
		else if(DosUsuarios && !Jugador2Calibrado)
		{
			if (!lstTodoslosUsuarios.Contains(UserId))
			{
				if(CheckForCalibrationPose(UserId, ref PosturadeCalibraciondeJugador2, ref darosdeCalibracionparaJugador2, ref skeletonData))
				{
					Jugador2Calibrado = true;
					IDJugador2 = UserId;
					IndicedeJugador2 = UserIndex;
					
					lstTodoslosUsuarios.Add(UserId);
					
					foreach(AvatarController controller in lstControladoresdeJugador2)
					{
						controller.SuccessfulCalibration(UserId);
					}
					
					// add the gestures to detect, if any
					foreach(KinectGestures.Gestures gesture in lstGestosJugador2)
					{
						DetectGesture(UserId, gesture);
					}
					
					// notify the gesture listeners about the new user
					foreach(KinectGestures.GestureListenerInterface listener in lstHeadersdeGestos)
					{
						listener.UserDetected(UserId, 1);
					}
					
					// reset skeleton filters
					ResetFilters();
					
					// All users are calibrated!
					TodoslosJugadoresEstanCalibrados = !DosUsuarios ? lstTodoslosUsuarios.Count >= 1 : lstTodoslosUsuarios.Count >= 2; // true;
				}
			}
		}
		
		// If all users are calibrated, stop trying to find them.
		if(TodoslosJugadoresEstanCalibrados)
		{
			Debug.Log("All players calibrated.");
			
			if(TextodeCalibracion != null)
			{
				TextodeCalibracion.guiText.text = "";
			}
		}
    }
	
	// Remove a lost UserId
	void RemoveUser(uint UserId)
	{
		// If we lose player 1...
		if(UserId == IDJugador1)
		{
			// Null out the ID and reset all the models associated with that ID.
			IDJugador1 = 0;
			IndicedeJugador1 = 0;
			Jugador1Calibrado = false;
			
			foreach(AvatarController controller in lstControladoresdeJugador1)
			{
				controller.RotateToCalibrationPose(UserId, IsCalibrationNeeded());
			}
			
			foreach(KinectGestures.GestureListenerInterface listener in lstHeadersdeGestos)
			{
				listener.UserLost(UserId, 0);
			}
			
			datosdeCalibracionparaJugador1.userId = 0;
		}
		
		// If we lose player 2...
		if(UserId == IDJugador2)
		{
			// Null out the ID and reset all the models associated with that ID.
			IDJugador2 = 0;
			IndicedeJugador2 = 0;
			Jugador2Calibrado = false;
			
			foreach(AvatarController controller in lstControladoresdeJugador2)
			{
				controller.RotateToCalibrationPose(UserId, IsCalibrationNeeded());
			}
			
			foreach(KinectGestures.GestureListenerInterface listener in lstHeadersdeGestos)
			{
				listener.UserLost(UserId, 1);
			}
			
			darosdeCalibracionparaJugador2.userId = 0;
		}
		
		// clear gestures list for this user
		ClearGestures(UserId);

        // remove from global users list
        lstTodoslosUsuarios.Remove(UserId);
		TodoslosJugadoresEstanCalibrados = !DosUsuarios ? lstTodoslosUsuarios.Count >= 1 : lstTodoslosUsuarios.Count >= 2; // false;
		
		// Try to replace that user!
		Debug.Log("Waiting for users.");

		if(TextodeCalibracion != null)
		{
			TextodeCalibracion.guiText.text = "WAITING FOR USERS";
		}
	}
	
	// Some internal constants
	private const int stateTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.Tracked;
	private const int stateNotTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.NotTracked;
	
	private int [] mustBeTrackedJoints = { 
		(int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft,
		(int)KinectWrapper.NuiSkeletonPositionIndex.FootLeft,
		(int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight,
		(int)KinectWrapper.NuiSkeletonPositionIndex.FootRight,
	};
	
	// Process the skeleton data
	void ProcessSkeleton()
	{
		List<uint> lostUsers = new List<uint>();
		lostUsers.AddRange(lstTodoslosUsuarios);
		
		// calculate the time since last update
		float currentNuiTime = Time.realtimeSinceStartup;
		float deltaNuiTime = currentNuiTime - nuiUltimaVez;
		
		for(int i = 0; i < KinectWrapper.Constants.NuiSkeletonCount; i++)
		{
			KinectWrapper.NuiSkeletonData skeletonData = MarcodelEsqueleto.SkeletonData[i];
			uint userId = skeletonData.dwTrackingID;
			
			if(skeletonData.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked)
			{
				// get the skeleton position
				Vector3 skeletonPos = kinectToWorld.MultiplyPoint3x4(skeletonData.Position);
				
				if(!TodoslosJugadoresEstanCalibrados)
				{
					// check if this is the closest user
					bool bClosestUser = true;
					
					if(DetectaCercaniadeUsuario)
					{
						for(int j = 0; j < KinectWrapper.Constants.NuiSkeletonCount; j++)
						{
							if(j != i)
							{
								KinectWrapper.NuiSkeletonData skeletonDataOther = MarcodelEsqueleto.SkeletonData[j];
								
								if((skeletonDataOther.eTrackingState == KinectWrapper.NuiSkeletonTrackingState.SkeletonTracked) &&
									(Mathf.Abs(kinectToWorld.MultiplyPoint3x4(skeletonDataOther.Position).z) < Mathf.Abs(skeletonPos.z)))
								{
									bClosestUser = false;
									break;
								}
							}
						}
					}
					
					if(bClosestUser)
					{
						CalibrateUser(userId, i + 1, ref skeletonData);
					}
				}

				//// get joints orientations
				//KinectWrapper.NuiSkeletonBoneOrientation[] jointOrients = new KinectWrapper.NuiSkeletonBoneOrientation[(int)KinectWrapper.NuiSkeletonPositionIndex.Count];
				//KinectWrapper.NuiSkeletonCalculateBoneOrientations(ref skeletonData, jointOrients);
				
				if(userId == IDJugador1 && Mathf.Abs(skeletonPos.z) >= DistanciaMinimaalSensor &&
				   (DistanciaMaximaalSensor <= 0f || Mathf.Abs(skeletonPos.z) <= DistanciaMaximaalSensor))
				{
					IniciodeJugador1 = i;

					// get player position
					PosicionJugador1 = skeletonPos;
					
					// apply tracking state filter first
					arrFiltrodeEstadodeRastreo[0].UpdateFilter(ref skeletonData);
					
					// fixup skeleton to improve avatar appearance.
					if(UsarFiltrodePiernasCortadas && arrFiltrodePiernasCortadas[0] != null)
					{
						arrFiltrodePiernasCortadas[0].FilterSkeleton(ref skeletonData, deltaNuiTime);
					}
	
					if(UsarRestricciondeAutointerseccion && RestricciondeInterseccion != null)
					{
						RestricciondeInterseccion.Constrain(ref skeletonData);
					}
	
					// get joints' position and rotation
					for (int j = 0; j < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; j++)
					{
						bool playerTracked = IgnorarConjuntosdeArticulaciones ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
							(Array.BinarySearch(mustBeTrackedJoints, j) >= 0 ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
							(int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked);
						arrArticulacionesdeJugador1Detectadas[j] = player1PrevTracked[j] && playerTracked;
						player1PrevTracked[j] = playerTracked;
						
						if(arrArticulacionesdeJugador1Detectadas[j])
						{
							arrPOsiciondeArticulacionesJugador1[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
							//OrientaciondeArticulacionesJugador1[j] = jointOrients[j].absoluteRotation.rotationMatrix * MatrizdeMovimientos;
						}
						
//						if(j == (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter)
//						{
//							string debugText = String.Format("{0} {1}", /**(int)skeletonData.eSkeletonPositionTrackingState[j], */
//								arrArticulacionesdeJugador1Detectadas[j] ? "T" : "F", arrPOsiciondeArticulacionesJugador1[j]/**, skeletonData.SkeletonPositions[j]*/);
//							
//							if(TextodeCalibracion)
//								TextodeCalibracion.guiText.text = debugText;
//						}
					}
					
					// draw the skeleton on top of texture
					if(MostrarLineasdeEsqueleto && ProcesarMapadeUsuario)
					{
						DrawSkeleton(lblTexturaUsuarios, ref skeletonData, ref arrArticulacionesdeJugador1Detectadas);
						lblTexturaUsuarios.Apply();
					}
					
					// calculate joint orientations
					KinectWrapper.GetSkeletonJointOrientation(ref arrPOsiciondeArticulacionesJugador1, ref arrArticulacionesdeJugador1Detectadas, ref OrientaciondeArticulacionesJugador1);
					
					// filter orientation constraints
					if(UsarRestricciondeOrientacionesdeHueso && FiltrodeRestriccionesdeHuesos != null)
					{
						FiltrodeRestriccionesdeHuesos.Constrain(ref OrientaciondeArticulacionesJugador1, ref arrArticulacionesdeJugador1Detectadas);
					}
					
                    // filter joint orientations.
                    // it should be performed after all joint position modifications.
	                if(UsarFiltrodeOrientacionesdeHueso && arrFiltrodeOrientaciondeHuesos[0] != null)
	                {
	                    arrFiltrodeOrientaciondeHuesos[0].UpdateFilter(ref skeletonData, ref OrientaciondeArticulacionesJugador1);
	                }
	
					// get player rotation
					OrientaciondeJugador1 = OrientaciondeArticulacionesJugador1[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];
					
					// check for gestures
					if(Time.realtimeSinceStartup >= arrRastreodeGestosenTiempo[0])
					{
						int listGestureSize = lstJugador1Gestos.Count;
						float timestampNow = Time.realtimeSinceStartup;
						
						for(int g = 0; g < listGestureSize; g++)
						{
							KinectGestures.GestureData gestureData = lstJugador1Gestos[g];
							
							if((timestampNow >= gestureData.startTrackingAtTime) && 
								!IsConflictingGestureInProgress(gestureData))
							{
								KinectGestures.CheckForGesture(userId, ref gestureData, Time.realtimeSinceStartup, 
									ref arrPOsiciondeArticulacionesJugador1, ref arrArticulacionesdeJugador1Detectadas);
								lstJugador1Gestos[g] = gestureData;

								if(gestureData.complete)
								{
									arrRastreodeGestosenTiempo[0] = timestampNow + TiempoMinimoEntreGestos;
								}
							}
						}
					}
				}
				else if(userId == IDJugador2 && Mathf.Abs(skeletonPos.z) >= DistanciaMinimaalSensor &&
				        (DistanciaMaximaalSensor <= 0f || Mathf.Abs(skeletonPos.z) <= DistanciaMaximaalSensor))
				{
					IniciodeJugador2 = i;

					// get player position
					PosicionJugador2 = skeletonPos;
					
					// apply tracking state filter first
					arrFiltrodeEstadodeRastreo[1].UpdateFilter(ref skeletonData);
					
					// fixup skeleton to improve avatar appearance.
					if(UsarFiltrodePiernasCortadas && arrFiltrodePiernasCortadas[1] != null)
					{
						arrFiltrodePiernasCortadas[1].FilterSkeleton(ref skeletonData, deltaNuiTime);
					}
	
					if(UsarRestricciondeAutointerseccion && RestricciondeInterseccion != null)
					{
						RestricciondeInterseccion.Constrain(ref skeletonData);
					}

					// get joints' position and rotation
					for (int j = 0; j < (int)KinectWrapper.NuiSkeletonPositionIndex.Count; j++)
					{
						bool playerTracked = IgnorarConjuntosdeArticulaciones ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
							(Array.BinarySearch(mustBeTrackedJoints, j) >= 0 ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
							(int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked);
						arrArticulacionesdeJugador2Detectadas[j] = player2PrevTracked[j] && playerTracked;
						player2PrevTracked[j] = playerTracked;
						
						if(arrArticulacionesdeJugador2Detectadas[j])
						{
							arrPosiciondeArticulacionesJugador2[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
						}
					}
					
					// draw the skeleton on top of texture
					if(MostrarLineasdeEsqueleto && ProcesarMapadeUsuario)
					{
						DrawSkeleton(lblTexturaUsuarios, ref skeletonData, ref arrArticulacionesdeJugador2Detectadas);
						lblTexturaUsuarios.Apply();
					}
					
					// calculate joint orientations
					KinectWrapper.GetSkeletonJointOrientation(ref arrPosiciondeArticulacionesJugador2, ref arrArticulacionesdeJugador2Detectadas, ref OrientaciondeArticulacionesJugador2);
					
					// filter orientation constraints
					if(UsarRestricciondeOrientacionesdeHueso && FiltrodeRestriccionesdeHuesos != null)
					{
						FiltrodeRestriccionesdeHuesos.Constrain(ref OrientaciondeArticulacionesJugador2, ref arrArticulacionesdeJugador2Detectadas);
					}
					
                    // filter joint orientations.
                    // it should be performed after all joint position modifications.
	                if(UsarFiltrodeOrientacionesdeHueso && arrFiltrodeOrientaciondeHuesos[1] != null)
	                {
	                    arrFiltrodeOrientaciondeHuesos[1].UpdateFilter(ref skeletonData, ref OrientaciondeArticulacionesJugador2);
	                }
	
					// get player rotation
					OrientaciondeJugador2 = OrientaciondeArticulacionesJugador2[(int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter];
					
					// check for gestures
					if(Time.realtimeSinceStartup >= arrRastreodeGestosenTiempo[1])
					{
						int listGestureSize = lstJugador2Gestos.Count;
						float timestampNow = Time.realtimeSinceStartup;
						
						for(int g = 0; g < listGestureSize; g++)
						{
							KinectGestures.GestureData gestureData = lstJugador2Gestos[g];
							
							if((timestampNow >= gestureData.startTrackingAtTime) &&
								!IsConflictingGestureInProgress(gestureData))
							{
								KinectGestures.CheckForGesture(userId, ref gestureData, Time.realtimeSinceStartup, 
									ref arrPosiciondeArticulacionesJugador2, ref arrArticulacionesdeJugador2Detectadas);
								lstJugador2Gestos[g] = gestureData;

								if(gestureData.complete)
								{
									arrRastreodeGestosenTiempo[1] = timestampNow + TiempoMinimoEntreGestos;
								}
							}
						}
					}
				}
				
				lostUsers.Remove(userId);
			}
		}
		
		// update the nui-timer
		nuiUltimaVez = currentNuiTime;
		
		// remove the lost users if any
		if(lostUsers.Count > 0)
		{
			foreach(uint userId in lostUsers)
			{
				RemoveUser(userId);
			}
			
			lostUsers.Clear();
		}
	}
	
	// draws the skeleton in the given texture
	private void DrawSkeleton(Texture2D aTexture, ref KinectWrapper.NuiSkeletonData skeletonData, ref bool[] playerJointsTracked)
	{
		int jointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
		
		for(int i = 0; i < jointsCount; i++)
		{
			int parent = KinectWrapper.GetSkeletonJointParent(i);
			
			if(playerJointsTracked[i] && playerJointsTracked[parent])
			{
				Vector3 posParent = KinectWrapper.MapSkeletonPointToDepthPoint(skeletonData.SkeletonPositions[parent]);
				Vector3 posJoint = KinectWrapper.MapSkeletonPointToDepthPoint(skeletonData.SkeletonPositions[i]);
				
//				posParent.y = KinectWrapper.Constants.ImageHeight - posParent.y - 1;
//				posJoint.y = KinectWrapper.Constants.ImageHeight - posJoint.y - 1;
//				posParent.x = KinectWrapper.Constants.ImageWidth - posParent.x - 1;
//				posJoint.x = KinectWrapper.Constants.ImageWidth - posJoint.x - 1;
				
				//Color lineColor = playerJointsTracked[i] && playerJointsTracked[parent] ? Color.red : Color.yellow;
				DrawLine(aTexture, (int)posParent.x, (int)posParent.y, (int)posJoint.x, (int)posJoint.y, Color.yellow);
			}
		}
	}
	
	// draws a line in a texture
	private void DrawLine(Texture2D a_Texture, int x1, int y1, int x2, int y2, Color a_Color)
	{
		int width = a_Texture.width;  // KinectWrapper.Constants.DepthImageWidth;
		int height = a_Texture.height;  // KinectWrapper.Constants.DepthImageHeight;
		
		int dy = y2 - y1;
		int dx = x2 - x1;
	 
		int stepy = 1;
		if (dy < 0) 
		{
			dy = -dy; 
			stepy = -1;
		}
		
		int stepx = 1;
		if (dx < 0) 
		{
			dx = -dx; 
			stepx = -1;
		}
		
		dy <<= 1;
		dx <<= 1;
	 
		if(x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
			for(int x = -1; x <= 1; x++)
				for(int y = -1; y <= 1; y++)
					a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
		
		if (dx > dy) 
		{
			int fraction = dy - (dx >> 1);
			
			while (x1 != x2) 
			{
				if (fraction >= 0) 
				{
					y1 += stepy;
					fraction -= dx;
				}
				
				x1 += stepx;
				fraction += dy;
				
				if(x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
					for(int x = -1; x <= 1; x++)
						for(int y = -1; y <= 1; y++)
							a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
			}
		}
		else 
		{
			int fraction = dx - (dy >> 1);
			
			while (y1 != y2) 
			{
				if (fraction >= 0) 
				{
					x1 += stepx;
					fraction -= dy;
				}
				
				y1 += stepy;
				fraction += dx;
				
				if(x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
					for(int x = -1; x <= 1; x++)
						for(int y = -1; y <= 1; y++)
							a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
			}
		}
		
	}
	
	// convert the matrix to quaternion, taking care of the mirroring
	private Quaternion ConvertMatrixToQuat(Matrix4x4 mOrient, int joint, bool flip)
	{
		Vector4 vZ = mOrient.GetColumn(2);
		Vector4 vY = mOrient.GetColumn(1);

		if(!flip)
		{
			vZ.y = -vZ.y;
			vY.x = -vY.x;
			vY.z = -vY.z;
		}
		else
		{
			vZ.x = -vZ.x;
			vZ.y = -vZ.y;
			vY.z = -vY.z;
		}
		
		if(vZ.x != 0.0f || vZ.y != 0.0f || vZ.z != 0.0f)
			return Quaternion.LookRotation(vZ, vY);
		else
			return Quaternion.identity;
	}
	
	// return the index of gesture in the list, or -1 if not found
	private int GetGestureIndex(uint UserId, KinectGestures.Gestures gesture)
	{
		if(UserId == IDJugador1)
		{
			int listSize = lstJugador1Gestos.Count;
			for(int i = 0; i < listSize; i++)
			{
				if(lstJugador1Gestos[i].gesture == gesture)
					return i;
			}
		}
		else if(UserId == IDJugador2)
		{
			int listSize = lstJugador2Gestos.Count;
			for(int i = 0; i < listSize; i++)
			{
				if(lstJugador2Gestos[i].gesture == gesture)
					return i;
			}
		}
		
		return -1;
	}
	
	private bool IsConflictingGestureInProgress(KinectGestures.GestureData gestureData)
	{
		foreach(KinectGestures.Gestures gesture in gestureData.checkForGestures)
		{
			int index = GetGestureIndex(gestureData.userId, gesture);
			
			if(index >= 0)
			{
				if(gestureData.userId == IDJugador1)
				{
					if(lstJugador1Gestos[index].progress > 0f)
						return true;
				}
				else if(gestureData.userId == IDJugador2)
				{
					if(lstJugador2Gestos[index].progress > 0f)
						return true;
				}
			}
		}
		
		return false;
	}
	
	// check if the calibration pose is complete for given user
	private bool CheckForCalibrationPose(uint userId, ref KinectGestures.Gestures calibrationGesture, 
		ref KinectGestures.GestureData gestureData, ref KinectWrapper.NuiSkeletonData skeletonData)
	{
		if(calibrationGesture == KinectGestures.Gestures.None)
			return true;
		
		// init gesture data if needed
		if(gestureData.userId != userId)
		{
			gestureData.userId = userId;
			gestureData.gesture = calibrationGesture;
			gestureData.state = 0;
			gestureData.joint = 0;
			gestureData.progress = 0f;
			gestureData.complete = false;
			gestureData.cancelled = false;
		}
		
		// get temporary joints' position
		int skeletonJointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;
		bool[] jointsTracked = new bool[skeletonJointsCount];
		Vector3[] jointsPos = new Vector3[skeletonJointsCount];

		int stateTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.Tracked;
		int stateNotTracked = (int)KinectWrapper.NuiSkeletonPositionTrackingState.NotTracked;
		
		int [] mustBeTrackedJoints = { 
			(int)KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft,
			(int)KinectWrapper.NuiSkeletonPositionIndex.FootLeft,
			(int)KinectWrapper.NuiSkeletonPositionIndex.AnkleRight,
			(int)KinectWrapper.NuiSkeletonPositionIndex.FootRight,
		};
		
		for (int j = 0; j < skeletonJointsCount; j++)
		{
			jointsTracked[j] = Array.BinarySearch(mustBeTrackedJoints, j) >= 0 ? (int)skeletonData.eSkeletonPositionTrackingState[j] == stateTracked :
				(int)skeletonData.eSkeletonPositionTrackingState[j] != stateNotTracked;
			
			if(jointsTracked[j])
			{
				jointsPos[j] = kinectToWorld.MultiplyPoint3x4(skeletonData.SkeletonPositions[j]);
			}
		}
		
		// estimate the gesture progess
		KinectGestures.CheckForGesture(userId, ref gestureData, Time.realtimeSinceStartup, 
			ref jointsPos, ref jointsTracked);
		
		// check if gesture is complete
		if(gestureData.complete)
		{
			gestureData.userId = 0;
			return true;
		}
		
		return false;
	}
	
}


