using UnityEngine;
using System.Collections;

public class AVOWTab : MonoBehaviour {

	
	public enum State{
		kNormal,
		kOver,
		kPressed,
		kSelected,
		kNumStates
	};
	
	public Color[] cols = new Color[(int)State.kNumStates];
	public State state;
	State oldState = State.kNumStates;
	
	bool isMouseInside = false;
	bool isMouseSelected = false;
	bool isMouseDown = false;
	
	
	// Use this for initialization
	void Start () {
		AVOWUI.singleton.RegisterTab(this);
	
	}
	
	public bool IsContaining(Vector3 worldPos){
		worldPos.z = transform.position.z;
		return renderer.bounds.Contains(worldPos);
	}
	
	public void SetSelected(bool isSelected){
		isMouseSelected = isSelected;
	}
	
	public void SetMouseInside(bool isInside){
		isMouseInside = isInside;
	}
	
	void SetMouseDown(bool isDown){
		isMouseDown = isDown;
		
	}
	

	
	// Update is called once per frame
	void Update () {
		// Do the state logic
		if (isMouseInside && isMouseDown){
			state = State.kPressed;
		}
		else if (isMouseInside){
			state = State.kOver;
		}
		else if (isMouseSelected){
			state = State.kSelected;
		}		
		else{
			state = State.kNormal;
		}
		
		// If the state has chnaged, inform our material
		if (state != oldState){
			oldState = state;
			renderer.material.SetColor("_Color", cols[(int)state]);
		}
		
		
	}
}
