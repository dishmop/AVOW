using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AVOWUIDisabledTool :  AVOWUITool{
	
	
	// New attempt at encoding the state of the UI
	Vector3 				mouseWorldPos;
	GameObject 				cursorCube;
	
	public override void Start(){
		cursorCube = InstantiateCursorCube();
		cursorCube.transform.parent = AVOWUI.singleton.transform;
		
		// Remove the metal material from the cube that we have
		RemoveMetal(cursorCube);
		uiZPos = AVOWUI.singleton.transform.position.z;
		
	}
	
	public override void OnDestroy(){
		GameObject.Destroy(cursorCube);
	}
	
	
	
	public override void Update () {
		//		Debug.Log(Time.time + ": UICreateTool Update");
		// Calc the mouse posiiton on world spave
		Vector3 mousePos = Input.mousePosition;
		mousePos.z = 0;
		mouseWorldPos = Camera.main.ScreenToWorldPoint( mousePos);
		mouseWorldPos.z = uiZPos;
		cursorCube.transform.position = mouseWorldPos;
		
		
	}
	
	protected override GameObject InstantiateCursorCube(){
		return AVOWUI.singleton.InstantiateGreenCursorCube();
	}
	
	
	
}