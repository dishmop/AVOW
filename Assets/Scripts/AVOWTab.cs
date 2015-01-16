﻿using UnityEngine;
using System.Collections;

public class AVOWTab : MonoBehaviour {

	
	public enum State {
		kNormal,
		kOver,
		kPressed,
		kSelected,
		kNumStates
	};
	
	public Color[] cols = new Color[(int)State.kNumStates];
	public State state;
	public AVOWGraph.Node thisNode;
	public AVOWComponent thisComponent;
	
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
		int nodeID = thisNode.id + 1;
		/*
		// Create a standard colour
		int red = (nodeID * 89) % 256;
		int green = (nodeID * 137) % 256;
		int blue = (nodeID * 34) % 256;
		
		Color newCol = new Color(red / 256.0f, green / 256.0f, blue / 256.0f);
		cols[(int)State.kNormal] = 	Color.Lerp (newCol, new Color(0.5f, 0.5f, 0.5f), 0.8f);
		cols[(int)State.kOver] = 	Color.Lerp (newCol, new Color(0.5f, 0.5f, 0.5f), 0.4f);
		cols[(int)State.kPressed] = Color.Lerp (newCol, new Color(1, 1, 1), 0.4f);
		cols[(int)State.kSelected] =newCol;
		*/
	}
	
	public void SetAVOWComponent(AVOWComponent component){
		thisComponent = component;
	}	
	

	public AVOWGraph.Node GetNode(){
		return thisNode;
		
	}
	
	public AVOWComponent GetAVOWComponent(){
		return thisComponent;
		
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
		if (state != oldState || true){
			oldState = state;
			renderer.material.SetColor("_Color", cols[(int)state]);
		}
		
		
	}
}
