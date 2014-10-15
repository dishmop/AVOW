﻿using UnityEngine;
using System.Collections;

public class CircuitElementWire : CircuitElement {

	public GameObject wirePointPrefab;
	public GameObject wireStraightDownPrefab;
	public GameObject wireDownLeftCornerPrefab;
	public GameObject wireDeadEndDownPrefab;
	public GameObject wireCrossPrefab;
	public GameObject wireTJuncFromTopPrefab;
		
	public GameObject 	currentPrefab;
	public GameObject	currentDisplay;
	public float resistance = 0;
	
	
	public void Start(){
		Debug.Log ("CircuitElementWire:Start()");
	}
	
	public override void SetupMesh(){
		// Placeholder
		GameObject newPrefab;
		int newOrient = -1;
		
		// No connections
		if (HasConnections(false, false, false, false)){
			newPrefab = wirePointPrefab;
			newOrient = 0;
		}
		// 1 Connection
		else if (HasConnections(true, false, false, false)){
			newPrefab = wireDeadEndDownPrefab;
			newOrient = 0;
		}
		else if (HasConnections(false, true, false, false)){
			newPrefab = wireDeadEndDownPrefab;
			newOrient = 3;
		}
		else if (HasConnections(false, false, true, false)){
			newPrefab = wireDeadEndDownPrefab;
			newOrient = 2;
		}
		else if (HasConnections(false, false, false, true)){
			newPrefab = wireDeadEndDownPrefab;
			newOrient = 1;
		}
		// 2 connections
		else if (HasConnections(true, true, false, false)){
			newPrefab = wireDownLeftCornerPrefab;
			newOrient = 3;
		}	
		else if (HasConnections(false, true, true, false)){
			newPrefab = wireDownLeftCornerPrefab;
			newOrient = 2;
		}
		else if (HasConnections(false, false, true, true)){
			newPrefab = wireDownLeftCornerPrefab;
			newOrient = 1;
		}	
		else if (HasConnections(true, false, false, true)){
			newPrefab = wireDownLeftCornerPrefab;
			newOrient = 0;
		}									
		else if (HasConnections(true, false, true, false)){
			newPrefab = wireStraightDownPrefab;
			newOrient = 0;
		}
		else if (HasConnections(false, true, false, true)){
			newPrefab = wireStraightDownPrefab;
			newOrient = 1;
		}	
		// 3 connections
		else if (HasConnections(true, true, true, false)){
			newPrefab = wireTJuncFromTopPrefab;
			newOrient = 3;
		}		
		else if (HasConnections(false, true, true, true)){
			newPrefab = wireTJuncFromTopPrefab;
			newOrient = 2;
		}	
		else if (HasConnections(true, false, true, true)){
			newPrefab = wireTJuncFromTopPrefab;
			newOrient = 1;
		}	
		else if (HasConnections(true, true, false, true)){
			newPrefab = wireTJuncFromTopPrefab;
			newOrient = 0;
		}
		// 4 connections
		else if (HasConnections(true, true, true, true)){
			newPrefab = wireCrossPrefab;
			newOrient = 0;
		}														
		else{
			Debug.LogError ("Unknown junction type");
			// But set up something to draw anyway
			newPrefab = wirePointPrefab;
			newOrient = 0;
		}	
		
		
		if (newPrefab != currentPrefab){
			GameObject.Destroy(currentDisplay);
			currentPrefab = newPrefab;
			currentDisplay = Instantiate(currentPrefab, gameObject.transform.position, Quaternion.Euler(0, 0, newOrient * 90)) as GameObject;
			currentDisplay.transform.parent = transform;
		}
		else if (newOrient != orient){
			orient = newOrient;
			currentDisplay.transform.rotation = Quaternion.Euler(0, 0, newOrient * 90);
		}
	}	
	
	void Update(){
		SetupMesh ();
	}
	
	void OnDestroy() {
		// When the object is destoryed, we must make sure to dispose of any meshes we may have
		Debug.Log ("OnDestroy");
		
	}	
	
	public override float GetResistance(int dir){
		if (!isConnected[dir]) Debug.LogError("Being asked about a nonexistanct connection");
		return resistance;
	}		
	
	
	



}
