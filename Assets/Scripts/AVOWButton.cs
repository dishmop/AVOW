using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AVOWButton : MonoBehaviour {

	public Color[] color = new Color[(int)State.kNumColors];

	public enum State{
		kDisabled,
		kNormal,
		kOver,
		kSelected,
		kNumColors
	}
	public State state = State.kNormal;
	
	public UnityEngine.Events.UnityEvent onClick;
	
	public AVOWUI.ToolMode mode;

	// Use this for initialization
	void Start () {
	
		// Each button should have its own copy of the material
		Material newMat = UnityEngine.Object.Instantiate(transform.FindChild("SizedPanel").FindChild("Image").GetComponent<Image>().material);
		transform.FindChild("SizedPanel").FindChild("Image").GetComponent<Image>().material = newMat;
		
	
	}
	

	
	// Update is called once per frame
	void Update () {
	
		Vector2 mousePos = Input.mousePosition;
		bool inside = IsInsideRect(transform.FindChild("SizedPanel").GetComponent<RectTransform>(), mousePos);
		
		
		int numComponentsLeft = (AVOWConfig.singleton.maxNumResistors - AVOWGraph.singleton.allComponents.Count + 1);
		if (!AVOWConfig.singleton.noResistorLimit && mode == AVOWUI.ToolMode.kCreate && numComponentsLeft <= 0){
			state = State.kDisabled;
			
		}
		else if (AVOWConfig.singleton.tutDisableCreateUIButton&& mode == AVOWUI.ToolMode.kCreate ){
			state = State.kDisabled;
		
		}
		else if (AVOWConfig.singleton.tutDisableDestroyUIButton&& mode == AVOWUI.ToolMode.kDelete ){
			state = State.kDisabled;
			
		}
		else{
			state = State.kNormal;
		}
		
		if (state != State.kDisabled && AVOWUI.singleton.mode == mode){
			state = State.kSelected;
		}
		if (state == State.kSelected && AVOWUI.singleton.mode != mode){
			state = State.kNormal;
		}
		
		if (state == State.kNormal && inside){	
			state = State.kOver;
		}
		else if (state == State.kOver && !inside){
			state = State.kNormal;
		}
		
		if (Input.GetMouseButtonDown(0) && state == State.kOver){
			onClick.Invoke();
		}
		

		
		

		transform.FindChild("SizedPanel").FindChild("Image").GetComponent<Image>().material.SetColor ("_Color", color[(int)state]);
		transform.FindChild("SizedPanel").FindChild("Label").GetComponent<Text>().color = color[(int)state] * 20;
		transform.FindChild("SizedPanel").GetComponent<Image>().material = state == State.kSelected ? transform.FindChild("SizedPanel").FindChild("Image").GetComponent<Image>().material : null;
	}
	
	
	bool IsInsideRect(RectTransform rect, Vector2 screenPos){
		Vector3[] fourCorners = new Vector3[4];
		rect.GetWorldCorners(fourCorners);
		
		float xMin = Screen.width;
		float xMax = 0;
		float yMin = Screen.height;
		float yMax = 0;
		foreach (Vector3 corner in fourCorners){
			//Vector3 screenCoord = RectTransformUtility.WorldToScreenPoint(Camera.main, corner);
			xMin = Mathf.Min (xMin, corner.x);
			xMax = Mathf.Max (xMax, corner.x);
			yMin = Mathf.Min (yMin, corner.y);
			yMax = Mathf.Max (yMax, corner.y);
		}
		return screenPos.x >= xMin && screenPos.x <= xMax && screenPos.y >= yMin && screenPos.y <= yMax;
		
	}
}
