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
	public AVOWGraph.Node thisNode;
	
	State oldState = State.kNumStates;
	
	bool isMouseInside = false;
	bool isMouseSelected = false;

	
	
	// Use this for initialization
	void Start () {
		AVOWUI.singleton.RegisterTab(this);
	
	}
	
	void OnDestroy(){
		if (AVOWUI.singleton){
			AVOWUI.singleton.UnregisterTab(this);
		}
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
	
	public void SetNode(AVOWGraph.Node node){
		thisNode = node;
	}
	

	public AVOWGraph.Node GetNode(){
		return thisNode;
		
	}
	
	

	
	// Update is called once per frame
	void Update () {
		// Do the state logic
		if (isMouseSelected){
			state = State.kSelected;
		}		
		else if (isMouseInside){
			state = State.kOver;
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
