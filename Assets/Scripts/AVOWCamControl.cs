using UnityEngine;
using System.Collections;

public class AVOWCamControl : MonoBehaviour {

	public static AVOWCamControl singleton = null;


	public GameObject		sidePanel;
	public GameObject		topPanel;
	public float 			zoomSpeed;
	public bool				ignoreSide = false;
	public float xFOV		= 1.7f;
	public float yFOV		= 1.7f;
	public float xOffset 	= 0;
	public bool disableMovement = false;
	Vector3					prevMousePos = new Vector3();
	
	
	int delay = 0;
	
	// Use this for initialization
	void Start () {
	

	
	}
	
	// Update is called once per frame
	public void RenderUpdate () {
		delay--;
		if (delay > 0) return;

		// Even if movement disabled - we still can change the viewport size		
		GetComponent<Camera>().rect = new Rect(AVOWConfig.singleton.GetSidePanelFrac(), AVOWConfig.singleton.GetBottomPanelFrac(), 1, 1);

		if (disableMovement) return;
		
		Camera camera = gameObject.GetComponent<Camera>();
		float wheelVal = 0;//Input.GetAxis("Mouse ScrollWheel");
		
		// We need to offset the camera so that the mouse pointer 
		// remains in the same position - in which case, get its current position in world space
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = transform.position.z - Camera.main.transform.position.z;
		
		
		
		if (wheelVal != 0){
			mousePos.z = transform.position.z - Camera.main.transform.position.z;
			Vector3 mouseOldWorldPos = camera.ScreenToWorldPoint( mousePos);

			camera.orthographicSize *= Mathf.Pow (zoomSpeed, -wheelVal);
		
			Vector3 mouseNewWorldPos = camera.ScreenToWorldPoint( mousePos);
			
			// Offset the camera so that the mouse is pointing at the same world position
			Vector3 camPos = gameObject.transform.position;
			camPos += (mouseOldWorldPos - mouseNewWorldPos);
			gameObject.transform.position = camPos;
		}
		
		
		if (Input.GetMouseButton(1) || (Input.GetMouseButton(0) && Input.GetKey (KeyCode.LeftControl))){
			Vector3 mouseOldWorldPos = camera.ScreenToWorldPoint( prevMousePos);
			Vector3 mouseNewWorldPos = camera.ScreenToWorldPoint( mousePos);
			
			// Offset the camera so that the mouse is pointing at the same world position
			Vector3 camPos = gameObject.transform.position;
			camPos += (mouseOldWorldPos - mouseNewWorldPos);
			gameObject.transform.position = camPos;
			
		
		}
		prevMousePos = mousePos;
		
		// If mouse is not held down
		float prop = 0.1f;
		if (!Input.GetMouseButton(0)){
			// the -0.5f si the battery
			float gameBoardXMin = AVOWObjectiveManager.singleton.GetMinX();
			float simMax = Mathf.Max (1, AVOWSim.singleton.xMax);
			Rect bounds = new Rect(gameBoardXMin, AVOWSim.singleton.yMin, simMax  - gameBoardXMin , AVOWSim.singleton.yMax - AVOWSim.singleton.yMin);
			
		//	bounds.xMax = Mathf.Max(bounds.xMax, 1);
			
			
			Vector2 centre = new Vector2((bounds.xMin + bounds.xMax) * 0.5f + xOffset, (bounds.yMin + bounds.yMax) * 0.5f);
			Vector3 pos = gameObject.transform.position;
			pos.x = Mathf.Lerp(pos.x, centre.x, prop);
			pos.y = Mathf.Lerp(pos.y, centre.y, prop);
			gameObject.transform.position = pos;
			
			Vector2 size = new Vector2( bounds.xMax - bounds.xMin, bounds.yMax - bounds.yMin);
			
			float vSize0 = size.y;
			float vSize1 = size.x / Camera.main.aspect;
			float useVSize =  Mathf.Max (yFOV *vSize0, xFOV *vSize1);
			
			float currentSize = transform.GetComponent<Camera>().orthographicSize;
			float newSize = Mathf.Lerp (currentSize, useVSize * 0.5f, prop);
			
			transform.GetComponent<Camera>().orthographicSize = newSize;
			
		}
			
		
	}
	
	public void AddOffset(Vector3 offset){
		// Offset the camera so that the mouse is pointing at the same world position
		Vector3 camPos = gameObject.transform.position;
		camPos += offset;
		gameObject.transform.position = camPos;		
	}
	
	public void CentreCamera(){
//		Rect bounds = Circuit.singleton.bounds;
//		if (bounds.width >= 1 || bounds.height >= 1){
//		
//			// What proportion of the screen is taken up with the side panel
////			float hPropSide  = 0f;
////			if (!ignoreSide){
////				hPropSide = sidePanel.GetComponent<RectTransform>().anchorMax.x;
////			}
////			float propHScreen = (1f-hPropSide);
////			
////			float vPropSide = 0f;
////			if (topPanel.activeSelf){
////				vPropSide = 1f - topPanel.GetComponent<RectTransform>().anchorMin.y;
////			}
////			float propVScreen = (1f-vPropSide);
////			
//			// Override this stuff
//			float propVScreen = 1;
//			float propHScreen = 1;
//			
//			float sideX = sidePanel.GetComponent<RectTransform>().anchorMax.x;
//			float topY = 1;
//			if (topPanel.activeSelf){
//				topY = topPanel.GetComponent<RectTransform>().anchorMin.y;
//			}
//			Rect camRect = GetComponent<Camera>().rect;
//			camRect.x = sideX;
//			camRect.height = topY;		
//			GetComponent<Camera>().rect = camRect;			
//			
//			
//			// This means that the screen's width is actuall 1-that
//			float adjustedAspect = propHScreen * Camera.main.aspect /  propVScreen;
//		
//			// Get range and work out orthographic size			
//			float border = 1;
//			float vRange = Mathf.Max (bounds.height + 2 * border, (bounds.width + 2 * border) / adjustedAspect);
//			transform.GetComponent<Camera>().orthographicSize = vRange * 0.5f;
//			
//			// calc how much extra of the scene to add on to the left to ensure we are looking in the middle
//			float extraX = (bounds.width + 2 * border) *(1f/propHScreen - 1);
//			float extraY = (bounds.height + 2 * border) *(1f/propVScreen - 1);
//			
//			Vector2 centre = new Vector2(((bounds.xMin - extraX) + bounds.xMax) / 2f, (bounds.yMin + extraY + bounds.yMax) / 2f);
//			Vector3 newCamPos = new Vector3(centre.x, centre.y, -10);
//			transform.position = newCamPos;
			
//		}
	}
	// Use this for initialization
	void Awake () {
		if (singleton != null) Debug.LogError ("Error assigning singleton");
		singleton = this;
		
	}
	
	
	void OnDestroy(){
		
		singleton = null;
	}	
}
